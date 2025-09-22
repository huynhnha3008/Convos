using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IEmojiRepository : IGenericRepository<Emoji>
    {

        Task<List<Emoji>> GetAllServerEmoAsync(Guid serverId);

    }
}
