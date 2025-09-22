using BusinessObjects.DTOs;
using BusinessObjects.DTOs.ChannelDto;
using BusinessObjects.DTOs.RealTImeDto;

namespace Services.SignalR.Interfaces
{
    public interface IServerHub
    {
        Task AlertToServer(Guid id, string username);
        Task UpdateMemberList();
        Task SendMessageAsync(string message);
        Task UpdateServer(Guid id, string serverName, string icon);
        Task DeleteServer(Guid id, string serverName);

        Task ShutdownServer();

        //invite
        Task CreateInvite(Guid serverId, string code);
        Task DeleteInvite(Guid serverId, string code);


        //category
        Task DeleteCategory(Guid serverId, string categoryId);
        Task CreateCategory(Guid serverId, CategoryRealtimeResponse category);
        Task UpdateCategory(Guid serverId, CategoryRealtimeResponse category);
        Task ChangeCategoryPosition(Guid serverId, Guid categoryId, int newPosition);

        // Task AlertToCategory(Guid serverId, string categoryName);

        //channel
        Task DeleteChannel(Guid serverId, string channelId);
        Task CreateChannel(Guid serverId, ChannelRealtimeResponse channelName);
        Task UpdateChannel(Guid serverId, ChannelRealtimeResponse channelName);
        Task ChangeChannelPosition(Guid serverId, Guid channelId, int newPosition);
        Task UpdateChannelPermission(Guid serverId, Guid channelId, Guid roleId, string permissionCode, bool isGranted);
        //Task AlertToChannel(Guid serverId, string categoryName);

        //emoji
        Task DeleteEmoji(Guid serverId, string EmojiName);
        Task CreateEmoji(Guid serverId, string EmojiName);
        Task UpdateEmoji(Guid serverId, string EmojiName);

        //servermember
        Task KickMember(Guid serverId, Guid targetMemberId);
        Task BanMember(Guid serverId, Guid targetMemberId);
        Task MuteMember(Guid serverId, Guid targetMemberId);
        Task DeafenMember(Guid serverId, Guid targetMemberId);

        Task UnbanMember(Guid serverId, Guid targetMemberId);
        Task UnmuteMember(Guid serverId, Guid targetMemberId);
        Task UndeafenMember(Guid serverId, Guid targetMemberId);

        Task UpdateMemberName (Guid serverId,Guid targetMemberId, string memberName);


        //soundboard
        Task DeleteSoundBoard(Guid serverId, string SoundBoardName);
        Task CreateSoundBoard(Guid serverId, string SoundBoardName);
        Task UpdateSoundBoard(Guid serverId, string SoundBoardName);

        //role
        Task OnRolePermissionChanged(Guid serverId, Guid roleId, Guid channelIds, string action);

        Task ChannelPermissionUpdated(Guid channelId, bool isPrivate, Guid roleId);
    }
}
