using InterviewsApplication.Models.Enums;

namespace InterviewsApplication.DTOs
{
    public class UpdateStatusDto
    {
        public string UniversityId { get; set; }
        public StatusEnum NewStatus { get; set; }
    }
}
