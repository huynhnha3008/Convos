using Repositories.Interfaces;
using Services.impl;
using Services.Interfaces;
using Services;
using BusinessObjects.Models;

namespace Repositories.impl
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ConvosDbContext _context;
        private ICategoryRepository _category;
        private IChannelRepository _channel;
        private IChannelRolePermissionRepository _channelsRole;
        private IEmojiRepository _emoji;
        private IFriendShipRepository _friendShip;
        private IInviteRepository _invite;
        private IInviteUsageRepository _inviteUsage;
        private IMemberRoleRepository _memberRole;
        private IPermissionRepository _permission;
        private IRefreshTokenRepository _refreshToken;
        private IRoleRepository _role;
        private IServerMemberRepository _serverMember;
        private IUserRepository _user;
        private IServerRepository _server;
        private IRolePermissionRepository _rolePermission;
        private ISoundBoardRepository _soundBoard;
        private IEventRepository _event;
        private IQuizRepository _quiz;
        private IQuizMemberRepository _quizMember;
        public UnitOfWork(ConvosDbContext context)
        {
            _context = context;
        }


        public ICategoryRepository Categories => _category ??= new CategoryRepository(_context);

        public IChannelRepository Channels => _channel ??= new ChannelRepository(_context);


        public IEmojiRepository Emojis => _emoji ??= new EmojiRepository(_context);
        public ISoundBoardRepository SoundBoards => _soundBoard ??= new SoundBoardRepository(_context);
        public IFriendShipRepository FriendShips => _friendShip ??= new FriendShipRepository(_context);
        public IInviteRepository Invites => _invite ??= new InviteRepository(_context);

        public IMemberRoleRepository MemberRoles => _memberRole ??= new MemberRoleRepository(_context);

        public IInviteUsageRepository InvitesUsages => _inviteUsage ??= new InviteUsageRepository(_context);

        public IPermissionRepository Permissions => _permission ??= new PermissionRepository(_context);

        public IRefreshTokenRepository RefreshTokens => _refreshToken ??= new RefreshTokenRepository(_context);

        public IRoleRepository Roles => _role ??= new RoleRepository(_context);

        public IServerMemberRepository ServerMembers => _serverMember ??= new ServerMemberRepository(_context);

        public IServerRepository Servers => _server ??= new ServerRepository(_context);

        public IUserRepository Users => _user ??= new UserRepository(_context);

        public IRolePermissionRepository RolePermissions => _rolePermission ??= new RolePermissionRepository(_context);

        public IChannelRolePermissionRepository ChannelRolePermissions => _channelsRole ??= new ChannelRolePermissionRepository(_context);

        public IEventRepository Events => _event ??= new EventRepository(_context);

        public IQuizRepository Quizzes => _quiz ??= new QuizRepository(_context);

        public IQuizMemberRepository QuizMembers => _quizMember ??= new QuizMemberRepository(_context);


        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
