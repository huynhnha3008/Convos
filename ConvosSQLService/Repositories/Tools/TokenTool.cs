using BusinessObjects.DTOs.UserDto;
using BusinessObjects.Models;
using BusinessObjects.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Tools
{
    public class TokenTool
    {
        private readonly KeySetting _appsettings;
        private readonly IUserRepository _userRepository;
        private readonly ConvosDbContext _context;
        public TokenTool(IOptionsMonitor<KeySetting> appsettings, IUserRepository userRepository, ConvosDbContext context)
        {
            _appsettings = appsettings.CurrentValue;
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<TokenSetting> GenerateToken(UserModel user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_appsettings.SecretKey);

            var claims = new List<Claim>
            {
                new Claim("UserID", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Hashtag",user.Hashtag),
                new Claim("isVerified",user.IsVerified.ToString()),
                new Claim("JoinedAt",user.JoinedAt.ToString()),
                new Claim("DisplayName",user.DisplayName),
                


                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Status", user.Status.ToString()),
                //token
                new Claim("TokenId", Guid.NewGuid().ToString())
            };

            //#

            var student = await _userRepository.GetUserByEmail(user.Email);
            // Add non-null claims
            if (user.PhoneNumber != null)
                claims.Add(new Claim("PhoneNumber", user.PhoneNumber));
            if (user.Avatar != null)
                claims.Add(new Claim("Avatar", user.Avatar));

            if (user.Banner != null)
                claims.Add(new Claim("Banner", user.Banner));

            if (user.Pronouns != null)
                claims.Add(new Claim("Pronouns", user.Pronouns));

            if (user.About != null)
                claims.Add(new Claim("About", user.About));
            if (user.Birthdate != null)
            {
                claims.Add(new Claim("Birthdate", user.Birthdate.ToString()));
            }
            if (user.Role == UserRole.Admin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            if (user.Role == UserRole.User)
            {
                claims.Add(new Claim(ClaimTypes.Role, "User"));
            }
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                JwtId = token.Id,
                UserId = user.Id,
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddHours(1)
            };
            await _context.RefreshTokens.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();
            var tokenResponse = new TokenSetting
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken

            };

            return tokenResponse;

        }

        private string GenerateRefreshToken()
        {
            var random=new byte[32];
            using(var rng=RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }
        
    }
}
