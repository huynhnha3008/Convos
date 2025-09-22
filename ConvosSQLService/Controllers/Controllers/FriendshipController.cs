using BusinessObjects.DTOs;
using BusinessObjects.DTOs.UserDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services;
using Services.Interfaces;
using Services.SignalR;

namespace Controllers.Controllers
{
    [Route("api/friendships")]
    [ApiController]
    public class FriendshipController : ControllerBase
    {
        private readonly IUserService _userService;

        private readonly IFriendshipService _friendshipService;
        public FriendshipController(IUserService user, IFriendshipService friendshipService)
        {
            _userService = user;

            _friendshipService = friendshipService;
        }


        [HttpPost("invite-friend")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestModel request)
        {

            if (request == null || string.IsNullOrEmpty(request.AddresseeUsername))
            {
                return BadRequest(new { message = "Invalid request data." });
            }
            var currentUserName = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;


            var addresseeExists = await _userService.GetUserByUsername(request.AddresseeUsername);


            if (addresseeExists == null)
            {
                return BadRequest(new { message = "This user do not exist." });
            }

            if (currentUserName == request.AddresseeUsername)
            {
                return Conflict(new { message = "You cannot send your own invitation to yourself" });
            }
            var result = await _friendshipService.SendFriendRequest(currentUserName, request.AddresseeUsername);

            if (result)
            {
                try
                {


                    return Ok(new { message = $"Friend request sent to {request.AddresseeUsername} successfully." });

                }
                catch (Exception ex)
                {

                    return StatusCode(500, new { message = $"An error occurred while sending notifications: {ex}" });
                }
            }

            return BadRequest(new { message = "Cannot send friend request. The users may not exist, or you may have been blocked." });
        }


        [HttpPut("add-reminder-name")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddReminderName([FromBody] FriendshipReminderNameRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == "UserID")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid parsedUserId))
                {
                    throw new UnauthorizedAccessException("Invalid or missing UserID claim.");
                }

                var result = await _friendshipService.AddReminderNameAsync(parsedUserId, request.targetId, request.reminderName);

                if (result)
                {
                    return Ok($"Reminder name change to {request.reminderName} successfully");
                }
                return BadRequest(new { message = "Error accepting friend request." });

            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }




        }

        [HttpPut("accept-friend")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AcceptFriendRequest(string requesterUsername)
        {
            var currentUserName = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
            if (string.IsNullOrEmpty(requesterUsername))
            {

                return BadRequest(new { message = "Invalid request data." });
            }
            var result = await _friendshipService.AcceptFriendRequest(currentUserName, requesterUsername);
            if (result)
            {


                return Ok(new { message = "Friend request accepted." });
            }
            return BadRequest(new { message = "Error accepting friend request." });

        }
        [HttpPut("ignore-friend")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> IgnoreFriendRequest(string requesterUsername)
        {
            var currentUserName = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
            var result = await _friendshipService.IgnoreFriendRequest(currentUserName, requesterUsername);
            if (result)
            {


                return Ok(new { message = "Friend request ignored." });
            }
            return BadRequest(new { message = "Error ignoring friend request." });

        }
        [HttpDelete("remove-friend")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RemoveFriend(string deletedUser)
        {
            var currentUserName = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
            var result = await _friendshipService.RemoveFriend(currentUserName, deletedUser);
            if (result)
            {


                return Ok(new { message = "Friend removed." });
            }
            return BadRequest(new { message = "Error removing friend." });

        }
        [HttpPost("blocked-user")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> BlockedUser(string blockedUsername)
        {
            var currentUserName = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
            if (blockedUsername == currentUserName)
            {
                return Conflict(new { message = "You cannot block for yourself" });
            }
            var result = await _friendshipService.BlockUser(currentUserName, blockedUsername);
            if (result)
            {


                return Ok(new { message = "User blocked." });
            }
            return BadRequest(new { message = "Error block user." });

        }
        [HttpPost("unblock-user")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UnblockUser(string unblockedUsername)
        {
            var currentUserName = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
            var success = await _friendshipService.UnblockUser(currentUserName, unblockedUsername);

            if (!success)
                return BadRequest(new { message = "Unblock operation failed." });


            return Ok(new { message = "User unblocked successfully." });

        }
        [HttpGet("pending-requests")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetPendingFriendRequests()
        {
            var currentUserName = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
            var requests = await _friendshipService.GetPendingFriendRequests(currentUserName);
            if (requests != null && requests.Count > 0)
            {

                return Ok(requests);
            }
            return NotFound(requests);
        }
        [HttpGet("accepted-friends")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetAcceptedFriends()
        {
            var currentUserName = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
            var acceptedFriends = await _friendshipService.ShowAcceptedFriends(currentUserName);

            if (acceptedFriends == null || !acceptedFriends.Any())
            {
                return NotFound(new { message = "No accepted friends found." });

            }

            return Ok(acceptedFriends);
        }
        [HttpGet("online-friends")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetOnlineFriends()
        {
            var currentUserName = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
            var onlineFriends = await _friendshipService.ShowOnlineFriends(currentUserName);

            if (onlineFriends == null || !onlineFriends.Any())
            {
                return NotFound(new { message = "No online friends found." });

            }


            return Ok(onlineFriends);
        }
        [HttpGet("blocked-users")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetBlockedUsers()
        {
            var currentUserName = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
            var blockedUsers = await _friendshipService.GetBlockedUsers(currentUserName);
            if (blockedUsers == null || !blockedUsers.Any())
            {
                return NotFound();
            }


            return Ok(blockedUsers);
        }

        [Authorize(Roles = "User")]
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserDetails(string username)
        {

            var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
            if (username == User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value)
            {
                return BadRequest(new { message = "You can't find mutualfriend and mutualserver for yourself" });
            }
            var userDetails = await _friendshipService.GetUserDetailsByUsername(username, currentUserId);

            if (userDetails == null)
                return NotFound();

            return Ok(userDetails);
        }
    }
}