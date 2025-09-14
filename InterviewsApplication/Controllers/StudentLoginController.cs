using InterviewsApplication.Data;
using InterviewsApplication.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using InterviewsApplication.Models.Enums;


namespace InterviewsApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(StudentLoginDto dto)
        {
            var student = await _context.TestStudents
                .FirstOrDefaultAsync(s => s.UniversityID == dto.UniversityID);

            if (student == null || student.NationalID != dto.NationalID)
                return Unauthorized("Invalid credentials");

            if (student.IsPaid?.Trim().ToLower() == "no")
                return Unauthorized("Please review your financial status with the university.");

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, student.UniversityID),
        new Claim(ClaimTypes.Name, student.FullName ?? ""),
        new Claim(ClaimTypes.Email, student.Email ?? "")
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(1)
                });

            Console.WriteLine("✅ Cookie issued with UniversityID: " + student.UniversityID);

            return Ok(new
            {
                universityID = student.UniversityID,
                fullName = student.FullName,
                email = student.Email
            });
        }



        [HttpPost("Bookingslogin")]
        public async Task<IActionResult> BookingsLogin(StudentLoginDto dto)
        {
            var student = await _context.TestStudents
                .FirstOrDefaultAsync(s => s.UniversityID == dto.UniversityID);

            if (student == null || student.NationalID != dto.NationalID)
                return Unauthorized("Invalid credentials");


            var latestStatus = await _context.StudentStatuses
                .Where(s => s.UniversityID == dto.UniversityID)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestStatus == null || latestStatus.Status != StatusEnum.Fulfilled)
            {
                return Unauthorized("You are not yet eligible to book an interview. Please complete your data first.");
            }


           var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, student.UniversityID),
        new Claim(ClaimTypes.Name, student.FullName ?? ""),
        new Claim(ClaimTypes.Email, student.Email ?? "")
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(1)
                });

            Console.WriteLine("✅ Cookie issued with UniversityID: " + student.UniversityID);
            Console.WriteLine($"Student {dto.UniversityID} Latest Status: {latestStatus?.Status} at {latestStatus?.CreatedAt}");

            return Ok(new
            {
                universityID = student.UniversityID,
                fullName = student.FullName,
                email = student.Email
            });
        }



    }
}
