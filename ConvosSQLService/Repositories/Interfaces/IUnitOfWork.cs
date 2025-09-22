using Services.Interfaces;

namespace Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {

        IRolePermissionRepository RolePermissions { get; }
        ICategoryRepository Categories { get; }
        IChannelRepository Channels { get; }
        IChannelRolePermissionRepository ChannelRolePermissions { get; }
        IEmojiRepository Emojis { get; }
        ISoundBoardRepository SoundBoards { get; }
        IFriendShipRepository FriendShips { get; }
        IInviteRepository Invites { get; }
        IInviteUsageRepository InvitesUsages { get; }
        IMemberRoleRepository MemberRoles { get; }
        IPermissionRepository Permissions { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IRoleRepository Roles { get; }
        IServerMemberRepository ServerMembers { get; }
        IServerRepository Servers { get; }
        IUserRepository Users { get; }
        IEventRepository Events { get; }
        IQuizRepository Quizzes { get; }
        IQuizMemberRepository QuizMembers { get; }
        int Complete();
    }
}
