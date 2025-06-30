using Microsoft.IdentityModel.Tokens;
using RedBook.Core.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RedBook.Core.Security
{
    public class InternalAPICallService
    {
        public static string JWTForInternal
        {
            get
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                byte[] tokenKey = CommonConstants.SecurityConfig.SaltByte;
                SecurityToken token = new JwtSecurityToken
                (
                    issuer: "Inventory",
                    audience: "Client",
                    expires: DateTime.UtcNow.AddDays(CommonConstants.SecurityConfig.JWTExpire),
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(tokenKey),
                        SecurityAlgorithms.HmacSha256Signature
                    ),
                    claims: new List<Claim> {
                        new Claim("UserId", "Internal User"),
                        new Claim("UserName", "Internal User")
                    }
                );

                return tokenHandler.WriteToken(token);
            }

            private set { }
        }
    }
}
