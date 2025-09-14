namespace InterviewsApplication.Models
{
    public class LogEntry
    {
        public int LogEntryId { get; set; }

        public string? UserId { get; set; }      
        public string? UserEmail { get; set; }    
        public string? Controller { get; set; }     
        public string? Action { get; set; }
        public string? TableName { get; set; }
        public string? Method { get; set; }
        public string? Description { get; set; }   
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }   
    }
}
