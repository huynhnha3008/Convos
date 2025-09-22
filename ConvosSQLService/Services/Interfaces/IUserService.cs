
using BusinessObjects.DTOs.UserDto;
using BusinessObjects.Models;
namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserRequest>> GetAllUsersAsync();
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByUsername(string username);
        Task SaveUser(User user);
        Task<User> GetUserByPhonenumber(string phonenumber);
        Task<User> GetUserById(Guid id);
        Task UpdateUser(User user);
       
        Task<User> DeleteUserAsync(Guid userId);
    }
}
