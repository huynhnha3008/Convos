using BusinessObjects.DTOs.UserDto;

namespace Services.Interfaces
{
    public interface IFriendshipService
    {
        Task<bool> SendFriendRequest(string requesterUsername, string addresseeUsername);
        Task<bool> AcceptFriendRequest(string addresseeUsername, string requesterUsername);
        Task<bool> IgnoreFriendRequest(string addresseeUsername, string requesterUsername);
        Task<bool> RemoveFriend(string user1Username, string user2Username);
        Task<bool> BlockUser(string blockerUsername, string blockedUsername);
        Task<bool> UnblockUser(string requesterUsername, string addresseeUsername);
        Task<List<FriendRequestDto>> GetPendingFriendRequests(string username);
        Task<List<FriendRequestDto>> ShowAcceptedFriends(string username);
        Task<List<FriendRequestDto>> ShowOnlineFriends(string username);
        Task<List<FriendRequestDto>> GetBlockedUsers(string username);
        Task<UserDetailDto> GetUserDetailsByUsername(string username, Guid currentUserId);
        Task<bool> AddReminderNameAsync(Guid userId, Guid targetId, string reminderName);
    }
}
