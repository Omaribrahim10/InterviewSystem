using InterviewsApplication.Data;
using InterviewsApplication.DTOs;
using InterviewsApplication.Models;
using InterviewsApplication.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InterviewsApplication.Controllers
{



    [ApiController]
    [Route("api/[controller]")]
    public class StudentsDataController : ControllerBase
    {

        private StudentDataResponseDto MapStudentData(StudentsData data, StudentStatus latestStatus)
        {
            return new StudentDataResponseDto
            {
                UniversityID = data.UniversityID,
                ReferralSource = data.ReferralSource,
                Activities = data.Activities,
                Awards = data.Awards,
                ImageAttach = data.ImageAttach,
                SubmittedAt = data.SubmittedAt,
                Status = latestStatus.Status.ToString().ToLower(),
                IsLocked = latestStatus.IsLocked,
                MailID = data.MailID,
                MailResult = data.MailResult
            };
        }


        private readonly AppDbContext _context;

        public StudentsDataController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("info/{universityID}")]
        public async Task<IActionResult> GetStudentInfo(string universityID)
        {
            var data = await _context.StudentsData
                .FirstOrDefaultAsync(s => s.UniversityID == universityID);

            if (data == null)
                return NotFound("Student data not found.");

            var status = await _context.StudentStatuses
                .Where(s => s.UniversityID == universityID)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (status == null)
                return NotFound("Student status not found.");


            return Ok(MapStudentData(data, status));
        }



        [HttpGet("review/{universityID}")]
        public async Task<IActionResult> ReviewStudent(string universityID)
        {
            var data = await _context.StudentsData
                .FirstOrDefaultAsync(s => s.UniversityID == universityID);

            if (data == null)
                return NotFound("Student data not found.");

            var status = await _context.StudentStatuses
                .FirstOrDefaultAsync(s => s.UniversityID == universityID);

            if (status == null)
                return NotFound("Student status not found.");

            if (status.Status == StatusEnum.New && !status.IsLocked)
            {
                status.IsLocked = true;
                status.ReviewedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _context.SaveChangesAsync();
            }

            return Ok(MapStudentData(data, status));
        }


        [HttpPost]
        public async Task<IActionResult> SubmitStudentData([FromForm] StudentDataDto dto)
        {
            var existing = await _context.StudentsData.FindAsync(dto.UniversityID);
            if (existing != null)
                return BadRequest("Student data already submitted.");

            if (dto.ImageAttach == null || dto.ImageAttach.Length == 0)
                return BadRequest("Image is required.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(dto.ImageAttach.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.ImageAttach.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("uploads", fileName).Replace("\\", "/");

            var newEntry = new StudentsData
            {
                UniversityID = dto.UniversityID,
                ReferralSource = dto.ReferralSource,
                Activities = dto.Activities,
                Awards = dto.Awards,
                ImageAttach = relativePath,
                SubmittedAt = DateTime.UtcNow
            };

            var newStatus = new StudentStatus
            {
                UniversityID = dto.UniversityID,
                Status = StatusEnum.New,
                CreatedAt = DateTime.UtcNow,
                ReviewedBy = null,
                IsLocked = false
            };


            _context.StudentsData.Add(newEntry);
            _context.StudentStatuses.Add(newStatus);

            await _context.SaveChangesAsync();

            return Ok(MapStudentData(newEntry, newStatus));
        }




        [HttpPut]
        public async Task<IActionResult> EditStudentData([FromForm] StudentDataDto dto)
        {
            var existingData = await _context.StudentsData.FindAsync(dto.UniversityID);
            if (existingData == null)
                return NotFound("Student data not found.");

            var status = await _context.StudentStatuses
                .Where(s => s.UniversityID == dto.UniversityID)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (status == null)
                return NotFound("Student status not found.");

            if (status.Status == StatusEnum.Pending && status.IsLocked)
            {
                status.IsLocked = false;
                await _context.SaveChangesAsync();
            }

            if (status.IsLocked)
                return BadRequest("You cannot edit your data after it has been reviewed.");

            existingData.ReferralSource = dto.ReferralSource;
            existingData.Activities = dto.Activities;
            existingData.Awards = dto.Awards;
            existingData.SubmittedAt = DateTime.UtcNow;

            if (dto.ImageAttach != null && dto.ImageAttach.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.ImageAttach.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageAttach.CopyToAsync(stream);
                }

                var relativePath = Path.Combine("uploads", fileName).Replace("\\", "/");
                existingData.ImageAttach = relativePath;
            }

            await _context.SaveChangesAsync();

            var latestStatus = await _context.StudentStatuses
                .Where(s => s.UniversityID == dto.UniversityID)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            return Ok(MapStudentData(existingData, latestStatus));
        }

        

    }
}
