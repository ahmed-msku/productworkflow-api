using ProductWorkflow.API.Models;

namespace ProductWorkflow.API.Repositories.Interfaces
{
    public interface IJobRepository
    {
        Task<ProcessingJob> AddJob(ProcessingJob job);
        Task<ProcessingJob> GetJobAsync(Guid jobId);
    }
}
