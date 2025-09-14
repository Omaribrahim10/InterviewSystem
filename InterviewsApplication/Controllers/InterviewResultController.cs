using InterviewsApplication.Data;
using InterviewsApplication.Models;
using InterviewsApplication.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InterviewsApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewResultController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InterviewResultController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InterviewResult>>> GetAll()
        {
            var results = await _context.InterviewResults
                .Include(r => r.Student)
                .Include(r => r.Department)
                .Include(r => r.Agent)
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("{universityID}")]
        public async Task<ActionResult<InterviewResult>> GetByUniversityID(string universityID)
        {
            var result = await _context.InterviewResults
                .Include(r => r.Student)
                .Include(r => r.Department)
                .Include(r => r.Agent)
                .FirstOrDefaultAsync(r => r.UniversityID == universityID);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<InterviewResult>> Create([FromBody] ResultDto input)
        {
            var exists = await _context.InterviewResults
                .AnyAsync(r => r.UniversityID == input.UniversityID);

            if (exists)
                return Conflict("Result already exists for this student.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated.");

            var agent = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (agent == null)
                return NotFound("Agent not found.");

            var result = new InterviewResult
            {
                UniversityID = input.UniversityID,
                DepartmentID = agent.DepartmentID,     
                InterviewStatus = input.Result,
                ReviewedBy = userId,
                Timestamp = DateTime.UtcNow
            };

            _context.InterviewResults.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByUniversityID), new { universityID = input.UniversityID }, result);
        }


        [HttpPut("{universityID}")]
        public async Task<IActionResult> Update(string universityID, [FromBody] ResultDto input)
        {
            if (universityID != input.UniversityID)
                return BadRequest("University ID mismatch.");

            var existing = await _context.InterviewResults
                .FirstOrDefaultAsync(r => r.UniversityID == universityID);

            if (existing == null)
                return NotFound();

            existing.InterviewStatus = input.Result;
            existing.Timestamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{universityID}")]
        public async Task<IActionResult> Delete(string universityID)
        {
            var result = await _context.InterviewResults
                .FirstOrDefaultAsync(r => r.UniversityID == universityID);

            if (result == null)
                return NotFound();

            _context.InterviewResults.Remove(result);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        public class ResultDto
        {
            public string UniversityID { get; set; }
            public InterviewResultEnum Result { get; set; }
        }

        [HttpGet("present-students")]
        public async Task<ActionResult<IEnumerable<TestStudent>>> GetPresentStudentsData()
        {
            var presentIDs = await _context.InterviewHistories
                .Where(h =>
                    h.InterviewStatus == InterviewStatusEnum.Present &&
                    h.DepartmentID == 1
                )
                .Select(h => h.UniversityID)
                .Distinct()
                .ToListAsync();

            var students = await _context.TestStudents
                .Where(s => presentIDs.Contains(s.UniversityID))
                .ToListAsync();

            return Ok(students);
        }


        [HttpGet("present")]
        public async Task<ActionResult<IEnumerable<TestStudent>>> GetPresentStudentsDat()
        {
            var presentIDs = await _context.InterviewHistories
                .Where(h =>
                    h.InterviewStatus == InterviewStatusEnum.Present &&
                    h.DepartmentID == 1
                )
                .Where(h => !_context.InterviewResults
                    .Any(r => r.UniversityID == h.UniversityID))
                .Select(h => h.UniversityID)
                .Distinct()
                .ToListAsync();

            var students = await _context.TestStudents
                .Where(s => presentIDs.Contains(s.UniversityID))
                .ToListAsync();

            return Ok(students);
        }

        
        [HttpGet("college-summary")]
        public async Task<IActionResult> GetCollegeSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var query = from student in _context.TestStudents
                        join result in _context.InterviewResults
                            on student.UniversityID equals result.UniversityID
                        select new { student.College, result.InterviewStatus, result.Timestamp };

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(x => x.Timestamp.Date >= startDate.Value.Date &&
                                         x.Timestamp.Date <= endDate.Value.Date);
            }
            else if (startDate.HasValue)
            {
                query = query.Where(x => x.Timestamp.Date == startDate.Value.Date);
            }

            var summary = await query
                .GroupBy(x => x.College)
                .Select(g => new
                {
                    College = g.Key,
                    Accepted = g.Count(r => r.InterviewStatus == InterviewResultEnum.Accepted),
                    Rejected = g.Count(r => r.InterviewStatus == InterviewResultEnum.Rejected),
                    Pending = g.Count(r => r.InterviewStatus == InterviewResultEnum.Pending)
                })
                .ToListAsync();

            return Ok(summary);
        }



    }
}
