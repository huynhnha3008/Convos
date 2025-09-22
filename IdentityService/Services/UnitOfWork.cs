
using BusinessObjects.Models;
using Repositories.impl;
using Repositories.Interfaces;
using Services.Interface;

namespace Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ConvosDbContext _context;
        private IUserRepository _user;
        private IRefreshTokenRepository _refreshToken;
        public UnitOfWork(ConvosDbContext context)
        {
            _context = context;
        }
        public IRefreshTokenRepository RefreshTokens => _refreshToken ??= new RefreshTokenRepository(_context);
        public IUserRepository Users => _user ??= new UserRepository(_context);
        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
