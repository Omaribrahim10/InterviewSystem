using InterviewsApplication.Data;
using InterviewsApplication.DTOs;
using InterviewsApplication.Interfaces;
using InterviewsApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class MailingContentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogService _logService;

    public MailingContentController(AppDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var mails = await _context.MailingContents
            .Include(m => m.Agent)
            .Select(m => new
            {
                m.MailID,
                m.Subject,
                m.Body,
                m.IsDefault,
                m.CreatedBy,
                AgentName = m.Agent != null ? m.Agent.FullName : null
            })
            .ToListAsync();

        return Ok(mails);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var mail = await _context.MailingContents
            .Include(m => m.Agent)
            .Include(m => m.TargetedStudents)
            .Include(m => m.InterviewSchedules)
            .FirstOrDefaultAsync(m => m.MailID == id);

        if (mail == null) return NotFound();

        return Ok(new
        {
            mail.MailID,
            mail.Subject,
            mail.Body,
            mail.IsDefault,
            mail.CreatedBy,
            AgentName = mail.Agent?.FullName,
            mail.TargetedStudents,
            mail.InterviewSchedules
        });
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> CreateMailingContent([FromBody] MailingContentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized("User not authenticated.");

        var mail = new MailingContent
        {
            Subject = dto.Subject,
            Body = dto.Body,
            IsDefault = false,
            CreatedBy = userId
        };

        _context.MailingContents.Add(mail);
        await _context.SaveChangesAsync();

        await _logService.LogAsync(HttpContext, "MailingContent", $"Created mailing content with Subject: {mail.Subject}");

        return Ok(new
        {
            mail.MailID,
            mail.Subject,
            mail.Body,
            mail.IsDefault,
            mail.CreatedBy
        });
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MailingContentDto dto)
    {
        if (dto == null) return BadRequest();

        var mail = await _context.MailingContents
            .Include(m => m.Agent)
            .FirstOrDefaultAsync(m => m.MailID == id);

        if (mail == null) return NotFound();

        mail.Subject = dto.Subject;
        mail.Body = dto.Body;
        mail.IsDefault = false;

        await _context.SaveChangesAsync();

        await _logService.LogAsync(HttpContext, "MailingContent", $"Updated mailing content with ID: {id}");

        return Ok(new
        {
            mail.MailID,
            mail.Subject,
            mail.Body,
            mail.IsDefault,
            mail.CreatedBy,
            AgentName = mail.Agent?.FullName
        });
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("setdefault/{id}")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var mail = await _context.MailingContents.FindAsync(id);
        if (mail == null) return NotFound();

        if (mail.IsDefault) return NoContent();

        await ResetDefaultMailAsync();

        mail.IsDefault = true;
        await _context.SaveChangesAsync();

        await _logService.LogAsync(HttpContext, "MailingContent", $"Set mailing content ID {id} as default");

        return NoContent();
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var mail = await _context.MailingContents.FindAsync(id);
        if (mail == null) return NotFound();

        _context.MailingContents.Remove(mail);
        await _context.SaveChangesAsync();

        await _logService.LogAsync(HttpContext, "MailingContent", $"Deleted mailing content with ID: {id}");

        return NoContent();
    }

    private async Task ResetDefaultMailAsync()
    {
        var defaultMails = await _context.MailingContents.Where(m => m.IsDefault).ToListAsync();
        foreach (var m in defaultMails)
        {
            m.IsDefault = false;
        }
        await _context.SaveChangesAsync();
    }
}
