using Google.Api.Gax;
using Services.Interface;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IConnectionMultiplexer _redis;

        public PasswordResetService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        public async Task DeleteTokenAsync(string token)
        {
            var database = _redis.GetDatabase();
            await database.KeyDeleteAsync(token);
        }

        public async Task<Guid?> GetUserIdByTokenAsync(string token)
        {
            var database = _redis.GetDatabase();

            
            var data = await database.StringGetAsync(token);
            if (string.IsNullOrEmpty(data))
                return null;

            var result = JsonSerializer.Deserialize<PasswordResetTokenData>(data);
            return result?.UserId;
        }

        public async Task SaveTokenAsync(string token, Guid userId, TimeSpan expiration)
        {
            var database = _redis.GetDatabase();

            
            var data = JsonSerializer.Serialize(new { UserId = userId });
            await database.StringSetAsync(token, data, expiration);
        }
    }
    public class PasswordResetTokenData
    {
        public Guid UserId { get; set; }
    }
}
