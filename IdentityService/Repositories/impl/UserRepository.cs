using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories.impl
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ConvosDbContext _context;

        public UserRepository(ConvosDbContext context) : base(context)
        {
            _context = context;
        }



        // Fetch a user by their username
        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        // Fetch a user by their email
        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }




        // Fetch a user by their phone number
        public async Task<User?> GetUserByPhonenumber(string phonenumber)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phonenumber);
        }

        // Save a new user to the database
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
