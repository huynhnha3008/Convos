using BusinessObjects.DTOs;
using BusinessObjects.DTOs.ChannelDto;
using BusinessObjects.Models;
using BusinessObjects.QueryObject;

namespace Services.Interfaces
{
    public interface IChannelService
    {
         Task<List<ChannelDetailResponse>> GetAllByServerIdAsync(Guid serverId, QueryChannel query);
         Task<ChannelDetailResponse> GetByIdAsync(Guid id);
         Task<Channel> CreateAutoAsync(Channel channel);
         Task<ChannelDetailResponse> CreateCustomAsync(ChannelCreateRequest request, Guid userId);
         Task<ChannelDetailResponse> UpdateAsync(Guid id, Guid userId, ChannelUpdateRequest request);

         Task SetChannelPrivacyAsync(Guid channelId, bool isPrivate);
         Task<List<ChannelDetailResponse>> SearchAsync(Guid serverId, QueryChannel query);

         Task DeleteAsync(Guid id, Guid userId);
         Task<string> ChangePositionAsync(Guid channelId, int newPosition, Guid userId);

        Task<List<Channel>> GetAllByRoleIdAsync(Guid roleId);
    }
}
