using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Models;
using Repository.UnitOfWork;

namespace Service.EmojiService
{
    public class EmojiService : IEmojiService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmojiService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Emoji> CreateEmojiAsync(Emoji emoji)
        {
            var createdEmoji = await _unitOfWork.Emojis.AddAsync(emoji);
            await _unitOfWork.SaveAsync();

            return createdEmoji;
        }

        public async Task<Emoji> DeleteEmojiAsync(string id)
        {
            var deletedEmoji = await _unitOfWork.Emojis.DeleteAsync(id);

            await _unitOfWork.SaveAsync();
            return deletedEmoji;
        }

        public async Task<IEnumerable<Emoji>> GetAllEmojisAsync()
        {
            return await _unitOfWork.Emojis.GetAllAsync();
        }

        public async Task<Emoji> GetEmojiByIdAsync(string id)
        {
            return await _unitOfWork.Emojis.GetByIdAsync(id);
        }

        public async Task<Emoji> UpdateEmojiAsync(string id, Emoji emoji)
        {
            var updatedEmoji = await _unitOfWork.Emojis.UpdateAsync(id, emoji);
            await _unitOfWork.SaveAsync();

            return updatedEmoji;
        }
    }
}