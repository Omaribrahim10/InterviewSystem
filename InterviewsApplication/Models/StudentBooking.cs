using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewsApplication.Models
{
    public class StudentBooking
    {
        [Key]
        public int BookingID { get; set; }
        public string UniversityID { get; set; }
        public int ScheduleID { get; set; }
        public DateTime BookedAt { get; set; }

        [ForeignKey("UniversityID")]
        public TestStudent Student { get; set; }

        [ForeignKey("ScheduleID")]
        public InterviewSchedule InterviewSchedule { get; set; }
    }
}
