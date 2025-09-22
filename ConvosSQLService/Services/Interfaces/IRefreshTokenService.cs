using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> GetRefreshToken(string refreshtoken);
        Task Update(RefreshToken refreshToken);
    }
}
