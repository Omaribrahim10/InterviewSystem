namespace InterviewsApplication.DTOs
{
    public class StudentDataDto
    {
        public string UniversityID { get; set; }
        public string ReferralSource { get; set; }
        public string? Activities { get; set; }
        public string? Awards { get; set; }
        public IFormFile ImageAttach { get; set; }
    }
}
