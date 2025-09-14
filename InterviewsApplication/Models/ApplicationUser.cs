using Microsoft.AspNetCore.Identity;
using InterviewsApplication.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewsApplication.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public RoleEnum Role { get; set; }
        public int DepartmentID { get; set; }

        [ForeignKey("DepartmentID")]
        public Department Department { get; set; }

        public ICollection<StudentStatus> StudentStatus { get; set; }
        public ICollection<InterviewSchedule> InterviewSchedule { get; set; }
        public ICollection<MailingContent> MailingContent { get; set; }
        public ICollection<InterviewHistory> InterviewHistories { get; set; }
        public ICollection<InterviewResult> InterviewResults { get; set; } = new List<InterviewResult>();

    }
}
