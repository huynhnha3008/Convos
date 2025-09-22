using BusinessObjects.DTOs;
using BusinessObjects.DTOs.EventDto;
using BusinessObjects.QueryObject;
using BusinessObjects.QueryObjects;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IEventService
    {
        Task<EventCreateResponse> CreateAsync(EventCreateRequest request, Guid currentUserId);
        Task<List<EventCreateResponse>> GetAllInServerAsync(Guid serverId, QueryEvent query);  // handle paging then
        Task<List<EventCreateResponse>> GetAllInChannelAsync(Guid channelId, QueryEvent query);  // handle paging then

        Task<EventCreateResponse> GetByIdAsync(Guid id);

        Task<string> UpdateAsync(Guid id, EventUpdateRequest request);

        Task<string> DeleteAsync(Guid id);
    }
}
