using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Loggers
{
    public class ServiceLogger
    {
        private readonly ILogger<ServiceLogger> _logger;
        private readonly IIdentifiableRepository<ErrorLogDB> _errorLogRepository;
        private readonly IIdentifiableRepository<AccessLogDB> _accessLogRepository;
        private readonly IIdentifiableRepository<UserLogDB> _userLogRepository;

        public ServiceLogger(
            ILogger<ServiceLogger> logger,
            IIdentifiableRepository<ErrorLogDB> errorLogRepository,
            IIdentifiableRepository<AccessLogDB> accessLogRepository,
            IIdentifiableRepository<UserLogDB> userLogRepository)
        {
            _logger = logger;
            _errorLogRepository = errorLogRepository;
            _accessLogRepository = accessLogRepository;
            _userLogRepository = userLogRepository;
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation("{Message}", message);
        }

        public void WriteLog(int statusCode, string message, Guid? userId)
        {
            var errorLog = new ErrorLogDB
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogError("Status Code: {StatusCode}, Message: {Message}, UserId: {UserId}", statusCode, message, userId);
            _errorLogRepository.Insert(errorLog);
            _errorLogRepository.Commit();
        }

        public void WriteLog(AccessLogActions action, string message, Guid userId)
        {
            var accessLog = new AccessLogDB
            {
                Id = Guid.NewGuid(),
                Action = action,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };
            string actionString = action.ToString();

            _logger.LogInformation("Action: {Action}, Message: {Message}, UserId: {UserId}", actionString, message, userId);
            _accessLogRepository.Insert(accessLog);
            _accessLogRepository.Commit();
        }

        public void WriteLog(UserLogAction action, string message, Guid userId, Guid? adminId)
        {
            var userLog = new UserLogDB
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AdminId = adminId,
                Action = action,
            };
            string actionString = action.ToString();
            _logger.LogInformation("Action: {Action}, Message: {Message}, UserId: {UserId}, AdminId: {AdminId}", actionString, message, userId, adminId);

            _userLogRepository.Insert(userLog);
            _userLogRepository.Commit();
        }
    }
}
