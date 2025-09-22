using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IPasswordResetService
    {
        Task SaveTokenAsync(string token, Guid userId, TimeSpan expiration);
        Task<Guid?> GetUserIdByTokenAsync(string token);
        Task DeleteTokenAsync(string token);
    }
}
