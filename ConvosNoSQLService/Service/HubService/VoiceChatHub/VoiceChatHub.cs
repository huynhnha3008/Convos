using Microsoft.AspNetCore.SignalR;

public class VoiceChatHub : Hub
{
    private static Dictionary<string, HashSet<string>> channelUsers = new();
    private static Dictionary<string, UserState> userStates = new();

    public class UserState
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsMuted { get; set; }
        public bool IsDeafened { get; set; }
        public bool IsScreenSharing { get; set; }
        public bool IsVideoOn { get; set; }
        public string AudioInput { get; set; }
        public string AudioOutput { get; set; }
        public string VideoInput { get; set; }
    }

    public async Task JoinChannel(string channelId, UserState userState)
    {
        if (!channelUsers.ContainsKey(channelId))
        {
            channelUsers[channelId] = new HashSet<string>();
        }

        channelUsers[channelId].Add(Context.ConnectionId);
        userStates[Context.ConnectionId] = userState;

        await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
        await Clients.Group(channelId).SendAsync("userJoined", userState);
        
        // Send current users in channel to the new user
        var channelParticipants = channelUsers[channelId]
            .Select(id => userStates[id])
            .ToList();
        await Clients.Caller.SendAsync("currentUsers", channelParticipants);
    }
public async Task UpdateDevices(string audioInput, string audioOutput, string videoInput)
{
    userStates[Context.ConnectionId] = new UserState
    {
        AudioInput = audioInput,
        AudioOutput = audioOutput,
        VideoInput = videoInput
    };

    await Clients.Caller.SendAsync("currentDevices", new
    {
        AudioInput = audioInput,
        AudioOutput = audioOutput,
        VideoInput = videoInput
    });
    await Clients.Group(GetChannelIdForUser()).SendAsync("updateDevices", new
    {
        AudioInput = audioInput,
        AudioOutput = audioOutput,
        VideoInput = videoInput
    });
}

private string GetChannelIdForUser()
{
    return channelUsers.FirstOrDefault(x => x.Value.Contains(Context.ConnectionId)).Key;
}
    public async Task LeaveChannel(string channelId)
    {
        if (channelUsers.ContainsKey(channelId))
        {
            channelUsers[channelId].Remove(Context.ConnectionId);
            var userState = userStates[Context.ConnectionId];
            userStates.Remove(Context.ConnectionId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
            await Clients.Group(channelId).SendAsync("userLeft", userState);
        }
    }

    public async Task UpdateUserState(UserState newState)
    {
        if (userStates.ContainsKey(Context.ConnectionId))
        {
            userStates[Context.ConnectionId] = newState;
            var channelId = channelUsers.FirstOrDefault(x => x.Value.Contains(Context.ConnectionId)).Key;
            if (channelId != null)
            {
                await Clients.Group(channelId).SendAsync("userStateUpdated", newState);
            }
        }
    }

    // WebRTC Signaling
    public async Task SendOffer(string targetUserId, object offer)
    {
        await Clients.Client(targetUserId).SendAsync("receiveOffer", Context.ConnectionId, offer);
    }

    public async Task SendAnswer(string targetUserId, object answer)
    {
        await Clients.Client(targetUserId).SendAsync("receiveAnswer", Context.ConnectionId, answer);
    }

    public async Task SendIceCandidate(string targetUserId, object candidate)
    {
        await Clients.Client(targetUserId).SendAsync("receiveIceCandidate", Context.ConnectionId, candidate);
    }
}