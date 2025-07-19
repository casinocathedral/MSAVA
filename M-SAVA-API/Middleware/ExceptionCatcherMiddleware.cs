using M_SAVA_BLL.Models;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace M_SAVA_API.Middleware
{
    public class ExceptionCatcherMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionCatcherMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
            var logger = context.RequestServices.GetRequiredService<ILogger<ExceptionCatcherMiddleware>>();
            var errorLogRepository = context.RequestServices.GetRequiredService<IIdentifiableRepository<ErrorLogDB>>();
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred");
                LogError(context, ex, errorLogRepository);
                await HandleExceptionAsync(context, ex, env.IsDevelopment());
            }
        }

        private void LogError(HttpContext context, Exception exception, IIdentifiableRepository<ErrorLogDB> errorLogRepository)
        {
            Guid? userId = null;
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdStr, out var parsedId))
                {
                    userId = parsedId;
                }
            }

            var errorLog = new ErrorLogDB
            {
                Id = Guid.NewGuid(),
                Message = exception.Message,
                StackTrace = exception.ToString(),
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };
            errorLogRepository.Insert(errorLog);
            errorLogRepository.Commit();
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
        {
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string message = exception.Message;
            string? stack = null;

            if (exception is UnauthorizedAccessException)
            {
                statusCode = (int)HttpStatusCode.Unauthorized;
            }
            else if (exception is KeyNotFoundException)
            {
                statusCode = (int)HttpStatusCode.NotFound;
            }

            if (isDevelopment)
            {
                stack = exception.ToString();
                var devResponse = new ErrorLogDTO { Id = Guid.Empty, Message = message, StackTrace = stack, Timestamp = DateTime.UtcNow, User = null };
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(JsonSerializer.Serialize(devResponse));
            }
            else
            {
                var prodResponse = new ErrorLogDTO { Id = Guid.Empty, Message = message, StackTrace = null, Timestamp = DateTime.UtcNow, User = null };
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(JsonSerializer.Serialize(prodResponse));
            }
        }
    }
}
