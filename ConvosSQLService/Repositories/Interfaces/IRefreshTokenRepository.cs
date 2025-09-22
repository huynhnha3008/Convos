using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken> GetRefreshToken(string refreshtoken);
    }
}
