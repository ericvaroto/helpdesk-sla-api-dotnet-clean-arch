using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Helpdesk.Application.Auth
{
    public sealed class JwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateAccessToken(Guid userId, string role, Guid tenantId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim("tenant_id", tenantId.ToString())
            };

            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static (string Raw, string Hash) GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            var raw = Convert.ToBase64String(bytes);

            using var sha = SHA256.Create();
            var hash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(raw)));

            return (raw, hash);
        }

        public static string HashToken(string raw)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        }
    }
}