using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewsApplication.Models
{
    public class InterviewSchedule
    {
        [Key]
        public int ScheduleID { get; set; }
        public DateTime InterviewDate { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MailID { get; set; }

        [ForeignKey("MailID")]
        public MailingContent MailingContent { get; set; }

        [ForeignKey("CreatedBy")]
        public ApplicationUser Agent { get; set; }

        public ICollection<StudentBooking> StudentBookings { get; set; }
    }
}
