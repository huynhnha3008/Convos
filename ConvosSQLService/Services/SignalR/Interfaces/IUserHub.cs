using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SignalR.Interfaces
{
    public interface IUserHub
    {
        Task OnConnectedAsync();
        Task OnDisconnectedAsync(Exception? exception);
        Task SendUserUpdate(User user);
        Task SendFriendRequestNotification(string addresseeUsername, string requesterUsername);
        Task NotifyFriendRequestAccepted(string requesterUsername, string addresseeUsername);
        Task NotifyFriendRequestIgnored(string requesterUsername, string addresseeUsername);
        Task NotifyFriendRemoved(string user1Username, string user2Username);
        Task NotifyUserBlocked(string blockerUsername, string blockedUsername);
        Task NotifyUserUnblocked(string requesterUsername, string addresseeUsername);
        
        Task NotifyNewUser(User newUser);
    }
}
