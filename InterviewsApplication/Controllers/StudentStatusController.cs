using InterviewsApplication.Data;
using InterviewsApplication.Models.Enums;
using InterviewsApplication.Models;
using InterviewsApplication.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using InterviewsApplication.DTOs;

[Authorize(Roles = "SuperAdmin,Agent,Admin")]
[ApiController]
[Route("api/[controller]")]
public class StudentStatusController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public StudentStatusController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDto dto)
    {
        var universityId = dto.UniversityId;
        var newStatus = dto.NewStatus;
        var latestStatus = await _context.StudentStatuses
            .Where(s => s.UniversityID == universityId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestStatus != null && latestStatus.Status != StatusEnum.New && latestStatus.Status != StatusEnum.Pending)
        {
            return BadRequest("Only students in 'New' or 'Pending' status can be updated.");
        }

        var reviewedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var statusRecord = new StudentStatus
        {
            UniversityID = universityId,
            Status = newStatus,
            CreatedAt = DateTime.UtcNow,
            ReviewedBy = reviewedBy,
            IsLocked = false
        };


        if (newStatus == StatusEnum.Fulfilled)
        {

            statusRecord.IsLocked = true;

            var defaultMail = await _context.MailingContents.FirstOrDefaultAsync(m => m.IsDefault);
            if (defaultMail == null)
                return BadRequest("No default email template found.");

            var student = await _context.TestStudents.FirstOrDefaultAsync(s => s.UniversityID == universityId);
            if (student == null || string.IsNullOrWhiteSpace(student.Email))
                return BadRequest("Student email not found.");

            var bookingUrl = "https://localhost:7286/student-login-booking.html";

            var baseBody = defaultMail.Body ?? "You are eligible to book your interview.";
            if (!baseBody.Contains(bookingUrl))
            {
                baseBody += $"<br><br><strong>To book your interview, please visit:</strong> <a href='{bookingUrl}'>Click here to book your interview</a>";
            }

            var mailResult = MailResultEnum.Failed;

            try
            {
                await _emailService.SendEmailAsync(
                    student.Email,
                    defaultMail.Subject ?? "Interview Booking Info",
                    baseBody,
                    isHtml: true 
                );

                mailResult = MailResultEnum.Sent;
            }
            catch
            {
                // Optionally log error here
            }

            var studentData = await _context.StudentsData
                .FirstOrDefaultAsync(s => s.UniversityID == universityId);

            if (studentData != null)
            {
                studentData.MailID = defaultMail.MailID;
                studentData.MailResult = mailResult;
            }
        }
        _context.StudentStatuses.Add(statusRecord);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"Student status updated to {newStatus}",
            reviewedBy
        });
    }


    [Authorize(Roles = "SuperAdmin,Admin,Agent")]
    [HttpPost("resend-email")]
    public async Task<IActionResult> ResendEmail([FromBody] ResendEmailDto dto)
    {
        var student = await _context.TestStudents
            .FirstOrDefaultAsync(s => s.UniversityID == dto.UniversityID);

        if (student == null || string.IsNullOrWhiteSpace(student.Email))
            return BadRequest("Student email not found.");

        var mail = dto.MailID.HasValue
            ? await _context.MailingContents.FindAsync(dto.MailID.Value)
            : await _context.MailingContents.FirstOrDefaultAsync(m => m.IsDefault);

        if (mail == null)
            return BadRequest("Mail content not found.");

        var baseBody = mail.Body ?? "You are eligible to book your interview.";
        var bookingUrl = "https://localhost:7286/student-login-booking.html";

        if (!baseBody.Contains(bookingUrl))
        {
            baseBody += $"<br><br><strong>To book your interview, please visit:</strong> <a href='{bookingUrl}'>{bookingUrl}</a>";
        }

        try
        {
            await _emailService.SendEmailAsync(
                student.Email,
                mail.Subject ?? "Interview Booking Info",
                baseBody,
                true
            );

            var studentData = await _context.StudentsData
                .FirstOrDefaultAsync(s => s.UniversityID == dto.UniversityID);

            if (studentData != null)
            {
                studentData.MailID = mail.MailID;
                studentData.MailResult = MailResultEnum.Sent;
            }

            await _context.SaveChangesAsync();
            return Ok("Email resent successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest("Failed to send email: " + ex.Message);
        }

    }


    [HttpGet]
    public async Task<IActionResult> GetAllStatuses()
    {
        var statuses = await _context.StudentStatuses.ToListAsync();
        return Ok(statuses);
    }

    [Authorize(Roles = "SuperAdmin,Admin,Agent")]
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestStatuses()
    {
        var latestStatuses = await _context.StudentStatuses
            .GroupBy(s => s.UniversityID)
            .Select(g => g.OrderByDescending(s => s.CreatedAt).FirstOrDefault())
            .ToListAsync();

        return Ok(latestStatuses);
    }

    [Authorize]
    [HttpGet("status/latest/{universityId}")]
    public async Task<IActionResult> GetStudentLatestStatus(string universityId)
    {
        var latestStatus = await _context.StudentStatuses
            .Where(s => s.UniversityID == universityId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestStatus == null)
            return NotFound("No status found for this student.");

        return Ok(new
        {
            status = latestStatus.Status.ToString(),
            isLocked = latestStatus.IsLocked
        });
    }


}
