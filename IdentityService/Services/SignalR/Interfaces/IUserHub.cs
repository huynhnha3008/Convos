

using BusinessObjects.Models;

namespace Services.SignalR.Interfaces
{
    public interface IUserHub
    {
        Task OnConnectedAsync();
        Task OnDisconnectedAsync(Exception? exception);
        Task SendUserUpdate(User user);


        Task NotifyNewUser(User newUser);
    }
}
