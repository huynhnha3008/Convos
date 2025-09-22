namespace Services.SignalR.Interfaces
{
    public interface IInviteHub
    {
        Task AlertToServer(Guid id, string username);
        Task UpdateMemberList();
        Task SendMessageAsync(string message);
        Task CreateInvite(Guid serverId, string code);
        Task DeleteInvite(Guid serverId, string code);
    }
}
