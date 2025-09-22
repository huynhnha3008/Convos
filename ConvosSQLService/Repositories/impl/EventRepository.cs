using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Services.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.impl
{
    public class EventRepository : GenericRepository<Event>, IEventRepository
    {
        private ConvosDbContext _context;
        public EventRepository(ConvosDbContext context) : base(context)
        {
            _context = context; 
        }

        public async Task<List<Event>> GetAllInChannelAsync(Guid channelId)
        {
            return await _context.Events.Where(e => e.ChannelId.Equals(channelId)).ToListAsync();
        }

        public async Task<List<Event>> GetAllInServerAsync(Guid serverId)
        {
            return await _context.Events.Where(e => e.ServerId.Equals(serverId)).ToListAsync();

        }

        public async Task<Event> GetByTitleAsync(string name)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Title.Equals(name));
        }

        public async Task<List<Event>> SearchInChannelAsync(Guid channelId, string keyword)
        {
            return await _context.Events.Where(e => e.ChannelId.Equals(channelId) && (e.Title.Contains(keyword) || e.Description.Contains(keyword)))
                                        .ToListAsync();
        }

        public async Task<List<Event>> SearchInServerAsync(Guid serverId, string keyword)
        {
            return await _context.Events.Where(e => e.ServerId.Equals(serverId) && (e.Title.Contains(keyword) || e.Description.Contains(keyword)))
                                        .ToListAsync();
        }
    }
}
