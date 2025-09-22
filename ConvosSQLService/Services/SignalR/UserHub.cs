using BusinessObjects.Models;
using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;
using Services.SignalR.Interfaces;

namespace Services.SignalR
{
    public class UserHub : Hub, IUserHub
    {
        private readonly IUserService _userService;
        private readonly IFriendshipService _friendshipService;

        public UserHub(IUserService userService, IFriendshipService friendshipService)
        {
            _userService = userService;
            _friendshipService = friendshipService;

        }
        public override Task OnConnectedAsync()
        {
            //Console.WriteLine($"Client connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }


        public override Task OnDisconnectedAsync(Exception? exception)
        {
            //Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendUserUpdate(User user)
        {
            await Clients.All.SendAsync("ReceiveUserUpdate", user);
        }

        public async Task SendFriendRequestNotification(string addresseeUsername, string requesterUsername)

        {
            //Console.WriteLine($"Sending friend request notification from {requesterUsername} to {addresseeUsername}");
            await Clients.User(addresseeUsername).SendAsync("NewFriendRequest", requesterUsername);
            await Clients.User(requesterUsername).SendAsync("FriendRequestSent", addresseeUsername);

            // Fetch and send updated pending requests to the addressee
            var pendingRequests = await _friendshipService.GetPendingFriendRequests(addresseeUsername);
            await Clients.User(addresseeUsername).SendAsync("PendingRequestsFetched", pendingRequests);

        }

        public async Task NotifyFriendRequestAccepted(string requesterUsername, string addresseeUsername)
        {
            await Clients.User(requesterUsername).SendAsync("FriendRequestAccepted", addresseeUsername);
        }

        public async Task NotifyFriendRequestIgnored(string requesterUsername, string addresseeUsername)
        {
            await Clients.User(requesterUsername).SendAsync("FriendRequestIgnored", addresseeUsername);
        }

        public async Task NotifyFriendRemoved(string user1Username, string user2Username)
        {
            await Clients.User(user1Username).SendAsync("FriendRemoved", user2Username);
            await Clients.User(user2Username).SendAsync("FriendRemoved", user1Username);
        }

        public async Task NotifyUserBlocked(string blockerUsername, string blockedUsername)
        {
            await Clients.User(blockerUsername).SendAsync("UserBlocked", blockedUsername);
        }

        public async Task NotifyUserUnblocked(string requesterUsername, string addresseeUsername)
        {
            await Clients.User(requesterUsername).SendAsync("UserUnblocked", addresseeUsername);

        }

        

        public async Task NotifyNewUser(User newUser)
        {
            await Clients.All.SendAsync("ReceiveNewUser", newUser);
        }
    }
}
