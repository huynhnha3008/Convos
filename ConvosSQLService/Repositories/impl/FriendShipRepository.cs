using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class FriendShipRepository : GenericRepository<Friendship>, IFriendShipRepository
    {
        private readonly ConvosDbContext _context;

        public FriendShipRepository(ConvosDbContext context) : base(context)
        {
            _context = context;
        }
        public enum FriendshipQueryMode
        {
            OneWay,
            Reverse,
            AnyDirection
        }
        public async Task<Friendship> GetExistingFriendshipAsync(Guid requesterId, Guid addresseeId, FriendshipQueryMode mode = FriendshipQueryMode.AnyDirection)
        {
            return mode switch
            {
                FriendshipQueryMode.OneWay => await _context.Friendships.FirstOrDefaultAsync(f =>
                    f.RequesterId == requesterId && f.AddresseeId == addresseeId),

                FriendshipQueryMode.Reverse => await _context.Friendships.FirstOrDefaultAsync(f =>
                    f.RequesterId == addresseeId && f.AddresseeId == requesterId),

                FriendshipQueryMode.AnyDirection => await _context.Friendships.FirstOrDefaultAsync(f =>
                    (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
                    (f.RequesterId == addresseeId && f.AddresseeId == requesterId)),

                _ => null
            };
        }

        public async Task AddFriendshipAsync(Friendship friendship)
        {
            await _context.Friendships.AddAsync(friendship);
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task SaveChange()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Friendship> GetPendingFriendshipAsync(Guid requesterId, Guid addresseeId)
        {
            return await _context.Friendships.FirstOrDefaultAsync(f =>
            f.RequesterId == requesterId && f.AddresseeId == addresseeId && f.Status == FriendshipStatus.Pending);
        }

        public Task UpdateFriendshipAsync(Friendship friendship)
        {
            _context.Friendships.Update(friendship);
            return Task.CompletedTask;
        }

        public Task Remove(Friendship friendship)
        {
            _context.Friendships.Remove(friendship);
            return Task.CompletedTask;
        }

        public async Task<List<Friendship>> GetPendingFriendRequestsAsync(Guid userId)
        {
            return await _context.Friendships
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
            .Include(f => f.Requester)
            .ToListAsync();
        }



        public async Task<List<Friendship>> GetAcceptedFriendsAsync(Guid userId)
        {
            return await _context.Friendships
            .Where(f => f.RequesterId == userId && f.Status == FriendshipStatus.Accepted)
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .ToListAsync();
        }

        public async Task<List<Friendship>> GetOnlineFriendsAsync(Guid userId)
        {
            return await _context.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                        (f.RequesterId == userId && f.Addressee.Status == Status.Online)
                        )
            .ToListAsync();
        }

        public async Task<List<Friendship>> GetBlockedUsersAsync(Guid userId)
        {
            return await _context.Friendships
            .Include(f => f.Addressee)
            .Where(f => f.Status == FriendshipStatus.Blocked && f.RequesterId == userId)
            .ToListAsync();
        }

        public async Task<List<string>> GetMutualFriendsAsync(Guid currentUserId, Guid userId)
        {
            // Lấy danh sách bạn của currentUserId
            var currentUserFriendIds = await _context.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                       f.RequesterId == currentUserId )
                .Select(f => f.RequesterId == currentUserId ? f.AddresseeId : f.RequesterId)
                .ToListAsync();

            // Lấy danh sách bạn của userId
            var userFriendIds = await _context.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                        f.AddresseeId == userId)
                .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
                .ToListAsync();

            // Tìm danh sách bạn chung
            var mutualFriendIds = currentUserFriendIds.Intersect(userFriendIds).ToList();

            // Lấy thông tin người dùng từ bảng Users
            var mutualFriends = await _context.Users
                .Where(u => mutualFriendIds.Contains(u.Id))
                .ToListAsync();

            // Truy vấn ReminderName từ bảng Friendship nếu có
            var friendships = await _context.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                           (f.RequesterId == currentUserId || f.AddresseeId == userId) &&
                           mutualFriendIds.Contains(f.RequesterId == currentUserId ? f.AddresseeId : f.RequesterId))
                .ToListAsync();

            var displayNames = mutualFriends.Select(user =>
            {
                var friendship = friendships.FirstOrDefault(f =>
                    (f.RequesterId == currentUserId && f.AddresseeId == user.Id));

                return !string.IsNullOrEmpty(friendship?.ReminderName) ? friendship.ReminderName : user.DisplayName;
            }).ToList();

            return displayNames;
        }


        public async Task<List<string>> GetMutualServersAsync(Guid currentUserId, Guid userId)
        {
            return await _context.ServerMembers
            .Where(sm => sm.UserId == userId &&
                _context.ServerMembers.Any(s => s.UserId == currentUserId && s.Server.Name == sm.Server.Name))
            .Select(sm => sm.Server.Name)
            .ToListAsync();
        }

        public async Task<List<Friendship>> GetFriends(Guid requesterId, Guid addresseeId)
        {
            return await _context.Friendships
         .Where(f => (f.RequesterId == requesterId && f.AddresseeId == addresseeId && f.Status == FriendshipStatus.Accepted) ||
                      (f.RequesterId == addresseeId && f.AddresseeId == requesterId && f.Status == FriendshipStatus.Accepted))
         .ToListAsync();
        }

        public Task RemoveRang(List<Friendship> friendship)
        {
            _context.RemoveRange(friendship);
            return Task.CompletedTask;
        }
    }
}
