using BusinessObjects.DTOs;
using BusinessObjects.QueryObject;
using Microsoft.AspNetCore.Http;

namespace Services.Interfaces
{
    public interface IEmojiService
    {
        Task<EmojiCreateResponse> CreateAsync(EmojiCreateRequest emojiCreateRequest, Guid userId, IFormFile imageFile);
        Task<List<EmojiCreateResponse>> GetAllAsync(Guid serverId, QueryEmoji query);

        Task<EmojiCreateResponse> GetByIdAsync(Guid id);

        Task<string> UpdateAsync(EmojiUpdateRequest request, Guid userId, IFormFile imageFile);

        Task<List<EmojiCreateResponse>> SearchAsync(Guid serverId, QueryEmoji query);

        Task<string> DeleteAsync(Guid id, Guid userId);
    }
}
