using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Services.SignalR.Interfaces;
using Microsoft.AspNetCore.SignalR;


namespace Services.SignalR
{
    public class RoleHubs : Hub<IRoleHubs>
    {
        private static readonly ConcurrentDictionary<string, ConcurrentBag<string>> GroupConnections = new();

        public async Task JoinServer(string serverName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, serverName);
            var connections = GroupConnections.GetOrAdd(serverName, _ => new ConcurrentBag<string>());
            connections.Add(Context.ConnectionId);
        }

        public async Task AlertToChat(string groupId, string username)
        {
            await JoinServer(groupId);
            await Clients.Group(groupId).AlertToChat(groupId, username);
        }

        public async Task Chat(string groupId, string username, string content)
        {
            //await JoinServer(groupId);
            await Clients.Group(groupId).Chat(groupId, username, content);
        }
        public async Task UpdateParticipantsList(string groupId, string username)
        {
            await Clients.Group(groupId).UpdateParticipantsList();
        }

        public async Task OnLeaveServer(string groupId, string username)
        {
            await Clients.Group(groupId).OnLeaveServer(groupId, username);
        }

        public async Task OnAddServerMusic(string groupId, string musicName)
        {
            await Clients.Group(groupId).OnAddServerMusic(groupId, musicName);
        }



        public async Task LeaveServer(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
            if (GroupConnections.TryGetValue(groupId, out var connections))
            {
                connections = new ConcurrentBag<string>(connections.Except(new[] { Context.ConnectionId }));
                if (connections.IsEmpty)
                    GroupConnections.TryRemove(groupId, out _);
                else
                    GroupConnections[groupId] = connections;
            }
        }
        //------------------

        public async Task OnRoleCreated(string serverName, string roleName)
        {
            await Clients.Group(serverName).OnRoleCreated(serverName,roleName);
        }

        

        public async Task OnUserUnassignedFromRole(string serverName,string memberName, string roleName)
        {
            await Clients.Group(serverName).OnUserUnassignedFromRole( serverName,memberName,roleName);
        }

        public async Task OnRoleUpdated(string serverId, string roleName)
        {
            await Clients.Group(serverId).OnRoleUpdated(serverId, roleName);
        }

        public async Task OnRoleDeleted(string serverName, string roleId)
        {
            await Clients.Group(serverName).OnRoleDeleted(serverName, roleId);  
        }

        public async Task OnUserAssignedToRole(string serverName, string userId, string roleId)
        {
            await Clients.Group(serverName).OnUserAssignedToRole(serverName, userId, roleId);
        }

        public async Task OnUserRemovedFromRole(string serverName, string userId, string roleId)
        {
            await Clients.Group(serverName).OnUserRemovedFromRole(serverName, userId, roleId);
        }

        public async Task OnRoleHierarchyUpdated(string serverName)
        {
            await Clients.Group(serverName).OnRoleHierarchyUpdated(serverName);
        }

        public async Task OnUpdatePermission(string serverId, string roleId, string permissionCode)
        {
            await Clients.Group(serverId).OnUpdatePermission(serverId, roleId, permissionCode);
        }

        public async Task OnRolePermissionChanged(string serverId, string roleId, List<Guid> channelIds, string action)
        {
            await Clients.Group(serverId).OnRolePermissionChanged(serverId, roleId, channelIds, action);
        }

        public async Task ChannelPermissionUpdated(Guid channelId, bool isPrivate, Guid roleId)
        {
            await Clients.Group(roleId.ToString()).ChannelPermissionUpdated(channelId, isPrivate, roleId);
        }

        public async Task JoinRole(string roleId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roleId);
        }
        public async Task LeaveRole(string roleId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roleId);
        }

        public async Task JoinRoleGroupAlert(string roleId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roleId);
            var connections = GroupConnections.GetOrAdd(roleId, _ => new ConcurrentBag<string>());
            connections.Add(Context.ConnectionId);
        }

        public async Task AlertToRoleGroup(string roleId, string userId)
        {
            await JoinServer(roleId);
            await Clients.Group(roleId).AlertToRoleGroup(roleId, userId);
        }

        public async Task UpdateChannelListOnChangePermission(string groupId, string serverId)
        {
            await Clients.Group(groupId).UpdateChannelListOnChangePermission(groupId,serverId);
        }


    }
}
