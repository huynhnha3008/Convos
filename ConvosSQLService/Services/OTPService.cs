using Microsoft.Extensions.Caching.Distributed;
using Services.Interfaces;


namespace Services
{
    public class OTPService : IOTPService
    {
        private readonly IDistributedCache _redisCache;
        public OTPService(IDistributedCache redisCache)
        {
            _redisCache = redisCache;
        }
        public string GenerateOTP()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public async void StoreOTP(string keylogin, string otp)
        {
            var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            await _redisCache.SetStringAsync(keylogin, otp, options);
        }

        public async Task<bool> ValidateOTP(string keylogin, string otp)
        {
            var storedOtp = await _redisCache.GetStringAsync(keylogin);

            if (storedOtp == null) return false;
            return storedOtp == otp;
        }
    }
}
