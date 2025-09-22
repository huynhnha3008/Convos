using BussinessObjects.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
namespace IdentityService.Controllers
{
    [Route("api/validate")]
    [ApiController]
    public class ValidateController : Controller
    {
        private readonly KeySetting _appsettings;
        private readonly IConfiguration _configuration;

        public ValidateController(IConfiguration configuration, IOptionsMonitor<KeySetting> appsettings)
        {
            _configuration = configuration;
            _appsettings = appsettings.CurrentValue;
        }

        [HttpPost]
        public IActionResult ValidateToken([FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Token is missing or invalid" });
            }

            var token = authorization.Substring("Bearer ".Length).Trim();
            var validationResult = ValidateJwtToken(token);

            if (validationResult)
            {
                return Ok(new { message = "Token is valid" });
            }

            return Unauthorized(new { message = "Invalid token" });
        }

        private bool ValidateJwtToken(string token)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_appsettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                // Validate the token
                var principal = jwtTokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                // Check if the token is a JWT token and has a valid signing algorithm
                return validatedToken is JwtSecurityToken jwtToken &&
                       jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
            }
            catch
            {
                // Token is invalid
                return false;
            }
        }
    }
}
