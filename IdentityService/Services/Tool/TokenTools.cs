using BusinessObjects.DTOs.UserDTO;
using BussinessObjects.Settings;
using Repositories.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.Tool
{
    public class TokenTools
    {
        private readonly TokenTool _tooken;
        public TokenTools(TokenTool tooken)
        {
            _tooken = tooken;
        }
        public async Task<TokenSetting> GenerateToken(UserModel user)
        {
            return await _tooken.GenerateToken(user);
        }

        public DateTime ConvertUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();

            return dateTimeInterval;
        }

    }
}
