using InterviewsApplication.DTOs;
using InterviewsApplication.Services;
using InterviewsApplication.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TestEmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public TestEmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpGet("send-test")]
    public async Task<IActionResult> SendTestEmail()
    {
        var to = "YourEmailAddress@.."; //PUT YOUR EMAIL HERE 
        var subject = "📧 Test Email from Interview System";
        var body = "Hello ,\n\nThis is a test email sent from your ASP.NET Core project.\n\nRegards,\nYour App";

        await _emailService.SendEmailAsync(to, subject, body);

        return Ok("Test email sent.");
    }

    [Authorize(Roles = "SuperAdmin,Admin,Agent")]
    [HttpPost("send-fixed-email")]
    public async Task<IActionResult> SendFixedEmail([FromBody] SendDataEmailDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email address is required.");

        var subject = "Interview Booking Instructions";
        var body = @"
    <p>Dear student,</p>
    <p><strong>To complete your data, please visit:</strong></p>
    <p><a href='https://localhost:7286/student-login.html'>Click here to complete your data</a></p>
    <br>
    <p>Best regards,<br>Your University Team</p>
";


        try
        {
            await _emailService.SendEmailAsync(
                dto.Email,
                subject,
                body,
                true
            );

            return Ok("Fixed email sent successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest("Failed to send email: " + ex.Message);
        }
    }

}
