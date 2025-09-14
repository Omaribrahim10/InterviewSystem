using InterviewsApplication.Data;
using InterviewsApplication.DTOs;
using InterviewsApplication.Interfaces;
using InterviewsApplication.Models;
using InterviewsApplication.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

[ApiController]
[Route("api/[controller]")]
public class StudentBookingController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogService _logService;

    public StudentBookingController(AppDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    [HttpPost]
    public async Task<IActionResult> BookInterview([FromBody] StudentBookingDto dto)
    {
        var schedule = await _context.InterviewSchedules.FindAsync(dto.ScheduleID);
        if (schedule == null)
            return NotFound("Interview schedule not found.");

        var existingBookingsCount = await _context.StudentBookings
            .CountAsync(sb => sb.ScheduleID == dto.ScheduleID);

        if (existingBookingsCount >= schedule.Capacity)
            return BadRequest("The capacity for this interview day is full. Please choose another day.");

        var alreadyBooked = await _context.StudentBookings
            .AnyAsync(sb => sb.UniversityID == dto.UniversityID);

        if (alreadyBooked)
            return BadRequest("Student has already booked an interview.");

        var booking = new StudentBooking
        {
            UniversityID = dto.UniversityID,
            ScheduleID = dto.ScheduleID,
            BookedAt = DateTime.UtcNow
        };
        _context.StudentBookings.Add(booking);

        var newStatus = new StudentStatus
        {
            UniversityID = dto.UniversityID,
            Status = StatusEnum.Reserved,
            CreatedAt = DateTime.UtcNow,
            ReviewedBy = null,
            IsLocked = true
        };
        _context.StudentStatuses.Add(newStatus);

        await _context.SaveChangesAsync();

        await _logService.LogAsync(HttpContext, "StudentBooking", $"Student {dto.UniversityID} booked ScheduleID {dto.ScheduleID}");

        return Ok("Booking successful and status updated to Reserved.");
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllBookings()
    {
        var bookings = await _context.StudentBookings
            .Include(sb => sb.InterviewSchedule)
            .Include(sb => sb.InterviewSchedule.MailingContent)
            .Select(sb => new
            {
                sb.UniversityID,
                sb.ScheduleID,
                ScheduleDate = sb.InterviewSchedule.InterviewDate,
                sb.BookedAt
            })
            .ToListAsync();

        return Ok(bookings);
    }

    [HttpGet("{universityId}")]
    public async Task<IActionResult> GetBookingById(string universityId)
    {
        var booking = await _context.StudentBookings
            .Include(sb => sb.InterviewSchedule)
            .Where(sb => sb.UniversityID == universityId)
            .Select(sb => new
            {
                sb.UniversityID,
                sb.ScheduleID,
                ScheduleDate = sb.InterviewSchedule.InterviewDate,
                sb.BookedAt
            })
            .FirstOrDefaultAsync();

        if (booking == null)
            return NotFound("Booking not found.");

        return Ok(booking);
    }

    [HttpGet("todays-queue")]
    public async Task<IActionResult> TodaysQueue()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var todaysBookings = await _context.StudentBookings
            .Include(sb => sb.InterviewSchedule)
            .Where(sb => DateOnly.FromDateTime(sb.InterviewSchedule.InterviewDate) == today)
            .Select(sb => new
            {
                sb.UniversityID,
                sb.ScheduleID,
                ScheduleDate = sb.InterviewSchedule.InterviewDate,
                sb.BookedAt
            })
            .ToListAsync();

        return Ok(todaysBookings);
    }

    [HttpGet("pdf-data/{universityID}")]
    public async Task<ActionResult<PrintDataDto>> GetStudentPdfData(string universityID)
    {
        var student = await _context.TestStudents
            .Where(s => s.UniversityID == universityID)
            .FirstOrDefaultAsync();

        if (student == null)
            return NotFound("Student not found");

        var booking = await _context.StudentBookings
            .Where(b => b.UniversityID == universityID)
            .OrderByDescending(b => b.BookedAt)
            .FirstOrDefaultAsync();
        
        if (booking == null)
            return NotFound("Booking not found");

        var interviewDate = await _context.InterviewSchedules
            .Where(s => s.ScheduleID == booking.ScheduleID)
            .Select(s => s.InterviewDate)
            .FirstOrDefaultAsync();
        var location = await _context.InterviewSchedules
            .Where(s => s.ScheduleID == booking.ScheduleID)
            .Select(s => s.Location)
            .FirstOrDefaultAsync();

        var dto = new PrintDataDto
        {
            Name = student.FullName,
            UniversityID = student.UniversityID,
            Phone = student.Phone,
            NationalID = student.NationalID,
            BookedAt = interviewDate,
            Location = location,
            BookingID = booking.BookingID
        };

        return Ok(dto);
    }
    [HttpGet("filter-by-faculty")]
    public async Task<IActionResult> FilterByFaculty([FromQuery] string faculty)
    {
        if (string.IsNullOrWhiteSpace(faculty))
            return BadRequest("Faculty is required.");

        string normalizedFaculty = faculty.Trim().ToLower();

        var students = await _context.TestStudents
            .Where(s => s.College != null && s.College.Trim().ToLower() == normalizedFaculty)
            .ToListAsync();

        var studentIDs = students.Select(s => s.UniversityID).ToList();

        var bookings = await _context.StudentBookings
            .Include(sb => sb.InterviewSchedule)
            .Where(sb => studentIDs.Contains(sb.UniversityID))
            .ToListAsync();

        var result = bookings.Select(b =>
        {
            var student = students.FirstOrDefault(s => s.UniversityID == b.UniversityID);
            return new
            {
                Booking = b,
                Student = student,
                Schedule = b.InterviewSchedule
            };
        }).ToList();

        return Ok(result);
    }









}
