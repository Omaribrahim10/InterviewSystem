using InterviewsApplication.Interfaces;
using InterviewsApplication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogService _logger;

    public LoginController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogService logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, isPersistent: false, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            await _logger.LogAsync(HttpContext, "Authentication", $"Failed login attempt for {dto.Email}");
            return Unauthorized("Invalid email or password");
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);

        await _logger.LogAsync(HttpContext, "Authentication", $"User {user.Email} logged in");
        return Ok("Logged in successfully");
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        await _logger.LogAsync(HttpContext, "Authentication", $"User {email ?? userId} logged out");
        return Ok("Logged out");
    }
}
