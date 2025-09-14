using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InterviewsApplication.DTOs
{
        public class StudentLoginResponseDto
        {
            public string UniversityID { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
        }
}
