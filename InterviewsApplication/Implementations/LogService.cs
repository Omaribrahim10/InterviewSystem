using InterviewsApplication.Data;
using InterviewsApplication.Interfaces;
using InterviewsApplication.Models;
using Microsoft.AspNetCore.Identity;

namespace InterviewsApplication.Implementations
{
    public class LogService : ILogService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LogService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task LogAsync(HttpContext context, string tableName, string description = null)
        {
            var user = context.User.Identity?.IsAuthenticated == true
                ? await _userManager.GetUserAsync(context.User)
                : null;

            var routeData = context.GetRouteData();
            var controller = routeData.Values["controller"]?.ToString();
            var action = routeData.Values["action"]?.ToString();
            var method = context.Request.Method;
            var ip = context.Connection.RemoteIpAddress?.ToString();

            var log = new LogEntry
            {
                UserId = user?.Id,
                UserEmail = user?.Email,
                Controller = controller,
                Action = action,
                Method = method,
                TableName = tableName,
                Description = description,
                IPAddress = ip
            };

            _context.LogEntries.Add(log);
            await _context.SaveChangesAsync();
        }
    }

}
