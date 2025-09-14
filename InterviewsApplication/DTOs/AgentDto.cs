using InterviewsApplication.Models.Enums;

namespace InterviewsApplication.DTOs
{
    public class AgentDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public RoleEnum Role { get; set; }
        public int DepartmentID { get; set; }
    }
}
