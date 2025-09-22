
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IUserService
    {
        Task<User?> FindUserByEmail(string email);
        Task<User?> FindUserByUsername(string username);
        Task SaveUser(User user);
        Task<User?> FindUserByPhonenumber(string phonenumber);
        Task<User> FindUserById(Guid id);
        Task UpdateUser(User user);

    }
}
