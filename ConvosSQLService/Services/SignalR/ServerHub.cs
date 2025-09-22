using Services.SignalR.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using BusinessObjects.DTOs;
using BusinessObjects.DTOs.ChannelDto;
using BusinessObjects.DTOs.RealTImeDto; // Add this for IHostApplicationLifetime

namespace Services.SignalR
{
    public class ServerHub : Hub<IServerHub>
    {
        private static readonly ConcurrentDictionary<string, ConcurrentBag<string>> GroupConnections = new();
        private static readonly ConcurrentDictionary<Guid, List<string>> _serverMembers = new();
        private readonly IHostApplicationLifetime _applicationLifetime; // Inject IHostApplicationLifetime

        public ServerHub(IHostApplicationLifetime applicationLifetime)
        {
            _applicationLifetime = applicationLifetime;
        }

        public async Task JoinGroup(string serverId) // or groupName
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, serverId);
            var connections = GroupConnections.GetOrAdd(serverId, _ => new ConcurrentBag<string>());
            connections.Add(Context.ConnectionId);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await Clients.All.SendMessageAsync($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            await Clients.All.SendMessageAsync($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task AlertToServer(Guid serverId, string username)
        {
            await JoinGroup(serverId.ToString());
            await Clients.Group(serverId.ToString()).AlertToServer(serverId, username);
        }

        public async Task UpdateServer(Guid serverId, string serverName, string icon)
        {
            await Clients.Group(serverId.ToString()).UpdateServer(serverId, serverName, icon);
        }

        public async Task DeleteServer(Guid serverId, string serverName)
        {
            await Clients.Group(serverId.ToString()).DeleteServer(serverId, serverName);
        }

        public async Task UpdateMemberList(Guid serverId, Guid userId)
        {
            var members = _serverMembers.GetValueOrDefault(serverId, new List<string>());
            await Clients.Group(serverId.ToString()).UpdateMemberList();
        }

        public async Task ShutdownServer()
        {
            Console.WriteLine("Server is shutting down...");
            _applicationLifetime.StopApplication();
            await Task.CompletedTask;
        }

        //invite
        public async Task CreateInvite(Guid serverId, string code)
        {
            await Clients.Group(serverId.ToString()).CreateInvite(serverId, code);
        }

        public async Task DeleteInvite(Guid serverId, string code)
        {
            await Clients.Group(serverId.ToString()).DeleteInvite(serverId, code);
        }

        //category
        public async Task CreateCategory(Guid serverId, CategoryRealtimeResponse name)
        {
            await Clients.Group(serverId.ToString()).CreateCategory(serverId, name);
        }
        public async Task DeleteCategory(Guid serverId, CategoryRealtimeResponse name)
        {
            await Clients.Group(serverId.ToString()).DeleteCategory(serverId, name.Id.ToString());
        }

        public async Task UpdateCategory(Guid serverId, CategoryRealtimeResponse name)
        {
            await Clients.Group(serverId.ToString()).UpdateCategory(serverId, name);
        }
        public async Task ChangeCategoryPosition(Guid serverId, Guid categoryId, int newPosition)
        {
            await Clients.Group(serverId.ToString()).ChangeCategoryPosition(serverId, categoryId, newPosition);
        }

        //channel
        public async Task CreateChannel(Guid serverId, ChannelRealtimeResponse channel)
        {
            await Clients.Group(serverId.ToString()).CreateChannel(serverId, channel);
        }
        public async Task DeleteChannel(Guid serverId, string channelId)
        {
            await Clients.Group(serverId.ToString()).DeleteChannel(serverId, channelId);
        }

        public async Task UpdateChannel(Guid serverId, ChannelRealtimeResponse channel)
        {
            await Clients.Group(serverId.ToString()).UpdateChannel(serverId, channel);
        }

        public async Task ChangeChannelPosition(Guid serverId, Guid channelId, int newPosition)
        {
            await Clients.Group(serverId.ToString()).ChangeChannelPosition(serverId,channelId, newPosition);
        }

        public async Task UpdateChannelPermission(Guid serverId, Guid channelId, Guid roleId, string permissionCode, bool isGranted)
        {
            await Clients.Group(serverId.ToString()).UpdateChannelPermission(serverId, channelId, roleId, permissionCode, isGranted);
        }

        //emoji
        public async Task CreateEmoji(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).CreateEmoji(serverId, name);
        }
        public async Task DeleteEmoji(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).DeleteEmoji(serverId, name);
        }

        public async Task UpdateEmoji(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).UpdateEmoji(serverId, name);
        }

        //server member
        public async Task KickMember(Guid serverId, Guid targetMemberId)
        {
            await Clients.Group(serverId.ToString()).KickMember(serverId, targetMemberId);
        }
        public async Task BanMember(Guid serverId, Guid targetMemberId)
        {
            await Clients.Group(serverId.ToString()).BanMember(serverId, targetMemberId);
        }
        public async Task MuteMember(Guid serverId, Guid targetMemberId)
        {
            await Clients.Group(serverId.ToString()).MuteMember(serverId, targetMemberId);
        }
        public async Task DeafenMember(Guid serverId, Guid targetMemberId)
        {
            await Clients.Group(serverId.ToString()).DeafenMember(serverId, targetMemberId);
        }

        public async Task UnbanMember(Guid serverId, Guid targetMemberId)
        {
            await Clients.Group(serverId.ToString()).UnbanMember(serverId, targetMemberId);
        }
        public async Task UnmuteMember(Guid serverId, Guid targetMemberId)
        {
            await Clients.Group(serverId.ToString()).UnmuteMember(serverId, targetMemberId);
        }
        public async Task UndeafenMember(Guid serverId, Guid targetMemberId)
        {
            await Clients.Group(serverId.ToString()).UndeafenMember(serverId, targetMemberId);
        }

        public async Task UpdateMemberName(Guid serverId, Guid targetMemberId, string memberName)
        {
            await Clients.Group(serverId.ToString()).UpdateMemberName(serverId, targetMemberId, memberName);
        }


        //soundboard
        public async Task CreateSoundBoard(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).CreateSoundBoard(serverId, name);
        }
        public async Task DeleteSoundBoard(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).DeleteSoundBoard(serverId, name);
        }

        public async Task UpdateSoundBoard(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).UpdateSoundBoard(serverId, name);
        }

        //role
        public async Task JoinRole(Guid roleId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roleId.ToString());

            Console.WriteLine($"Connection {Context.ConnectionId} joined role group {roleId}");
        }
        public async Task OnRolePermissionChanged(Guid serverId, Guid roleId, Guid channelIds, string action)
        {
            await Clients.Group(roleId.ToString()).OnRolePermissionChanged(serverId, roleId, channelIds, action);
        }

        public async Task ChannelPermissionUpdated(Guid channelId, bool isPrivate, Guid roleId)
        {
            await Clients.Group(roleId.ToString()).ChannelPermissionUpdated(channelId, isPrivate, roleId);
        }


        public async Task LeaveRole(string roleId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roleId);
        }

    }
}
