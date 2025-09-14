namespace InterviewsApplication.DTOs
{
    public class InterviewScheduleResponseDto
    {
        public int ScheduleID { get; set; }
        public DateTime InterviewDate { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public string CreatedBy { get; set; }
        public string? AgentName { get; set; }
        public string? MailSubject { get; set; }
    }
}
