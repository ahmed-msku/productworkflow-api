using ProductWorkflow.API.Models;
using System.Threading.Channels;
using ProductWorkflow.Data;
using Microsoft.EntityFrameworkCore;
using ProductWorkflow.API.Repositories.Interfaces;

namespace ProductWorkflow.API.Services
{
    public class FileProcessingService : BackgroundService
    {
        private readonly IProductRepository _productRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ILogger<FileProcessingService> _logger;
        private const int ERRORS_LIMIT = 10;
        private const int BATCH_SIZE = 10;
        private const int SERVICE_DELAY = 2000; // 2s delay
        private const int TASK_DELAY = 3500;    // 3.5s as per requirement

        public FileProcessingService(IProductRepository productRepository, IJobRepository jobRepository, ILogger<FileProcessingService> logger)
        {
            _productRepository = productRepository;
            _jobRepository = jobRepository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var job = await _jobRepository.GetPendingJobAsync();

                if (job == null)
                {
                    await Task.Delay(SERVICE_DELAY, cancellationToken);
                    continue;
                }

                job.Status = "Processing";
                await _jobRepository.UpdateJob(job);
                _logger.LogInformation("Job processing started");

                var channel = Channel.CreateBounded<Product>(100);
                int totalAdded = 0;
                int totalErrors = 0;

                try
                {
                    // Producer Task: Read CSV line by line
                    var producer = Task.Run(async () =>
                    {
                        using var fs = new FileStream(job.FilePath, FileMode.Open, FileAccess.Read);
                        using var reader = new StreamReader(fs);

                        int lineNumber = 0;
                        string? line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            lineNumber++;

                            // Skip header 
                            if (lineNumber == 1 && line.StartsWith("Name,Description")) continue;

                            var product = ValidateLine(line, out string? error);
                            if (product != null)
                            {
                                totalAdded++;
                                await channel.Writer.WriteAsync(product, cancellationToken);
                            }
                            else
                            {
                                totalErrors++;

                                _logger.LogWarning("Job {JobId}: Error in data found: {error}", job.Id, error);

                                if (totalErrors > ERRORS_LIMIT)
                                {
                                    _logger.LogWarning("Job {JobId}: Exceeded allowed failure limit", job.Id);
                                    break;
                                }
                            }
                        }

                        channel.Writer.Complete();
                    }, cancellationToken);

                    // Consumer Tasks: Start exactly two threads to process
                    var consumers = new List<Task>();
                    for (int i = 0; i < 2; i++)
                    {
                        consumers.Add(Task.Run(async () =>
                        {
                            var batch = new List<Product>();

                            await foreach (var product in channel.Reader.ReadAllAsync(cancellationToken))
                            {
                                await Task.Delay(TASK_DELAY, cancellationToken); // As per requirement

                                batch.Add(product);

                                if (batch.Count >= BATCH_SIZE) // Bulk insert
                                {
                                    await _productRepository.AddBatch(batch);
                                    batch.Clear();
                                    _logger.LogInformation("Products added");
                                }
                            }

                            if (batch.Any())
                            {
                                await _productRepository.AddBatch(batch);
                            }
                        }, cancellationToken));
                    }

                    await producer;
                    await Task.WhenAll(consumers);

                    job.Status = totalErrors > ERRORS_LIMIT ? "Failed" : "Completed";
                    job.CompletedAt = DateTime.Now;
                    job.TotalAdded = totalAdded;
                    job.TotalFailed = totalErrors;

                    await _jobRepository.UpdateJob(job);
                    _logger.LogInformation("Job completed");
                }
                catch (Exception ex)
                {
                    job.Status = "Failed";
                    job.ErrorMessage = ex.Message;
                    job.CompletedAt = DateTime.Now;
                    await _jobRepository.UpdateJob(job);
                    _logger.LogError(ex, "Error processing job {JobId}", job.Id);
                }
            }
        }

        private Product? ValidateLine(string line, out string? error)
        {
            error = null;
            var parts = line.Split(',')
                .Select(p => p.Trim().Trim('"'))
                .ToArray();

            if (parts.Length != 4)
            {
                error = "Incorrect column count";
                return null;
            }

            if (string.IsNullOrWhiteSpace(parts[0]))
            {
                error = "Name is empty";
                return null;
            }

            if (!decimal.TryParse(parts[2], out decimal price))
            {
                error = "Invalid price";
                return null;
            }
            if(price<0)
            {
                error = "Invalid price";
                return null;
            }
            if (string.IsNullOrWhiteSpace(parts[3]))
            {
                error = "Category is empty";
                return null;
            }

            return new Product { Name = parts[0], Description=parts[1], Price = price, Category = parts[3] };
        }
    }

}
