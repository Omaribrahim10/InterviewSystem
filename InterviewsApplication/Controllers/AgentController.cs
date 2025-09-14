using InterviewsApplication.Data;
using InterviewsApplication.DTOs;
using InterviewsApplication.Interfaces;
using InterviewsApplication.Models;
using InterviewsApplication.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogService _logger;

    public AgentController(AppDbContext context, UserManager<ApplicationUser> userManager, ILogService logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var agents = await _userManager.Users
            .Include(a => a.Department)
            .Select(a => new AgentViewDto
            {
                AgentID = a.Id,
                FullName = a.FullName,
                Email = a.Email,
                Role = a.Role.ToString(),
                DepartmentName = a.Department.Name
            })
            .ToListAsync();

        await _logger.LogAsync(HttpContext, "ApplicationUser", "Fetched all agents");

        return Ok(agents);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var agent = await _userManager.Users
            .Include(a => a.Department)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agent == null) return NotFound();

        var agentDto = new AgentViewDto
        {
            AgentID = agent.Id,
            FullName = agent.FullName,
            Email = agent.Email,
            Role = agent.Role.ToString(),
            DepartmentName = agent.Department.Name
        };

        await _logger.LogAsync(HttpContext, "ApplicationUser", $"Fetched agent with ID {id}");

        return Ok(agentDto);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create(AgentDto dto)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (User.IsInRole("SuperAdmin"))
        {
        }
        else if (User.IsInRole("Admin"))
        {
            if (dto.Role != RoleEnum.Agent)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You are not allowed to assign this role."
                });
            }

            if (dto.DepartmentID != currentUser.DepartmentID)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only create agents in your own department."
                });
            }
        }
        else
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "You are not allowed to perform this action."
            });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            Role = dto.Role,
            DepartmentID = dto.DepartmentID
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, dto.Role.ToString());
        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, dto.Role.ToString()));

        var department = await _context.Departments.FindAsync(dto.DepartmentID);

        var agentDto = new AgentViewDto
        {
            AgentID = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            DepartmentName = department?.Name
        };

        await _logger.LogAsync(HttpContext, "ApplicationUser", $"Created new agent {user.Email}");

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, agentDto);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _logger.LogAsync(HttpContext, "ApplicationUser", $"Deleted agent with ID {id}");

        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            return BadRequest("New passwords do not match.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return NotFound("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _logger.LogAsync(HttpContext, "ApplicationUser", $"User {user.Email} changed password");

        return Ok("Password changed successfully.");
    }
}
