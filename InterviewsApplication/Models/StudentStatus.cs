using InterviewsApplication.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewsApplication.Models
{
    public class StudentStatus
    {
        [Key]
        public int StatusID { get; set; }
        public string UniversityID { get; set; }
        public StatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public bool IsLocked { get; set; } = false;

        [ForeignKey("ReviewedBy")]
        public ApplicationUser Agent { get; set; }

        [ForeignKey("UniversityID")]
        public TestStudent Student { get; set; }


    }
}
