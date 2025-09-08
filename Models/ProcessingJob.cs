namespace ProductWorkflow.API.Models
{
    public class ProcessingJob
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; }
        public string Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalAdded { get; set; }
        public int TotalFailed { get; set; }    
        public string? ErrorMessage { get; set; }
    }
}
