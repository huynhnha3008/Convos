using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Service.HubService.WebRTCHub
{
    public class PrivateCallHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["user-id"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await Clients.All.SendAsync("UserConnected", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["user-id"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }

            await Clients.All.SendAsync("UserDisconnected", userId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendOffer(string targetUserId, string senderName, string senderId, string offer)
        {
            await Clients.Group(targetUserId).SendAsync("ReceiveOffer", senderName, senderId, offer);
        }

        public async Task SendAnswer(string targetUserId, string senderId, string answer)
        {
            await Clients.Group(targetUserId).SendAsync("ReceiveAnswer", senderId, answer);
        }

        public async Task SendIceCandidate(string targetUserId, string senderId, string iceCandidate)
        {
            await Clients.Group(targetUserId).SendAsync("ReceiveIceCandidate", senderId, iceCandidate);
        }
        public async Task NotifyEndCall(string targetUserId, string senderId)
        {
            await Clients.Group(targetUserId).SendAsync("NotifyEndCall", senderId);
        }
        public async Task NotifyDeclineCall(string type, string senderName, string targetUserId, string senderId)
        {
            await Clients.Group(targetUserId).SendAsync("NotifyDeclineCall", type, senderName, senderId);
        }
        public async Task NotifyMute(string receiverId, string senderId, bool isMuted)
        {
            await Clients.User(receiverId).SendAsync("NotifyMute", senderId, isMuted);
        }

        public async Task NotifyDeafen(string receiverId, string senderId, bool isDeafened)
        {
            await Clients.User(receiverId).SendAsync("NotifyDeafen", senderId, isDeafened);
        }

        public async Task NotifyVideo(string receiverId, string senderId, bool isVideoOff)
        {
            await Clients.User(receiverId).SendAsync("NotifyVideo", senderId, isVideoOff);
        }
         public async Task NotifyScreenShare(string receiverId, string senderId, bool isScreenOff)
        {
            await Clients.User(receiverId).SendAsync("NotifyScreenShare", senderId, isScreenOff);
        }
    }
}