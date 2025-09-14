using InterviewsApplication.Data;
using InterviewsApplication.Interfaces;
using InterviewsApplication.Models;
using InterviewsApplication.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize(Roles = "SuperAdmin,Admin,Agent")]
[ApiController]
[Route("api/[controller]")]
public class InterviewHistoryController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogService _logger;

    public InterviewHistoryController(AppDbContext context, ILogService logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllInterviewHistories()
    {
        var allHistories = await _context.InterviewHistories
            .Include(h => h.Department)
            .Include(h => h.Agent)
            .OrderByDescending(h => h.Timestamp)
            .Select(h => new
            {
                h.HistoryID,
                h.UniversityID,
                Department = h.Department.Name,
                h.InterviewStatus,
                h.Timestamp,
                Agent = h.Agent.FullName
            })
            .ToListAsync();

        await _logger.LogAsync(HttpContext, "InterviewHistory", "Fetched all interview histories");
        return Ok(allHistories);
    }

    [HttpGet("student/{universityId}")]
    public async Task<IActionResult> GetStudentInterviewHistory(string universityId)
    {
        var history = await _context.InterviewHistories
            .Include(h => h.Department)
            .Include(h => h.Agent)
            .Where(h => h.UniversityID == universityId)
            .OrderBy(h => h.DepartmentID)
            .Select(h => new
            {
                h.UniversityID,
                Department = h.Department.Name,
                h.InterviewStatus,
                h.Timestamp,
                Agent = h.Agent.FullName
            })
            .ToListAsync();

        await _logger.LogAsync(HttpContext, "InterviewHistory", $"Viewed interview history for student {universityId}");
        return Ok(history);
    }

    [HttpPost("mark")]
    public async Task<IActionResult> MarkInterviewStatus(string universityId, InterviewStatusEnum status)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var agent = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (agent == null)
            return Unauthorized("Agent not found");

        var departmentId = agent.DepartmentID;

        var existing = await _context.InterviewHistories
            .FirstOrDefaultAsync(h => h.UniversityID == universityId && h.DepartmentID == departmentId);

        if (existing != null)
        {
            existing.InterviewStatus = status;
            existing.Timestamp = DateTime.UtcNow;
        }
        else
        {
            var newEntry = new InterviewHistory
            {
                UniversityID = universityId,
                DepartmentID = departmentId,
                InterviewStatus = status,
                Timestamp = DateTime.UtcNow,
                ReviewedBy = userId
            };
            _context.InterviewHistories.Add(newEntry);
        }

        await _context.SaveChangesAsync();

        await _logger.LogAsync(HttpContext, "InterviewHistory", $"Marked student {universityId} as {status} in department {agent.Department?.Name}");

        return Ok(new { message = $"Student marked as {status} for {agent.Department?.Name ?? "Unknown Department"}" });
    }
}
