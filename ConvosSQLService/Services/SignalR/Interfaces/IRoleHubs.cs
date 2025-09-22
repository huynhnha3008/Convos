
namespace Services.SignalR.Interfaces
{
    public interface IRoleHubs 
    {
        Task AlertToChat(string groupId, string username);
        Task AlertToRoleGroup(string groupId, string username);
        Task Chat(string groupId, string username, string content);
        Task OnLeaveServer(string groupId, string username);

        Task OnAddServerMusic(string groupId, string musicId);
        Task UpdateParticipantsList();

        Task OnRoleCreated(string serverName,string roleName);

        Task OnRoleUpdated(string serverId, string roleName);

        Task OnUserUnassignedFromRole(string serverName, string memberName,  string roleName);

        Task OnRoleDeleted(string serverName, string roleId);

        Task OnUserAssignedToRole(string serverId, string memberName, string roleName);

        Task OnUserRemovedFromRole(string serverName, string userId, string roleId);

        Task OnRoleHierarchyUpdated(string serverName);

        Task OnUpdatePermission(string serverId, string roleId, string permissionCode);

        Task OnRolePermissionChanged(string serverId, string roleId, List<Guid> channelIds, string action);

        Task ChannelPermissionUpdated(Guid channelId, bool isPrivate, Guid roleId);

        Task UpdateChannelListOnChangePermission(string groupId,string serverId);

        Task LeaveRole(string roleId);
        Task JoinRole(string roleId);

    }
}
