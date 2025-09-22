using BusinessObjects.Models;

namespace Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken> GetRefreshToken(string refreshtoken);
    }
}
