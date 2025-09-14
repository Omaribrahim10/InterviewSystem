using InterviewsApplication.Data;
using InterviewsApplication.DTOs;
using InterviewsApplication.Interfaces;
using InterviewsApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class InterviewScheduleController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogService _logger;

    public InterviewScheduleController(AppDbContext context, UserManager<ApplicationUser> userManager, ILogService logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var schedules = await _context.InterviewSchedules
            .Include(s => s.Agent)
            .Include(s => s.MailingContent)
            .Select(s => new InterviewScheduleResponseDto
            {
                ScheduleID = s.ScheduleID,
                InterviewDate = s.InterviewDate,
                Capacity = s.Capacity,
                Location = s.Location,
                CreatedBy = s.CreatedBy,
                AgentName = s.Agent != null ? s.Agent.FullName : null,
                MailSubject = s.MailingContent != null ? s.MailingContent.Subject : null
            })
            .ToListAsync();

        await _logger.LogAsync(HttpContext, "InterviewSchedule", "Fetched all interview schedules");
        return Ok(schedules);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var schedule = await _context.InterviewSchedules
            .Include(s => s.Agent)
            .Include(s => s.MailingContent)
            .Where(s => s.ScheduleID == id)
            .Select(s => new InterviewScheduleResponseDto
            {
                ScheduleID = s.ScheduleID,
                InterviewDate = s.InterviewDate,
                Capacity = s.Capacity,
                Location = s.Location,
                CreatedBy = s.CreatedBy,
                AgentName = s.Agent != null ? s.Agent.FullName : null,
                MailSubject = s.MailingContent != null ? s.MailingContent.Subject : null
            })
            .FirstOrDefaultAsync();

        if (schedule == null)
            return NotFound();

        await _logger.LogAsync(HttpContext, "InterviewSchedule", $"Viewed interview schedule with ID {id}");
        return Ok(schedule);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
        public async Task<IActionResult> CreateInterviewSchedule([FromBody] InterviewScheduleDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (dto.Capacity.HasValue && dto.Capacity.Value <= 0)
                return BadRequest("Interview capacity must be greater than 0.");

            var defaultMail = await _context.MailingContents.FirstOrDefaultAsync(m => m.IsDefault);
            if (defaultMail == null)
                return BadRequest("No default mailing content found.");
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"));

            var schedule = new InterviewSchedule
            {
                InterviewDate = dto.InterviewDate.Date,
                Capacity = dto.Capacity ?? 120,
                Location = string.IsNullOrWhiteSpace(dto.Location) ? "D126" : dto.Location,
                CreatedBy = userId,
                MailID = defaultMail.MailID,
                CreatedAt = egyptTime
            };

            _context.InterviewSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            await _logger.LogAsync(HttpContext, "InterviewSchedule", $"Created new interview schedule on {schedule.InterviewDate}");

            return Ok(new
            {
                schedule.ScheduleID,
                schedule.InterviewDate,
                schedule.Capacity,
                schedule.Location,
                schedule.CreatedBy,
                schedule.MailID
            });
        }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] InterviewScheduleDto dto)
    {
        var schedule = await _context.InterviewSchedules.FindAsync(id);
        if (schedule == null)
            return NotFound("Schedule not found.");

        var bookedCount = await _context.StudentBookings.CountAsync(sb => sb.ScheduleID == id);

        if (dto.Capacity.HasValue && dto.Capacity.Value < bookedCount)
            return BadRequest($"Cannot set capacity to {dto.Capacity.Value}. There are already {bookedCount} students booked.");

        if (!dto.Capacity.HasValue)
            schedule.Capacity = 120;
        else if (dto.Capacity <= 0)
            return BadRequest("Interview capacity must be greater than 0.");
        else
            schedule.Capacity = dto.Capacity.Value;
            schedule.InterviewDate = dto.InterviewDate.Date;
            schedule.Location = dto.Location;

        await _context.SaveChangesAsync();

        await _logger.LogAsync(HttpContext, "InterviewSchedule", $"Updated interview schedule ID {id}");

        return Ok(new
        {
            schedule.ScheduleID,
            schedule.InterviewDate,
            schedule.Capacity,
            schedule.Location,
            schedule.CreatedBy,
            schedule.MailID
        });
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var schedule = await _context.InterviewSchedules.FindAsync(id);
        if (schedule == null)
            return NotFound();

        _context.InterviewSchedules.Remove(schedule);
        await _context.SaveChangesAsync();

        await _logger.LogAsync(HttpContext, "InterviewSchedule", $"Deleted interview schedule ID {id}");
        return NoContent();
    }
}
