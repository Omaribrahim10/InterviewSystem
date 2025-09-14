using InterviewsApplication.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewsApplication.Models
{
    public class InterviewHistory
    {
        [Key]
        public int HistoryID { get; set; }
        public string UniversityID { get; set; }
        public int DepartmentID { get; set; }
        public InterviewStatusEnum InterviewStatus { get; set; }
        public DateTime Timestamp { get; set; }

        public string ReviewedBy { get; set; }

        [ForeignKey("ReviewedBy")]

        public ApplicationUser Agent { get; set; }

        [ForeignKey("DepartmentID")]
        public Department Department { get; set; }

        [ForeignKey("UniversityID")]
        public TestStudent Student { get; set; }
    }
}
