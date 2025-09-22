using BusinessObjects.DTOs.UserDto;
using BusinessObjects.Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<User> GetUserByEmail(string email)
        {
            return await _unitOfWork.Users.GetUserByEmail(email);
        }
        public async Task<List<UserRequest>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();

            return users.Select(u => new UserRequest
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Avatar = u.Avatar,
                Status = u.Status,
                Role = u.Role.ToString(),
                Banner = u.Banner,
                Pronouns = u.Pronouns,
                About = u.About,
                Hashtag = u.Hashtag,
                Birthdate = u.Birthdate,
                IsVerified = u.IsVerified,
                JoinedAt = u.JoinedAt,
                Servers = u.ServerMembers.Select(sm => new ServerDto
                {
                    Id = sm.Server.Id,
                    Name = sm.Server.Name,
                    CreatedAt = sm.Server.CreatedAt
                }).ToList()
            }).ToList();
        }

            public async Task<User> GetUserById(Guid id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User> GetUserByPhonenumber(string phonenumber)
        {
            return await _unitOfWork.Users.GetUserByPhonenumber(phonenumber);
        }

        public async Task<User> GetUserByUsername(string username)
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
       

        public async Task<User> DeleteUserAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if(user == null )
            {
                throw new InvalidDataException("User is not found");
            }
             return await _unitOfWork.Users.DeleteAsync(user);
        }
    }
}


