using BusinessObjects.Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RefreshTokenService(IUnitOfWork unitOfWork)
        {
           _unitOfWork = unitOfWork;
        }
        public async Task<RefreshToken> GetRefreshToken(string refreshtoken)
        {
            return await _unitOfWork.RefreshTokens.GetRefreshToken(refreshtoken);
        }

        public async Task Update(RefreshToken refreshToken)
        {
            await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);
        }
    }
}
