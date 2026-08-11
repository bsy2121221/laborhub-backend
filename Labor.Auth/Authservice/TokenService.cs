using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Labor.Auth.IAuthservice;
using Labor.Models.Entities.User;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Labor.Auth.Authservice
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user, string role)
        {
            var jwtKey = _configuration["JWT:SecretKey"];
            var jwtIssuer = _configuration["JWT:Issuer"];
            var jwtAudience = _configuration["JWT:Audience"];
            var expiryHours = int.Parse(_configuration["JWT:ExpiryHours"] ?? "24");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.MobileNumber),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.MobilePhone, user.MobileNumber),
                new Claim("IsProfileComplete", user.IsProfileComplete.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var jwtKey = _configuration["JWT:SecretKey"];
                var jwtIssuer = _configuration["JWT:Issuer"];
                var jwtAudience = _configuration["JWT:Audience"];

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
                
                throw new InvalidOperationException("User ID not found in token");
            }
            catch
            {
                throw new InvalidOperationException("Invalid token");
            }
        }

        public string GetRoleFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var roleClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
                return roleClaim?.Value ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public DateTime GetTokenExpiry(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                return jwtToken.ValidTo;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}