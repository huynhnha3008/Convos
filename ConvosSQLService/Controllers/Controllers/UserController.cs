using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;
using Services.SignalR;



namespace Services.Controllers
{

    [Route("api/users")]
    [ApiController]
    public class UserController : Controller
    {
       
        private readonly IUserService _userService;
       
        private readonly IHubContext<UserHub> _hubContext;
        
        public UserController(IUserService userService,IHubContext<UserHub> hubContext)

        {
            _userService = userService;
            _hubContext = hubContext;

            
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        [HttpDelete]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
          
            try
            {
                var result = await _userService.DeleteUserAsync(userId);
               
                await _hubContext.Clients.All.SendAsync("UserDeleted", userId);

                return Ok(new { Message = "Delete user successfully" });

            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }

}

