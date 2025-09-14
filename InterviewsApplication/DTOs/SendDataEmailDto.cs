using System.ComponentModel.DataAnnotations;

namespace InterviewsApplication.DTOs
{
    public class SendDataEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
