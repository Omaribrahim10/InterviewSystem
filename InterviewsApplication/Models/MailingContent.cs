using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewsApplication.Models
{
    public class MailingContent
    {
        [Key]
        public int MailID { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsDefault { get; set; }
        public string CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public ApplicationUser Agent { get; set; }
        public ICollection<StudentsData> TargetedStudents { get; set; }
        public ICollection<InterviewSchedule> InterviewSchedules { get; set; }
    }
}
