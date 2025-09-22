namespace Services.SignalR.Interfaces
{
    public interface IVoiceChannelHubs
    {
        Task AlertToChannelParticipants(string channelId, string username);
        Task OnLeaveVoiceChannel(string channelId, string username);
        Task UpdateParticipantsList();
    }
}
