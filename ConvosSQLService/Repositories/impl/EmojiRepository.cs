using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class EmojiRepository : GenericRepository<Emoji>, IEmojiRepository
    {
        private readonly ConvosDbContext _context;

        public EmojiRepository(ConvosDbContext context) : base (context)
        {
            _context = context;
        }

        public async Task<List<Emoji>> GetAllServerEmoAsync(Guid serverId)
        {
            try
            {
                return await _context.Emojis
                                     .Include(e => e.Server).ThenInclude(s =>s.ServerMembers)
                                     .Where(e => e.ServerId.Equals(serverId))
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching Emojies: " + ex.Message, ex);
            }
        }

    }
}
