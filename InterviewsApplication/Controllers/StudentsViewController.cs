using InterviewsApplication.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InterviewsApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsViewController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentsViewController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize]

        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.TestStudents.ToListAsync();
            return Ok(students);
        }



        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(string id)
        {
            var student = await _context.TestStudents
                .FirstOrDefaultAsync(s => s.UniversityID == id);

            if (student == null)
                return NotFound("Student not found");

            return Ok(student);
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        public async Task<IActionResult> GetLoggedInStudent()
        {
            var universityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("🔑 Claims received:");
            foreach (var c in User.Claims)
                Console.WriteLine($"🔹 {c.Type}: {c.Value}");

            if (string.IsNullOrWhiteSpace(universityId))
                return Unauthorized("No university ID in claims");

            var student = await _context.TestStudents
                .FirstOrDefaultAsync(s => s.UniversityID == universityId);

            if (student == null)
                return NotFound("Student not found");

            return Ok(student);
        }


        [Authorize]
        [HttpGet("new")]
        public async Task<IActionResult> GetNewStudents()
        {
            var submittedIDs = await _context.StudentsData
                .Select(s => s.UniversityID)
                .ToListAsync();

            var students = await _context.TestStudents
                .Where(ts => !submittedIDs.Contains(ts.UniversityID))
                .ToListAsync();

            return Ok(students);
        }






    }
}
