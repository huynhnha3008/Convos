using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SignalR.Interfaces
{
    public interface IEmojiHub
    {
        Task DeleteEmoji(Guid serverId, string EmojiName);
        Task CreateEmoji(Guid serverId, string EmojiName);
        Task UpdateEmoji(Guid serverId, string EmojiName);
        Task AlertToServer(Guid serverId, string EmojiName);
    }
}
