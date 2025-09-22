
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        private readonly ConvosDbContext _context;
        public RefreshTokenRepository(ConvosDbContext context): base(context) 
        {
            _context = context;
        }


        public async Task<RefreshToken> GetRefreshToken(string refreshtoken)
        {
            try
            {
                return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshtoken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

   
    }
}
