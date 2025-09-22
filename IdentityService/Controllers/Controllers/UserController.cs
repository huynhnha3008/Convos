using BusinessObjects.DTOs.UserDTO;
using BusinessObjects.Models;
using BussinessObjects.Settings;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Services;
using Services.Interface;
using Services.SignalR;
using Services.Tool;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Vonage.Server;


namespace Controllers.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IOTPService _otpService;
        private readonly IUserService _userService;
        private readonly UserPasswordHasher _userPasswordHasher;
        private readonly TokenTools _token;
        private readonly KeySetting _keySettings;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly SmsService _smsService;
        private readonly IHubContext<UserHub> _hubContext;
        private readonly FirebaseService _firebaseService;
        private readonly IPasswordResetService _passwordResetService;
        public UserController(IEmailService emailService, IOTPService otpService, IUserService userService,
            UserPasswordHasher userPasswordHasher, TokenTools token, IOptionsMonitor<KeySetting> keySettings,
            IRefreshTokenService refreshTokenService, SmsService smsService, IHubContext<UserHub> hubContext,
            FirebaseService firebaseService, IPasswordResetService passwordResetService)

        {
            _emailService = emailService;
            _otpService = otpService;
            _userService = userService;
            _userPasswordHasher = userPasswordHasher;
            _token = token;

            _keySettings = keySettings.CurrentValue;
            _refreshTokenService = refreshTokenService;
            _smsService = smsService;
            _hubContext = hubContext;
            _firebaseService = firebaseService;
            _passwordResetService = passwordResetService;
        }
        [HttpPost("email")]
        public async Task<IActionResult> SendOtpEmail(string email)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userService.FindUserByEmail(email);
            if (user != null && user.Password !=null)
            {
                return Conflict(new { message = "Email has existed!" });
            }
            var otpCode = _otpService.GenerateOTP();
            await _emailService.SendEmailAsync(email, "OTP Verification", $"Your OTP code is: {otpCode}");


            _otpService.StoreOTP(email, otpCode);

            return Ok(new { message = "OTP has been sent to your email." });
        }
        [HttpPost("phone")]
        public async Task<IActionResult> SendOtpPhone(string phonenumber)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userService.FindUserByPhonenumber(phonenumber);
            if (user != null)
            {
                return Conflict(new { message = "PhoneNumber has existed!" });
            }
            var otpCode = _otpService.GenerateOTP();
            await _smsService.SendSms(phonenumber, $"Your OTP code is: {otpCode}");


            _otpService.StoreOTP(phonenumber, otpCode);

            return Ok(new { message = "OTP has been sent to your phonenumber." });
        }
        [HttpPost("verify-otpEmail")]
        public async Task<IActionResult> VerifyOTPByEmail(string email, string OTPCodeEmail)
        {
            try
            {
                var isValidOtpEmail = await _otpService.ValidateOTP(email, OTPCodeEmail);
                if (!isValidOtpEmail) return BadRequest("Invalid OTP for your Email");
                var user_exist=await _userService.FindUserByEmail(email);
                if(user_exist != null && user_exist.Password==null) 
                {
                    
                    return Ok(new { message = "Verify OTP by Email is successfully." });
                }
                var newUser2 = new User()
                {
                    Email = email,
                    Status = Status.Online,
                    Banner = GenerateRandomHexColor(),
                    Hashtag = new Random().Next(1000, 9999).ToString(),
                    IsVerified = true,
                    JoinedAt = DateTime.Now,
                    Role = UserRole.User
                };
                await _userService.SaveUser(newUser2);
                return Ok(new { message = "Verify OTP by Email is successfully." });
                string GenerateRandomHexColor()
                {
                    Random random = new Random();
                    int r = random.Next(256);
                    int g = random.Next(256);
                    int b = random.Next(256);
                    return $"#{r:X2}{g:X2}{b:X2}";
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("verify-otpPhoneNumber")]
        public async Task<IActionResult> VerifyOTPByPhoneNumber(string PhoneNumber, string OTPCodePhone)
        {
            try
            {
                var isValidOtpEmail = await _otpService.ValidateOTP(PhoneNumber, OTPCodePhone);
                if (!isValidOtpEmail) return BadRequest("Invalid OTP for your PhoneNumber");
                return Ok(new { message = "Verify OTP by your PhoneNumber is successfully." });
            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm]UserRegister model, IFormFile? avatarFile)
        {
            var email_exist=await _userService.FindUserByEmail(model.Email);
            if(email_exist == null)
            {
                return BadRequest("This email does not exist");
            }
            if (email_exist != null && email_exist.IsVerified==false)
            {
                return BadRequest("This email does not verify");
            }
            if (email_exist !=null && email_exist.Password != null)
            {
                return Conflict("This email already exists, you should just update your profile.");
            }
            
            var passwordPattern = @"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$";

            if (!Regex.IsMatch(model.Password, passwordPattern))
            {
                return BadRequest("The password has at least 8 characters, including at least one capital letters and one digit");
            }

            var password =_userPasswordHasher.HashPassword(model, model.Password);
            var check_username = await _userService.FindUserByUsername(model.Username);
            if (check_username != null) { return BadRequest("Username has already exist"); }
            string avatarUrl;
            if (avatarFile != null)
            {
                using (var stream = avatarFile.OpenReadStream())
                {
                    avatarUrl = await _firebaseService.UploadAvatarAsync(stream, Guid.NewGuid() + Path.GetExtension(avatarFile.FileName));
                }

                email_exist.Username = model.Username;
                email_exist.DisplayName = model.DisplayName;
                email_exist.Password = password;
                email_exist.PhoneNumber = model.PhoneNumber;
                email_exist.Avatar = avatarUrl;
                email_exist.Pronouns = model.Pronouns;
                email_exist.About = model.About;
                email_exist.Birthdate = model.Birthdate;
                await _userService.UpdateUser(email_exist);
                await _hubContext.Clients.All.SendAsync("ReceiveNewUser", email_exist);
            }
            if (avatarFile == null)
            {
                email_exist.Username = model.Username;
                email_exist.DisplayName = model.DisplayName;
                email_exist.Password = password;
                email_exist.PhoneNumber = model.PhoneNumber;
                
                email_exist.Pronouns = model.Pronouns;
                email_exist.About = model.About;
                email_exist.Birthdate = model.Birthdate;
                await _userService.UpdateUser(email_exist);
                await _hubContext.Clients.All.SendAsync("ReceiveNewUser", email_exist);
            }

            return Ok(new { message = "Register user successfully." });
            string GenerateRandomHexColor()
            {
                Random random = new Random();
                int r = random.Next(256);
                int g = random.Next(256);
                int b = random.Next(256);
                return $"#{r:X2}{g:X2}{b:X2}";
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest model)
        {
            try
            {
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                string phonePattern = @"^\+?\d{10,15}$";
                if (string.IsNullOrEmpty(model.keyLogin))
                {
                    return Conflict(new { message = "Use Email or use PhoneNumber " });
                }

                User user = new User();
                if (Regex.IsMatch(model.keyLogin, emailPattern))
                {
                    user = await _userService.FindUserByEmail(model.keyLogin);
                    if (user == null)
                    {
                        return NotFound(new { message = "User is not existed " });
                    }
                }
                if (Regex.IsMatch(model.keyLogin, phonePattern))
                {
                    user = await _userService.FindUserByPhonenumber(model.keyLogin);
                    if (user == null)
                    {
                        return NotFound(new { message = "User is not existed " });
                    }
                }


                string pass = user.Password;
                var userconvert = new UserRegister
                {
                    Username = user.Username,
                    About = user.About,
                    PhoneNumber = user.PhoneNumber,
                    
                    Banner = user.Banner,
                    Birthdate = user.Birthdate,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Password = user.Password,
                    Pronouns = user.Pronouns
                };

                if (user != null)
                {
                    var verifypassword = _userPasswordHasher.VerifyHashedPassword(userconvert, user.Password, model.Password);
                    if (verifypassword == PasswordVerificationResult.Failed)
                    {
                        return BadRequest(new { message = "Password is wrong " });
                    }

                }
                var usermodel = new UserModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    About = user.About,
                    PhoneNumber = user.PhoneNumber,
                    Avatar = user.Avatar,
                    Banner = user.Banner,
                    Birthdate = user.Birthdate,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Hashtag = user.Hashtag,
                    IsVerified = user.IsVerified,
                    JoinedAt = user.JoinedAt,
                    Role = user.Role,
                    Status = user.Status,
                    Pronouns = user.Pronouns,
                    Password = user.Password,
                };
                var token = await _token.GenerateToken(usermodel);
                return Ok(token);


            }
            catch (Exception ex)
            {
                return BadRequest("Login failed :" + ex.Message);
            }
        }
        [HttpPost("renew-token")]
        public async Task<IActionResult> RenewToken(TokenSetting model)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_keySettings.SecretKey);
            var tokenValidateParam = new TokenValidationParameters
            {
                //tự cấp token
                ValidateIssuer = false,
                ValidateAudience = false,

                //ký vào token
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),

                ClockSkew = TimeSpan.Zero,

                ValidateLifetime = false //ko kiểm tra token hết hạn
            };
            try
            {
                //check 1: AccessToken valid format
                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validatedToken);

                //check 2: Check alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)//false
                    {
                        return BadRequest(new { message = "Invalid token " });

                    }
                }

                //check 3: Check accessToken expire?
                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expireDate = _token.ConvertUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Access token has not yet expired " });

                }

                //check 4: Check refreshtoken exist in DB
                var storedToken = await _refreshTokenService.GetRefreshToken(model.RefreshToken);
                if (storedToken == null)
                {
                    return NotFound(new { message = "Refresh token does not exist " });

                }

                //check 5: check refreshToken is used/revoked?
                if (storedToken.IsUsed)
                {
                    return BadRequest(new { message = "Refresh token has been used " });

                }
                if (storedToken.IsRevoked)
                {
                    return BadRequest(new { message = "Refresh token has been revoked" });

                }

                //check 6: AccessToken id == JwtId in RefreshToken
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    return BadRequest(new { message = "Token doesn't match" });

                }

                //Update token is used
                storedToken.IsRevoked = true;
                storedToken.IsUsed = true;
                await _refreshTokenService.Update(storedToken);

                //create new token
                var user = await _userService.FindUserById(storedToken.UserId);
                var usermodel = new UserModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    About = user.About,
                    PhoneNumber = user.PhoneNumber,
                    Avatar = user.Avatar,
                    Banner = user.Banner,
                    Birthdate = user.Birthdate,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Hashtag = user.Hashtag,
                    IsVerified = user.IsVerified,
                    JoinedAt = user.JoinedAt,
                    Role = user.Role,
                    Status = user.Status,
                    Pronouns = user.Pronouns,
                    Password = user.Password,
                };
                var token = await _token.GenerateToken(usermodel);
                return Ok(new
                {
                    message = "Renew token success",
                    accessToken = token.AccessToken,
                    refreshToken = token.RefreshToken
                });

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Something went wrong" });

            }
        }
        [HttpPut]
        public async Task<IActionResult> EditUserAccount([FromForm] UserUpdateRequest user, string? OtpEmail, string? OtpPhone, IFormFile? avatarFile)
        {
            try
            {
                User user_exist = await _userService.FindUserById(user.Id);
                if (user_exist == null)
                {
                    return NotFound(new { message = "User does not exist" });
                }

                bool isUpdated = false;


                if (user.Email != user_exist.Email && user.Email !=null)
                {
                    var isValidOtpEmail = await _otpService.ValidateOTP(user.Email, OtpEmail);
                    if (!isValidOtpEmail)
                    {
                        return BadRequest("Invalid OTP for your email");
                    }
                    user_exist.Email = user.Email;
                    isUpdated = true;
                }


                if (user.PhoneNumber != user_exist.PhoneNumber && user.PhoneNumber!=null)
                {
                    var isValidOtpPhone = await _otpService.ValidateOTP(user.PhoneNumber, OtpPhone);
                    if (!isValidOtpPhone)
                    {
                        return BadRequest("Invalid OTP for your phone number");
                    }
                    user_exist.PhoneNumber = user.PhoneNumber;
                    isUpdated = true;
                }


                if (user.Username != user_exist.Username && user.Username != null)
                {
                    user_exist.Username = user.Username;
                    isUpdated = true;
                }

                if (user.DisplayName != user_exist.DisplayName && user.DisplayName!=null)
                {
                    user_exist.DisplayName = user.DisplayName;
                    isUpdated = true;
                }
                string avatarUrl = user_exist.Avatar;
                if (avatarFile != null)
                {

                    using (var stream = avatarFile.OpenReadStream())
                    {
                        avatarUrl = await _firebaseService.UploadAvatarAsync(stream, Guid.NewGuid() + Path.GetExtension(avatarFile.FileName));

                    }
                }

                if (avatarUrl != user_exist.Avatar)
                {
                    user_exist.Avatar = avatarUrl;
                    isUpdated = true;
                }

                if (user.Banner != user_exist.Banner && user.Banner != null)
                {
                    user_exist.Banner = user.Banner;
                    isUpdated = true;
                }

                if (user.Pronouns != user_exist.Pronouns && user.Pronouns!=null)
                {
                    user_exist.Pronouns = user.Pronouns;
                    isUpdated = true;
                }

                if (user.About != user_exist.About && user.About != null)
                {
                    user_exist.About = user.About;
                    isUpdated = true;
                }

                if (user.Birthdate != user_exist.Birthdate && user.Birthdate !=null)
                {
                    user_exist.Birthdate = user.Birthdate;
                    isUpdated = true;
                }
                if (user.Status != user_exist.Status && user.Status !=null)
                {
                    user_exist.Status = user.Status;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    await _userService.UpdateUser(user_exist);

                    
                    await _hubContext.Clients.All.SendAsync("ReceiveUserUpdate", user_exist);

                    return Ok(new { message = "User successfully updated" });
                }

                return BadRequest(new { message = "No changes were made to the user" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update user due to an error: " + ex.Message });
            }
        }
        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(Guid userId, ChangePasswordRequest newpassword)
        {
            try
            {
                var user = await _userService.FindUserById(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User does not exist" });
                }

                var usermodel = new UserRegister
                {

                    Username = user.Username,
                    About = user.About,
                    PhoneNumber = user.PhoneNumber,
                    
                    Banner = user.Banner,
                    Birthdate = user.Birthdate,
                    DisplayName = user.DisplayName,
                    Email = user.Email,

                    Pronouns = user.Pronouns,
                    Password = user.Password,
                };
                var verifypassword = _userPasswordHasher.VerifyHashedPassword(usermodel, user.Password, newpassword.OldPassword);
                if (verifypassword == PasswordVerificationResult.Failed)
                {
                    return BadRequest(new { message = "OldPassword is wrong " });
                }
                if (newpassword.NewPassword != newpassword.ConfirmPassword)
                {
                    return BadRequest(new { message = "ConfirmPassword dont match with NewPassword" });
                }
                var passwordPattern = @"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$";
                if (!Regex.IsMatch(newpassword.NewPassword, passwordPattern))
                {
                    return BadRequest("The NewPassword has at least 8 characters, including at least one capital letters and one digit");
                }
                var n_password = _userPasswordHasher.HashPassword(usermodel, newpassword.NewPassword);
                user.Password = n_password;
                await _userService.UpdateUser(user);
                return Ok(new { message = "Update password successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update user password due to an error: " + ex.Message });
            }

        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user =await _userService.FindUserByEmail(request.Email);
            if (user == null)
                return NotFound("User with this email does not exist.");

            
            var token = Guid.NewGuid().ToString();
            var expiration = TimeSpan.FromHours(1);

            
            await _passwordResetService.SaveTokenAsync(token, user.Id, expiration);

            
            var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?token={token}";

            
            var emailContent = $@"
    <p>Hi {user.DisplayName},</p>
    <p>You recently requested to reset your password for your OCEA account.</p>
    <p>Click the link below to reset your password:</p>
    <a href='{resetLink}'>{resetLink}</a>
    <p>If you did not request this, please ignore this email or contact support if you have questions.</p>
    <p>Thanks,<br>The OCEA Team</p>";


            await _emailService.SendEmailAsync(user.Email, "Password Reset Request", emailContent);
            return Ok(new { message = "Password reset email sent.",data=token }) ;
            
        }
        [HttpPost("create-newpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            
            var userId = await _passwordResetService.GetUserIdByTokenAsync(request.ResetCode);
            if (userId == null)
                return BadRequest("Invalid or expired token.");

            var user = await _userService.FindUserByEmail(request.Email);
            if (user == null)
                return NotFound("User not found.");

            
            var usermodel = new UserRegister
            {

                Username = user.Username,
                About = user.About,
                PhoneNumber = user.PhoneNumber,

                Banner = user.Banner,
                Birthdate = user.Birthdate,
                DisplayName = user.DisplayName,
                Email = user.Email,

                Pronouns = user.Pronouns,
                Password = user.Password,
            };
            var passwordPattern = @"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$";
            if (!Regex.IsMatch(request.NewPassword, passwordPattern))
            {
                return BadRequest("The NewPassword has at least 8 characters, including at least one capital letters and one digit");
            }
            user.Password = _userPasswordHasher.HashPassword(usermodel, request.NewPassword);
           await _userService.UpdateUser(user);
               

            
            await _passwordResetService.DeleteTokenAsync(request.ResetCode);

            return Ok("NewPassword has been created successfully.");
        }
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
                var user= await _userService.FindUserByEmail(payload.Email);
                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = payload.Name ?? payload.Email.Split('@')[0],
                        DisplayName = payload.Name ?? "User",
                        Email = payload.Email,
                        Password = "",
                        PhoneNumber = null,
                        Avatar = payload.Picture,
                        Status = Status.Online,
                        Role = UserRole.User,
                        Banner = GenerateRandomHexColor(),
                        Pronouns = null,
                        About = null,
                        Hashtag = new Random().Next(1000, 9999).ToString(),
                        Birthdate = null, 
                        IsVerified = true, 
                        JoinedAt = DateTime.UtcNow
                    };
                    await _userService.SaveUser(user);
                    string GenerateRandomHexColor()
                    {
                        Random random = new Random();
                        int r = random.Next(256);
                        int g = random.Next(256);
                        int b = random.Next(256);
                        return $"#{r:X2}{g:X2}{b:X2}";
                    }

                }
                var usermodel = new UserModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    About = user.About,
                    PhoneNumber = user.PhoneNumber,
                    Avatar = user.Avatar,
                    Banner = user.Banner,
                    Birthdate = user.Birthdate,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Hashtag = user.Hashtag,
                    IsVerified = user.IsVerified,
                    JoinedAt = user.JoinedAt,
                    Role = user.Role,
                    Status = user.Status,
                    Pronouns = user.Pronouns,
                    Password = user.Password,
                };
                var token = await _token.GenerateToken(usermodel);
                return Ok(token); 
            }
            catch(InvalidJwtException)
            {
                return BadRequest(new { message = "Invalid Google Token" });
            }catch(Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }

    }
}
