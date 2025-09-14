using InterviewsApplication.Models.Enums;

namespace InterviewsApplication.DTOs
{
    public class StudentDataResponseDto
    {
        public string UniversityID { get; set; }
        public string ReferralSource { get; set; }
        public string? Activities { get; set; }
        public string? Awards { get; set; }
        public string ImageAttach { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; }
        public bool IsLocked { get; set; }
        public int? MailID { get; set; }
        public MailResultEnum? MailResult { get; set; }
    }
}
