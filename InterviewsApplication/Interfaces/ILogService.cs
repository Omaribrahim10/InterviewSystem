namespace InterviewsApplication.Interfaces
{
    public interface ILogService
    {
        Task LogAsync(HttpContext httpContext, string tableName, string description = null);
    }

}
