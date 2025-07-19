using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using M_SAVA_BLL.Models;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;

namespace M_SAVA_API.Handlers
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;
        private readonly ILogger<ExceptionHandler> _logger;
        private readonly IIdentifiableRepository<ErrorLogDB> _errorLogRepository;

        public ExceptionHandler(RequestDelegate next, IHostEnvironment env, ILogger<ExceptionHandler> logger, IIdentifiableRepository<ErrorLogDB> errorLogRepository)
        {
            _next = next;
            _env = env;
            _logger = logger;
            _errorLogRepository = errorLogRepository;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                LogError(context, ex);
                await HandleExceptionAsync(context, ex, _env.IsDevelopment());
            }
        }

        private void LogError(HttpContext context, Exception exception)
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
            _errorLogRepository.Insert(errorLog);
            _errorLogRepository.Commit();
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
