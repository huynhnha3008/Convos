
using BusinessObjects.Models;
using static Services.impl.FriendShipRepository;

namespace Services.Interfaces
{
        public interface IFriendShipRepository : IGenericRepository<Friendship>
        {
                //addfriend
                Task<Friendship> GetExistingFriendshipAsync(Guid requesterId, Guid addresseeId, FriendshipQueryMode mode = FriendshipQueryMode.AnyDirection);
                Task AddFriendshipAsync(Friendship friendship);
                Task CommitTransactionAsync();
                Task RollbackTransactionAsync();
                Task BeginTransactionAsync();
                Task SaveChange();
                Task Remove(Friendship friendship);
                Task RemoveRang(List<Friendship> friendship);

               
                Task<Friendship> GetPendingFriendshipAsync(Guid requesterId, Guid addresseeId);

                Task UpdateFriendshipAsync(Friendship friendship);
                Task<List<Friendship>> GetFriends(Guid requesterId, Guid addresseeId);
                Task<List<Friendship>> GetPendingFriendRequestsAsync(Guid userId);
                Task<List<Friendship>> GetAcceptedFriendsAsync(Guid userId);
                Task<List<Friendship>> GetOnlineFriendsAsync(Guid userId);
                Task<List<Friendship>> GetBlockedUsersAsync(Guid userId);
                Task<List<string>> GetMutualFriendsAsync(Guid currentUserId, Guid userId);
                Task<List<string>> GetMutualServersAsync(Guid currentUserId, Guid userId);

        }
}
