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
    public class SoundBoardRepository : GenericRepository<SoundBoard>, ISoundBoardRepository
    {
        private readonly ConvosDbContext _context;

        public SoundBoardRepository(ConvosDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<SoundBoard>> GetAllServerSoundAsync(Guid serverId)
        {
            try
            {
                return await _context.SoundBoards
                                     .Include(e => e.Server).ThenInclude(s => s.ServerMembers)
                                     .Where(e => e.ServerId.Equals(serverId))
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching SoundBoards: " + ex.Message, ex);
            }
        }
    }
}
