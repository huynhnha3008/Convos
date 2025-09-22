using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SignalR.Interfaces
{
    public interface IEventHub
    {
        Task DeleteEvent(Guid serverId, string eventName);
        Task CreateEvent(Guid serverId, string eventName);
        Task UpdateEvent(Guid serverId, string eventName);
        Task AlertToServer(Guid serverId, string eventName);
    }
}
