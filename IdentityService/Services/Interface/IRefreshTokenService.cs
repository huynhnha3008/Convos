

using BusinessObjects.Models;

namespace Services.Interface
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> GetRefreshToken(string refreshtoken);
        Task Update(RefreshToken refreshToken);
    }
}
