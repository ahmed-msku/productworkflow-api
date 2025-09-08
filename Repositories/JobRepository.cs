using Microsoft.EntityFrameworkCore;
using ProductWorkflow.API.Models;
using ProductWorkflow.API.Repositories.Interfaces;
using ProductWorkflow.Data;

namespace ProductWorkflow.API.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public JobRepository(IDbContextFactory<AppDbContext> contextFactory)
            => _dbContextFactory = contextFactory;

        public async Task<ProcessingJob> AddJob(ProcessingJob job)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            dbContext.ProcessingJob.Add(job);
            await dbContext.SaveChangesAsync();

            return job;
        }

        public async Task<ProcessingJob?> GetJobAsync(Guid jobId)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            return await dbContext.ProcessingJob.FindAsync(jobId);
        }

        public async Task<ProcessingJob?> GetPendingJobAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            return await dbContext.ProcessingJob
                    .Where(j => j.Status == "Pending")
                    .OrderBy(j => j.CreatedAt)
                    .FirstOrDefaultAsync();
        }

        public async Task UpdateJob(ProcessingJob job)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.ProcessingJob.Update(job);
            await dbContext.SaveChangesAsync();
        }

    }
}
