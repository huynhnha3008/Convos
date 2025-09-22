using Microsoft.AspNetCore.SignalR;
using Services.SignalR.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SignalR
{
    public class QuizHub : Hub<IQuizHub>
    {

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


        public async Task CreateQuiz(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).CreateQuiz(serverId, name);
        }
        public async Task DeleteQuiz(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).DeleteQuiz(serverId, name);
        }

        public async Task UpdateQuiz(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).UpdateQuiz(serverId, name);
        }
    }
}
