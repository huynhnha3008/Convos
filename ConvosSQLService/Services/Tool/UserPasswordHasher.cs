

using BusinessObjects.DTOs.UserDto;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace Services.Tools
{
    public class UserPasswordHasher : IPasswordHasher<UserRegister>
    {
        public string HashPassword(UserRegister user, string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hashedPassword = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashedPassword);
            }
        }

        public PasswordVerificationResult VerifyHashedPassword(UserRegister user, string hashedPassword, string providedPassword)
        {
            var providedHashed = HashPassword(user, providedPassword);
            if (providedHashed == hashedPassword)
                return PasswordVerificationResult.Success;
            return PasswordVerificationResult.Failed;
        }
    }
}
