using BusinessObjects.DTOs.UserDto;
using BusinessObjects.Models;
using Microsoft.AspNetCore.SignalR;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.SignalR;
using System;
using static Services.impl.FriendShipRepository;


namespace Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<UserHub> _hubContext;

        public FriendshipService(IUnitOfWork unitOfWork, IHubContext<UserHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }
        public async Task<bool> SendFriendRequest(string requesterUsername, string addresseeUsername)
        {
            await _unitOfWork.FriendShips.BeginTransactionAsync();
            try
            {
                var requester = await _unitOfWork.Users.GetUserByUsername(requesterUsername);
                var addressee = await _unitOfWork.Users.GetUserByUsername(addresseeUsername);

                if (requester == null || addressee == null)
                {
                    return false;
                }

                var existingFriendship = await _unitOfWork.FriendShips.GetExistingFriendshipAsync(requester.Id, addressee.Id, FriendshipQueryMode.AnyDirection);

                if (existingFriendship != null)
                {
                    if (existingFriendship.Status == FriendshipStatus.Blocked)
                    {
                        return false;
                    }
                    return false;
                }
                if (existingFriendship != null)
                {
                    var backRequest = await _unitOfWork.FriendShips.GetExistingFriendshipAsync(requester.Id, addressee.Id, FriendshipQueryMode.Reverse);

                    if (backRequest != null)
                    {
                        existingFriendship.Status = FriendshipStatus.Accepted;
                        var newFriendship = new Friendship
                        {
                            RequesterId = requester.Id,
                            AddresseeId = addressee.Id,
                            Status = FriendshipStatus.Accepted
                        };
                        await _unitOfWork.FriendShips.AddFriendshipAsync(newFriendship);
                        await _unitOfWork.FriendShips.CommitTransactionAsync();
                        await _unitOfWork.FriendShips.SaveChange();
                        try
                        {
                            await _hubContext.Clients.User(addresseeUsername).SendAsync("NewFriendRequest", requesterUsername);
                            await _hubContext.Clients.User(requesterUsername).SendAsync("FriendRequestSent", addresseeUsername);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error sending SignalR notification:" + ex.Message);
                        }
                        return true;
                    }
                }

                if (existingFriendship == null)
                {
                    var friendship = new Friendship
                    {
                        RequesterId = requester.Id,
                        AddresseeId = addressee.Id,
                        Status = FriendshipStatus.Pending
                    };
                    await _unitOfWork.FriendShips.AddFriendshipAsync(friendship);
                    await _unitOfWork.FriendShips.CommitTransactionAsync();
                    await _unitOfWork.FriendShips.SaveChange();
                    try
                    {
                        await _hubContext.Clients.User(addresseeUsername).SendAsync("NewFriendRequest", requesterUsername);
                        await _hubContext.Clients.User(requesterUsername).SendAsync("FriendRequestSent", addresseeUsername);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error sending SignalR notification:" + ex.Message);
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.FriendShips.RollbackTransactionAsync();
                return false;
            }

            return false;
        }
        public async Task<bool> AcceptFriendRequest(string addresseeUsername, string requesterUsername)
        {
            var addressee = await _unitOfWork.Users.GetUserByUsername(addresseeUsername);
            var requester = await _unitOfWork.Users.GetUserByUsername(requesterUsername);

            if (addressee == null || requester == null)
            {
                return false;
            }

            var friendship = await _unitOfWork.FriendShips.GetPendingFriendshipAsync(requester.Id, addressee.Id);

            if (friendship == null)
            {
                return false;
            }

            friendship.Status = FriendshipStatus.Accepted;

            var newFriendship = new Friendship
            {
                RequesterId = addressee.Id,
                AddresseeId = requester.Id,
                Status = FriendshipStatus.Accepted
            };

            await _unitOfWork.FriendShips.AddFriendshipAsync(newFriendship);
            await _unitOfWork.FriendShips.SaveChange();
            try
            {
                await _hubContext.Clients.User(addresseeUsername).SendAsync("FriendRequestAccepted", requesterUsername);
                await _hubContext.Clients.User(requesterUsername).SendAsync("FriendRequestAccepted", addresseeUsername);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending SignalR notification:" + ex.Message);
            }

            return true;
        }
        public async Task<bool> IgnoreFriendRequest(string addresseeUsername, string requesterUsername)
        {
            var addressee = await _unitOfWork.Users.GetUserByUsername(addresseeUsername);
            var requester = await _unitOfWork.Users.GetUserByUsername(requesterUsername);

            if (addressee == null || requester == null)
            {
                return false;
            }

            var friendship = await _unitOfWork.FriendShips.GetPendingFriendshipAsync(requester.Id, addressee.Id);

            if (friendship == null)
            {
                return false;
            }
            await _unitOfWork.FriendShips.Remove(friendship);
            await _unitOfWork.FriendShips.SaveChange();
            try
            {
                await _hubContext.Clients.User(addresseeUsername).SendAsync("FriendRequestIgnored", requesterUsername);
                await _hubContext.Clients.User(requesterUsername).SendAsync("FriendRequestIgnored", addresseeUsername);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending SignalR notification:" + ex.Message);
            }
            return true;
        }
        public async Task<bool> RemoveFriend(string user1Username, string user2Username)
        {
            var user1 = await _unitOfWork.Users.GetUserByUsername(user1Username);
            var user2 = await _unitOfWork.Users.GetUserByUsername(user2Username);
            if (user1 == null || user2 == null)
            {
                return false;
            }
            var friendship = await _unitOfWork.FriendShips.GetFriends(user1.Id, user2.Id);
            if (friendship.Count() == 0)
            {
                return false;
            }
            await _unitOfWork.FriendShips.RemoveRang(friendship);
            await _unitOfWork.FriendShips.SaveChange();
            try
            {
                await _hubContext.Clients.User(user1Username).SendAsync("FriendRemoved", user2Username);
                await _hubContext.Clients.User(user2Username).SendAsync("FriendRemoved", user1Username);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending SignalR notification:" + ex.Message);
            }
            return true;
        }
        public async Task<bool> BlockUser(string blockerUsername, string blockedUsername)
        {
            var blocker = await _unitOfWork.Users.GetUserByUsername(blockerUsername);
            var blocked = await _unitOfWork.Users.GetUserByUsername(blockedUsername);
            if (blocker == null || blocked == null)
            {
                return false;
            }
            var friendship = await _unitOfWork.FriendShips.GetExistingFriendshipAsync(blocker.Id, blocked.Id, FriendshipQueryMode.OneWay);
            if (friendship != null && friendship.Status != FriendshipStatus.Blocked)
            {

                friendship.Status = FriendshipStatus.Blocked;
            }
            else if (friendship != null && friendship.Status == FriendshipStatus.Blocked)
            {
                return false;
            }
            else
            {

                friendship = new Friendship
                {
                    RequesterId = blocker.Id,
                    AddresseeId = blocked.Id,
                    Status = FriendshipStatus.Blocked
                };
                await _unitOfWork.FriendShips.AddFriendshipAsync(friendship);
            }
            await _unitOfWork.FriendShips.SaveChange();
            try
            {
                await _hubContext.Clients.User(blockerUsername).SendAsync("UserBlocked", blockedUsername);
                await _hubContext.Clients.User(blockedUsername).SendAsync("YouHaveBeenBlocked", blockerUsername);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending SignalR notification:" + ex.Message);
            }
            return true;
        }
        public async Task<bool> UnblockUser(string requesterUsername, string addresseeUsername)
        {
            var addressee = await _unitOfWork.Users.GetUserByUsername(addresseeUsername);
            var requester = await _unitOfWork.Users.GetUserByUsername(requesterUsername);
            if (requester == null || addressee == null)
            {
                return false;
            }
            var friendship = await _unitOfWork.FriendShips.GetExistingFriendshipAsync(requester.Id, addressee.Id, FriendshipQueryMode.OneWay);
            if (friendship == null)
            {
                return false;
            }
            var convert_friendship = await _unitOfWork.FriendShips.GetExistingFriendshipAsync(requester.Id, addressee.Id, FriendshipQueryMode.Reverse);
            if (convert_friendship != null && convert_friendship.Status == FriendshipStatus.Accepted)
            {
                friendship.Status = FriendshipStatus.Accepted;
                await _unitOfWork.FriendShips.UpdateAsync(friendship);
            }
            if (friendship.Status == FriendshipStatus.Blocked)
            {
                await _unitOfWork.FriendShips.Remove(friendship);
            }
            await _unitOfWork.FriendShips.SaveChange();
            try
            {
                await _hubContext.Clients.User(requesterUsername).SendAsync("UserUnblocked", addresseeUsername);
                await _hubContext.Clients.User(addresseeUsername).SendAsync("YouHaveBeenUnblocked", requesterUsername);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending SignalR notification:" + ex.Message);
            }
            return true;
        }
        public async Task<List<FriendRequestDto>> GetPendingFriendRequests(string username)
        {
            var user = await _unitOfWork.Users.GetUserByUsername(username);

            if (user == null)
            {
                return new List<FriendRequestDto>();
            }
            var pendingRequests = await _unitOfWork.FriendShips.GetPendingFriendRequestsAsync(user.Id);

            try
            {
                if (pendingRequests != null && pendingRequests.Count > 0)
                {
                    await _hubContext.Clients.User(username).SendAsync("PendingRequestsFetched", pendingRequests.Select(f => new FriendRequestDto
                    {
                        Username = f.Requester.Username,
                        DisplayName = f.Requester.DisplayName,
                        Hashtag = f.Requester.Hashtag,
                        Avatar = f.Requester.Avatar,
                        Status = f.Requester.Status.ToString(),
                        Banner = f.Requester.Banner,
                        Pronouns = f.Requester.Pronouns,
                        About = f.Requester.About,
                        Birthdate = f.Requester.Birthdate
                    }).ToList());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SignalR notification: {ex.Message}");
                
            }

            return pendingRequests.Select(f => new FriendRequestDto
            {
                Username = f.Requester.Username,
                DisplayName = f.Requester.DisplayName,
                Hashtag = f.Requester.Hashtag,
                Avatar = f.Requester.Avatar,
                Status = f.Requester.Status.ToString(),
                Banner = f.Requester.Banner,
                Pronouns = f.Requester.Pronouns,
                About = f.Requester.About,
                Birthdate = f.Requester.Birthdate
            }).ToList();


        }
        public async Task<List<FriendRequestDto>> ShowAcceptedFriends(string username)
        {
            var user = await _unitOfWork.Users.GetUserByUsername(username);

            if (user == null)
            {
                return new List<FriendRequestDto>();
            }

            var acceptedFriends = await _unitOfWork.FriendShips.GetAcceptedFriendsAsync(user.Id);

            var acceptedFriendDtos = acceptedFriends.Select(f => new FriendRequestDto
            {
                UserId = f.RequesterId == user.Id ? f.Addressee.Id : f.Requester.Id,
                Username = f.RequesterId == user.Id ? f.Addressee.Username : f.Requester.Username,
                DisplayName = f.RequesterId == user.Id ? (string.IsNullOrEmpty(f.ReminderName) ? f.Addressee.DisplayName : f.ReminderName) : (string.IsNullOrEmpty(f.ReminderName) ? f.Requester.DisplayName : f.ReminderName),
                Hashtag = f.RequesterId == user.Id ? f.Addressee.Hashtag : f.Requester.Hashtag,
                Avatar = f.RequesterId == user.Id ? f.Addressee.Avatar : f.Requester.Avatar,
                Status = f.RequesterId == user.Id ? f.Addressee.Status.ToString() : f.Requester.Status.ToString(),
                Banner = f.RequesterId == user.Id ? f.Addressee.Banner : f.Requester.Banner,
                Pronouns = f.RequesterId == user.Id ? f.Addressee.Pronouns : f.Requester.Pronouns,
                About = f.RequesterId == user.Id ? f.Addressee.About : f.Requester.About,
                Birthdate = f.RequesterId == user.Id ? f.Addressee.Birthdate : f.Requester.Birthdate
            }).ToList();

            try
            {
                if (acceptedFriendDtos.Any())
                {
                    await _hubContext.Clients.User(username).SendAsync("AcceptedFriendsFetched", acceptedFriendDtos);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SignalR notification: {ex.Message}");
                
            }

            return acceptedFriendDtos;
        }
        public async Task<List<FriendRequestDto>> ShowOnlineFriends(string username)
        {
            var user = await _unitOfWork.Users.GetUserByUsername(username);

            if (user == null)
            {
                return new List<FriendRequestDto>();
            }

            var onlineFriends = await _unitOfWork.FriendShips.GetOnlineFriendsAsync(user.Id);

            var onlineFriendDtos = onlineFriends.Select(f => new FriendRequestDto
            {
                Username = f.RequesterId == user.Id ? f.Addressee.Username : f.Requester.Username,
                UserId = f.RequesterId == user.Id ? f.Addressee.Id : f.Requester.Id,
                DisplayName = f.RequesterId == user.Id ? (string.IsNullOrEmpty(f.ReminderName) ? f.Addressee.DisplayName : f.ReminderName) : (string.IsNullOrEmpty(f.ReminderName) ? f.Requester.DisplayName : f.ReminderName),
                Avatar = f.RequesterId == user.Id ? f.Addressee.Avatar : f.Requester.Avatar,
                Hashtag = f.RequesterId == user.Id ? f.Addressee.Hashtag : f.Requester.Hashtag,
                Banner = f.RequesterId == user.Id ? f.Addressee.Banner : f.Requester.Banner,
                Pronouns = f.RequesterId == user.Id ? f.Addressee.Pronouns : f.Requester.Pronouns,
                About = f.RequesterId == user.Id ? f.Addressee.About : f.Requester.About,
                Birthdate = f.RequesterId == user.Id ? f.Addressee.Birthdate : f.Requester.Birthdate,
                Status = f.RequesterId == user.Id ? f.Addressee.Status.ToString() : f.Requester.Status.ToString()
            }).ToList();
            try
            {
                if (onlineFriendDtos.Any())
                {
                    await _hubContext.Clients.User(username).SendAsync("OnlineFriendsFetched", onlineFriendDtos);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SignalR notification: {ex.Message}");

            }

            return onlineFriendDtos;
        }
        public async Task<List<FriendRequestDto>> GetBlockedUsers(string username)
        {
            var user = await _unitOfWork.Users.GetUserByUsername(username);

            if (user == null)
            {
                return new List<FriendRequestDto>();
            }

            var blockedUsers = await _unitOfWork.FriendShips.GetBlockedUsersAsync(user.Id);

            var blockedFriendDtos = blockedUsers.Select(f => new FriendRequestDto
            {
                Username = f.Addressee.Username,
                DisplayName = f.Addressee.DisplayName,
                Hashtag = f.Addressee.Hashtag,
                Avatar = f.Addressee.Avatar,
                Status = f.Requester.Status.ToString(),
                Banner = f.Addressee.Banner,
                Pronouns = f.Addressee.Pronouns,
                About = f.Addressee.About,
                Birthdate = f.Addressee.Birthdate
            }).ToList();
            try
            {
                if (blockedFriendDtos.Any())
                {
                    await _hubContext.Clients.User(username).SendAsync("BlockedFriendsFetched", blockedFriendDtos);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SignalR notification: {ex.Message}");

            }
            return blockedFriendDtos;
        }
        public async Task<UserDetailDto> GetUserDetailsByUsername(string username, Guid currentUserId)
        {
            var user = await _unitOfWork.Users.GetUserByUsername(username);

            if (user == null)
                return null;
            var friendship_id=await _unitOfWork.FriendShips.GetExistingFriendshipAsync(currentUserId,user.Id,FriendshipQueryMode.OneWay);
            List<string> mutualFriends = new List<string>();
            List<string> mutualServers = new List<string>();

            if (user.Username.Equals(username))
            {
                // Get mutual friends
                mutualFriends = await _unitOfWork.FriendShips.GetMutualFriendsAsync(currentUserId, user.Id);

                // Get mutual servers
                mutualServers = (await _unitOfWork.FriendShips.GetMutualServersAsync(currentUserId, user.Id))
                           .Select(serverId => serverId.ToString())
                           .ToList();
            }
            if(friendship_id.ReminderName!=null)
            {
                return new UserDetailDto
                {
                    userId = user.Id,
                    DisplayName = friendship_id.ReminderName,
                    Username = user.Username,
                    Hashtag = user.Hashtag,
                    Banner = user.Banner,
                    Status = user.Status.ToString(),
                    About = user.About,
                    JoinedAt = user.JoinedAt,
                    MutualFriends = mutualFriends,
                    MutualServers = mutualServers
                };
            }
            return new UserDetailDto
            {
                userId = user.Id,
                DisplayName = user.DisplayName,
                Username = user.Username,
                Hashtag = user.Hashtag,
                Banner = user.Banner,
                Status = user.Status.ToString(),
                About = user.About,
                JoinedAt = user.JoinedAt,
                MutualFriends = mutualFriends,
                MutualServers = mutualServers
            };
        }

        public async Task<bool> AddReminderNameAsync(Guid userId, Guid targetId, string reminderName)
        {
            if (string.IsNullOrEmpty(reminderName))
            {
                throw new InvalidDataException("Reminder name is required");
            }

            var existedFriendship = await _unitOfWork.FriendShips.GetExistingFriendshipAsync(userId, targetId, FriendshipQueryMode.OneWay);
            if (existedFriendship == null)
            {
                throw new InvalidOperationException("Friendship is required to set reminder name");
            }

            existedFriendship.ReminderName = reminderName;
            await _unitOfWork.FriendShips.SaveChange();

            
            try
            {
                await _hubContext.Clients.User(userId.ToString()).SendAsync("ReminderNameUpdated", new
                {
                    TargetId = targetId,
                    ReminderName = reminderName
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] ReminderNameUpdated error: {ex.Message}");
            }

            return true;
        }
    }
}
