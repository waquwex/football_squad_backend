using FootballSquad.Core.Domain.DTO;
using FootballSquad.Core.Domain.Entities;
using FootballSquad.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;


namespace FootballSquad.Core.Services
{
    public class JWTService : IJWTService
    {
        private readonly IConfiguration _configuration;

        public JWTService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenResponseDTO CreateJWTToken(Guid userId, string username, string email)
        {
            var expirationMinutes = Convert.ToInt32(_configuration["Auth:Jwt:ExpirationMinutes"]);
            var expirationTime = DateTime.UtcNow.AddMinutes(expirationMinutes);
            int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), // Subject user id
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT unique id
                new Claim(JwtRegisteredClaimNames.Iat, unixTimestamp.ToString()), // Issued at
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // Unique name identifier
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecretKey"]));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenGenerator = new JwtSecurityToken(
                _configuration["Auth:Jwt:Issuer"],
                _configuration["Auth:Jwt:Audience"],
                claims,
                expires: expirationTime,
                signingCredentials: signingCredentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(tokenGenerator);

            return new TokenResponseDTO()
            {
                Token = token,
                Email = email,
                UserName = username,
                TokenExpiration = expirationTime,
                RefreshToken = GenerateRefreshToken(),
                RefreshTokenExpiration = DateTime.UtcNow.AddMinutes(
                    Convert.ToInt32(_configuration["Auth:RefreshToken:ExpirationMinutes"]))
            };
        }

        public ClaimsPrincipal? GetClaimsPrincipalFromJwtToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudience = _configuration["Auth:Jwt:Audience"],
                ValidateIssuer = true,
                ValidIssuer = _configuration["Auth:Jwt:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecretKey"])),
                ValidateLifetime = false // Token might be expired, this method doesn't involve about expiration check
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters,
                out SecurityToken securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
                    StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return claimsPrincipal;
        }

        // Returns base64 string
        private string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(bytes); // fill random bytes of array
            return Convert.ToBase64String(bytes); // it will be 12 bytes
        }
    }
}
