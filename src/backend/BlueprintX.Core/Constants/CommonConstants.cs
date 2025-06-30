using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Text;

namespace RedBook.Core.Constants
{
    public static class CommonConstants
    {
        private static IConfiguration? _configuration;
        public static void InitializeCommonConstants(this IServiceCollection services, IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static class SecurityConfig
        {
            private static readonly string? _salt = _configuration == null ? null : _configuration["PasswordConfig:Salt"];
            public static string SaltStr
            {
                get
                {
                    if (string.IsNullOrEmpty(_salt))
                        return "$2a$12$FxYtSkA4a4t7.RbG0U4aT.";
                    else
                        return _salt;
                }
                private set { }
            }

            public static byte[] SaltByte
            {
                get
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(SaltStr);
                    // Ensure it's exactly 16 bytes
                    Array.Resize(ref byteArray, 16);
                    return byteArray;
                }
                private set { }
            }

            private static readonly string? _saltGeneratorLogRounds = _configuration == null ? null : _configuration["PasswordConfig:SaltGeneratorLogRounds"];
            public static int SaltGeneratorLogRounds
            {
                get
                {
                    if (string.IsNullOrEmpty(_saltGeneratorLogRounds))
                        return 12;
                    else
                        return int.Parse(_saltGeneratorLogRounds);
                }
                private set { }
            }

            public static readonly string? _defaultPassword = _configuration == null ? null : _configuration["PasswordConfig:DefaultPassword"];
            public static string DefaultPassword
            {
                get
                {
                    if (string.IsNullOrEmpty(_defaultPassword))
                        return "sn0pBZ#KCCW2";
                    else
                        return _defaultPassword;
                }
                private set { }
            }

            private static readonly string? _jwtExpire = _configuration == null ? null : _configuration["PasswordConfig:SaltExpire"];
            public static double JWTExpire
            {
                get
                {
                    if (string.IsNullOrEmpty(_jwtExpire))
                        return 1d;
                    else
                        return double.Parse(_jwtExpire);
                }
                private set { }
            }

            private static readonly string? _jwtSecret = _configuration == null ? null : _configuration["PasswordConfig:JWTSecret"];
            public static byte[] JWTSecret
            {
                get
                {
                    if (string.IsNullOrEmpty(_jwtSecret))
                        return Encoding.UTF8.GetBytes("2A8f$kP9rV8*zN3LmQ7hWp2!xR56eTyU");
                    else
                        return Encoding.UTF8.GetBytes(_jwtSecret);
                }
                private set { }
            }
        }

        public static class StatusTypes
        {
            public const char Active = 'A';
            public const char Cancel = 'C';
            public const char Archived = 'D';
            public const char Pending = 'P';
        }

        public static class HttpResponseMessages
        {
            // Generic
            public const string Success = "Operation Successful";
            public const string Failed = "Operation Failed";
            public const string KeyExists = "This item already exists,The item you are trying to save already exists";
            public const string Exception = "It's not your, it's us!,A critical error occured behind the scene which wasn't your fault. Our experts are looking into it.";
            public const string InvalidInput = "An invalid parameter was passed. Please contact support team.";
            public const string ResourceNotFound = "Resource Not Found";

            // Login Specific
            public const string UserNotFound = "User Not Found";
            public const string PasswordMismatched = "Password Mismatched";
            public const string MailExists = "Account with this Email Id already exists, Try resting password";
            public const string InvalidToken = "Http request was sent using invalid token";

            // Access control
            public const string NotAllowed = "You not allowed to perform this operation";
            public const string AdminAccessRequired = "Only admin user can perform this peration";
        }
    }
}
