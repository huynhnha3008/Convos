using BusinessObjects.Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IEventRepository : IGenericRepository<Event>
    {
        Task<List<Event>> GetAllInServerAsync(Guid serverId);
        Task<List<Event>> GetAllInChannelAsync(Guid channelId);
        Task<List<Event>> SearchInChannelAsync(Guid channelId, string keyword);
        Task<List<Event>> SearchInServerAsync(Guid serverId, string keyword);
        Task<Event> GetByTitleAsync(string name);    
    }
}
