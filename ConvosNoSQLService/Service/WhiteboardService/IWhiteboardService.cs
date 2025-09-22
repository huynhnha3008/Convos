using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Dtos;
using BusinessObject.RequestObjects;

namespace Service.WhiteboardService
{
    public interface IWhiteboardService
    {
        Task<WhiteboardDto> CreateWhiteboardAsync(string userId, CreateWhiteboardRequest request);
        Task<WhiteboardDto> GetWhiteboardByIdAsync(string whiteboardId);
        Task<List<WhiteboardDto>> GetChannelWhiteboardsAsync(string channelId, int skip = 0, int limit = 50);
        Task<WhiteboardDto> UpdateWhiteboardAsync(string userId, string whiteboardId, UpdateWhiteboardRequest request);
        Task<bool> DeleteWhiteboardAsync(string userId, string whiteboardId);
        Task<bool> AddCollaboratorAsync(string userId, string whiteboardId, string collaboratorId);
        Task<bool> RemoveCollaboratorAsync(string userId, string whiteboardId, string collaboratorId);
        Task<bool> PinWhiteboardAsync(string userId, string whiteboardId);
        Task<bool> UnpinWhiteboardAsync(string userId, string whiteboardId);
        Task<bool> MarkAsReadAsync(string userId, string whiteboardId);
        Task<WhiteboardDto> AddTagAsync(string userId, string whiteboardId, string tag);
        Task<WhiteboardDto> RemoveTagAsync(string userId, string whiteboardId, string tag);
    }
}