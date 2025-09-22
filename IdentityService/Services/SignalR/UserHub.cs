
using BusinessObjects.Models;
using Microsoft.AspNetCore.SignalR;
using Services.SignalR.Interfaces;
using Services.Interface;

namespace Services.SignalR
{


    public class UserHub : Hub, IUserHub
    {
        private readonly IUserService _userService;

        public UserHub(IUserService userService)
        {
            _userService = userService;
        }
        //public override async Task OnConnectedAsync()
        //{
        //    var httpContext = Context.GetHttpContext();
        //    var username = httpContext.Request.Query["username"];  // Get the username from query string
        //    Console.WriteLine($"User connected: {username}");

        //    if (!string.IsNullOrEmpty(username))
        //    {
        //        // Add user to their group
        //        await Groups.AddToGroupAsync(Context.ConnectionId, username);

        //        // Notify others that the user is online
        //        await Clients.Others.SendAsync("UserOnline", username);

        //        // Fetch and send pending friend requests
        //        var pendingRequests = await _userService.GetPendingFriendRequests(username);
        //        await Clients.Caller.SendAsync("PendingRequestsFetched", pendingRequests);
        //    }

        //    await base.OnConnectedAsync();
        //}

        //public override async Task OnDisconnectedAsync(Exception? exception)
        //{
        //    var httpContext = Context.GetHttpContext();
        //    var username = httpContext.Request.Query["username"];  // Lấy tên người dùng từ query string

        //    if (!string.IsNullOrEmpty(username))
        //    {
        //        // Xóa người dùng khỏi nhóm khi họ ngắt kết nối
        //        await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);

        //        // Thông báo cho những người dùng khác rằng người dùng này đã offline
        //        await Clients.Others.SendAsync("UserOffline", username);
        //    }

        //    await base.OnDisconnectedAsync(exception);
        //}
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }


        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendUserUpdate(User user)
        {
            await Clients.All.SendAsync("ReceiveUserUpdate", user);
        }







        public async Task NotifyNewUser(User newUser)
        {
            await Clients.All.SendAsync("ReceiveNewUser", newUser);
        }
    }
}
