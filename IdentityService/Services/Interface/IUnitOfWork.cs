using Repositories.Interfaces;

namespace Services.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IRefreshTokenRepository RefreshTokens { get; }
        IUserRepository Users { get; }

        int Complete();
    }
}
