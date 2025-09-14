using System.ComponentModel.DataAnnotations;

namespace InterviewsApplication.Models
{
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }
        public string Name { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<InterviewHistory> InterviewHistories { get; set; }

        public ICollection<InterviewResult> InterviewResults { get; set; } = new List<InterviewResult>();

    }
}
