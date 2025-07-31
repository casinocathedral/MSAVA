using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using M_SAVA_INF.Models;
using Serilog.Events;
using Serilog;

namespace M_SAVA_INF.Environment
{
    public class LocalEnvironment : ILocalEnvironment
    {
        public LocalEnvironmentValues Values { get; }
        private readonly Dictionary<string, string> _values;
        private static readonly string EnvFolder = Path.GetDirectoryName(typeof(LocalEnvironment).Assembly.Location)!;
        private static readonly string EnvFileName = IsDevelopment() ? ".env.development" : ".env";
        private static readonly string EnvFilePath = Path.Combine(EnvFolder, EnvFileName);
        public static LocalEnvironment Instance { get; } = new LocalEnvironment();

        public LocalEnvironment()
        {
            _values = LoadEnvFile(EnvFilePath);
            Values = new LocalEnvironmentValues
            {
                JwtIssuerSigningKey = GetValueOrDefault("jwt_issuer_signing_key"),
                JwtIssuerName = GetValueOrDefault("jwt_issuer_name"),
                JwtIssuerAudience = GetValueOrDefault("jwt_issuer_audience"),
                AdminUsername = GetValueOrDefault("admin_username"),
                AdminPassword = GetValueOrDefault("admin_password"),
                PostgresBaseDbUser = GetValueOrDefault("postgres_basedb_user"),
                PostgresBaseDbPassword = GetValueOrDefault("postgres_basedb_password"),
                PostgresBaseDbHost = GetValueOrDefault("postgres_basedb_host"),
                PostgresBaseDbPort = int.TryParse(GetValueOrDefault("postgres_basedb_port"), out var port) ? port : 0,
                PostgresBaseDbDbName = GetValueOrDefault("postgres_basedb_dbname"),
                PostgresBaseDbSslMode = GetValueOrDefault("postgres_basedb_ssl_mode"),
                SerilogInformationLevel = ParseLogEventLevel(GetValueOrDefault("serilog_information_level")),
                SerilogRollingInterval = ParseRollingInterval(GetValueOrDefault("serilog_rolling_interval")),
                SerilogRetainedFileCountLimit = ParseNullableInt(GetValueOrDefault("serilog_retained_file_count_limit")),
                SerilogFileSizeLimitBytes = long.TryParse(GetValueOrDefault("serilog_file_size_limit_bytes"), out var fsl) ? fsl : 10 * 1024 * 1024,
                SerilogRollOnFileSizeLimit = bool.TryParse(GetValueOrDefault("serilog_roll_on_file_size_limit"), out var roll) ? roll : true
            };
        }

        public static LocalEnvironment GetInstance()
        {
            return Instance;
        }

        private static LogEventLevel ParseLogEventLevel(string value)
        {
            return Enum.TryParse(value, true, out LogEventLevel lvl) ? lvl : LogEventLevel.Information;
        }

        private static RollingInterval ParseRollingInterval(string value)
        {
            return Enum.TryParse(value, true, out RollingInterval ri) ? ri : RollingInterval.Day;
        }

        private static int? ParseNullableInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.ToLowerInvariant() == "null")
                return null;
            return int.TryParse(value, out var i) ? i : null;
        }

        private string GetValueOrDefault(string key)
        {
            if (_values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;
            return string.Empty;
        }

        public static bool IsDevelopment()
        {
            var aspnetEnv = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return string.Equals(aspnetEnv, "Development", StringComparison.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string> LoadEnvFile(string path)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(path))
                return dict;
            foreach (var line in File.ReadAllLines(path))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                    continue;
                var idx = trimmed.IndexOf('=');
                if (idx <= 0) continue;
                var key = trimmed.Substring(0, idx).Trim();
                var value = trimmed.Substring(idx + 1).Trim();
                dict[key] = value;
            }
            return dict;
        }

        public byte[] GetSigningKeyBytes()
        {
            string key = Values.JwtIssuerSigningKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException($"'jwt_issuer_signing_key' is missing or empty in {EnvFileName}");
            return Encoding.UTF8.GetBytes(key);
        }
    }
}
