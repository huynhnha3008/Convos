using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Service.HubService.WebRTCHub
{
    public class ChannelCallHub : Hub
    {
private static readonly Dictionary<string, HashSet<string>> _channelParticipants = new Dictionary<string, HashSet<string>>();
    private static readonly Dictionary<string, string> _userNames = new Dictionary<string, string>();

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var userId = httpContext.Request.Query["user-id"].ToString();

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"User {userId} connected with connection ID: {Context.ConnectionId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var httpContext = Context.GetHttpContext();
        var userId = httpContext.Request.Query["user-id"].ToString();

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"User {userId} disconnected with connection ID: {Context.ConnectionId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
    public async Task JoinChannel(string channelId, string userId, string userName)
    {
        try
        {
            Console.WriteLine($"User {userId} ({userName}) joining channel {channelId}");
            
            // Store user name
            _userNames[userId] = userName;
            
            // Add to channel group
            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
            
            // Add to participants list
            if (!_channelParticipants.ContainsKey(channelId))
            {
                _channelParticipants[channelId] = new HashSet<string>();
            }
            _channelParticipants[channelId].Add(userId);

            // Notify others in the channel about the new participant
            await Clients.GroupExcept(channelId, Context.ConnectionId)
                .SendAsync("UserJoinedChannel", userId, userName);

            // Send current participants list to the new user
            var participants = _channelParticipants[channelId]
                .Select(p => new { userId = p, userName = _userNames.GetValueOrDefault(p, "Unknown User") })
                .ToList();
            
            await Clients.Caller.SendAsync("ChannelParticipants", participants);
            
            Console.WriteLine($"User {userId} successfully joined channel {channelId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinChannel: {ex}");
            throw;
        }
    }

        public async Task LeaveChannel(string channelId, string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
            
            if (_channelParticipants.ContainsKey(channelId))
            {
                _channelParticipants[channelId].Remove(userId);
                
                if (_channelParticipants[channelId].Count == 0)
                {
                    _channelParticipants.Remove(channelId);
                }
            }

            await Clients.Group(channelId).SendAsync("UserLeftChannel", userId);
        }

    public async Task SendOffer(string channelId, string targetUserId, string senderId, string offer)
    {
        try
        {
            Console.WriteLine($"Sending offer from {senderId} to {targetUserId} in channel {channelId}");
            await Clients.Group(targetUserId).SendAsync("ReceiveOffer", channelId, senderId, offer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendOffer: {ex}");
            throw;
        }
    }

    public async Task SendAnswer(string channelId, string targetUserId, string senderId, string answer)
    {
        try
        {
            Console.WriteLine($"Sending answer from {senderId} to {targetUserId} in channel {channelId}");
            await Clients.Group(targetUserId).SendAsync("ReceiveAnswer", channelId, senderId, answer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendAnswer: {ex}");
            throw;
        }
    }

    public async Task SendIceCandidate(string channelId, string targetUserId, string senderId, string iceCandidate)
    {
        try
        {
            Console.WriteLine($"Sending ICE candidate from {senderId} to {targetUserId} in channel {channelId}");
            await Clients.Group(targetUserId).SendAsync("ReceiveIceCandidate", channelId, senderId, iceCandidate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendIceCandidate: {ex}");
            throw;
        }
    }

        public async Task ToggleAudio(string channelId, string userId, bool isMuted)
        {
            await Clients.GroupExcept(channelId, Context.ConnectionId)
                .SendAsync("UserAudioToggled", userId, isMuted);
        }

        public async Task ToggleVideo(string channelId, string userId, bool isVideoOff)
        {
            await Clients.GroupExcept(channelId, Context.ConnectionId)
                .SendAsync("UserVideoToggled", userId, isVideoOff);
        }

        public async Task ToggleScreenShare(string channelId, string userId, bool isScreenSharing)
        {
            await Clients.GroupExcept(channelId, Context.ConnectionId)
                .SendAsync("UserScreenShareToggled", userId, isScreenSharing);
        }

        public async Task GetChannelParticipants(string channelId)
        {
            if (_channelParticipants.ContainsKey(channelId))
            {
                var participants = _channelParticipants[channelId];
                await Clients.Caller.SendAsync("ChannelParticipants", participants);
            }
        }

        public async Task NotifySpeaking(string channelId, string userId, bool isSpeaking)
        {
            await Clients.GroupExcept(channelId, Context.ConnectionId)
                .SendAsync("UserSpeaking", userId, isSpeaking);
        }

        public async Task UpdateUserStatus(string channelId, string userId, string status)
        {
            await Clients.GroupExcept(channelId, Context.ConnectionId)
                .SendAsync("UserStatusUpdated", userId, status);
        }
    }
}