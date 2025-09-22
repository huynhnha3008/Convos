using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Models;

namespace Service.EmojiService
{
    public interface IEmojiService
    {
        Task<Emoji> GetEmojiByIdAsync(string id);
        Task<IEnumerable<Emoji>> GetAllEmojisAsync();
        Task<Emoji> CreateEmojiAsync(Emoji emoji);
        Task<Emoji> UpdateEmojiAsync(string id, Emoji emoji);
        Task<Emoji> DeleteEmojiAsync(string id);
    }
}
