using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetUserByUsername(string username);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByPhonenumber(string phonenumber);
        Task<bool> SaveUser(User user);
        Task<bool> UpdateUser(User user);

    }
}
