using ProductWorkflow.API.Models;
using ProductWorkflow.API.Repositories.Interfaces;

namespace ProductWorkflow.API.Services
{
    public class JobService
    {
        private readonly IJobRepository _repository;
        private readonly ILogger _logger;

        public JobService(IJobRepository repository, ILogger<JobService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ProcessingJob> AddJob(ProcessingJob job)
        {
            return await _repository.AddJob(job);
        }

        public async Task<ProcessingJob> GetJobAsync(Guid jobId)
        {
            return await _repository.GetJobAsync(jobId);
        }

    }
}
