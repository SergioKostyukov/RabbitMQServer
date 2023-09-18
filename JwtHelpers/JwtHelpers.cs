using Microsoft.IdentityModel.Tokens;
using RabbitMQServer.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RabbitMQServer.JwtHelpers
{
    public static class JwtHelpers
    {
        private static IEnumerable<Claim> GetClaims(this UserTokens userAccounts)
        {
            IEnumerable<Claim> claims = new Claim[] {
                    new Claim(ClaimTypes.Name, userAccounts.UserName),
                    new Claim(ClaimTypes.Email, userAccounts.Email),
                    new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddDays(1).ToString("MMM ddd dd yyyy HH:mm:ss tt"))
            };
            return claims;
        }
        public static UserTokens GenTokenkey(UserTokens model, JwtSettings jwtSettings)
        {
            try
            {
                var UserToken = new UserTokens();
                if (model == null) throw new ArgumentException(null, nameof(model));

                // Get secret key
                var key = System.Text.Encoding.ASCII.GetBytes(jwtSettings.IssuerSigningKey);
                DateTime expireTime = DateTime.UtcNow.AddDays(1);
                UserToken.Validaty = expireTime.TimeOfDay;

                var JWToken = new JwtSecurityToken(
                    issuer: jwtSettings.ValidIssuer,
                    audience: jwtSettings.ValidAudience,
                    claims: GetClaims(model),
                    notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                    expires: new DateTimeOffset(expireTime).DateTime,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256));

                UserToken.Token = new JwtSecurityTokenHandler().WriteToken(JWToken);
                UserToken.UserName = model.UserName;
                UserToken.Email = model.Email;
                UserToken.ExpiredTime = DateTime.Now.AddDays(1);
                return UserToken;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}