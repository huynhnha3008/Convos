using BusinessObjects.Models;
using Services.Interface;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<User?> FindUserByEmail(string email)
        {
            return await _unitOfWork.Users.GetUserByEmail(email);
        }


        public async Task<User> FindUserById(Guid id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User?> FindUserByPhonenumber(string phonenumber)
        {
            return await _unitOfWork.Users.GetUserByPhonenumber(phonenumber);
        }

        public async Task<User?> FindUserByUsername(string username)
        {
            return await _unitOfWork.Users.GetUserByUsername(username);
        }
        public async Task SaveUser(User user)
        {
            await _unitOfWork.Users.CreateAsync(user);
        }
        public async Task UpdateUser(User user)
        {
            await _unitOfWork.Users.UpdateAsync(user);
        }
    }
}
