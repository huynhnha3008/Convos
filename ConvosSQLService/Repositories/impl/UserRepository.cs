using Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Services.impl;
using BusinessObjects.Models;
using Vonage.Conversations;



namespace Services
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ConvosDbContext _context;

        public UserRepository(ConvosDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }


        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByPhonenumber(string phonenumber)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phonenumber);
        }


        public async Task<bool> SaveUser(User user)
        {
            _context.Users.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }


        public async Task<bool> UpdateUser(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
