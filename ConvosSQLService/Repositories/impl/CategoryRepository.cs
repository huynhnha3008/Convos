using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly ConvosDbContext _context;

        public CategoryRepository(ConvosDbContext context) : base(context) 
        {
            _context = context;
        }


        public async Task<List<Category>> GetAllAsync(Guid serverId)
        {
            try
            {
                return await _context.Categories
                                     .Where(c => c.ServerId.Equals(serverId))
                                     .Include(x => x.Channels)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching Categories: " + ex.Message, ex);
            }
        }

    }
}
