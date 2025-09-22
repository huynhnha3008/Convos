using BusinessObjects.DTOs;
using BusinessObjects.DTOs.EventDto;
using BusinessObjects.DTOs.RoleDto;
using BusinessObjects.Models;
using BusinessObjects.QueryObjects;
using Microsoft.AspNetCore.SignalR;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.SignalR;
using Services.SignalR.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;
        private readonly IHubContext<EventHub, IEventHub> _hubContext;


        public EventService(IUnitOfWork unitOfWork, IPermissionService permissionService, IRoleService roleService, IHubContext<EventHub, IEventHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;
            _roleService = roleService;
            _hubContext = hubContext;

        }
        public async Task<EventCreateResponse> CreateAsync(EventCreateRequest request, Guid userId)
        {
            //check creator
            var member = await _unitOfWork.ServerMembers.GetMemberIncludeUserAsync(request.creatorId);
            if (member == null)
            {
                throw new InvalidDataException("Member is not found");
            }
            var channel = await _unitOfWork.Channels.GetSimpleChannelAsync(request.channelId);
            if (channel == null)
            {
                throw new InvalidDataException("Channel is not found");
            }

            var existedEvent = await _unitOfWork.Events.GetByTitleAsync(request.title);
            if (existedEvent != null)
            {
                throw new InvalidOperationException("Event's title has existed");
            }

            if (!channel.Type.ToString().Equals("Stage"))
            {
                throw new InvalidOperationException("Event can only be created in Stage Channel");
            }
            if (!channel.ServerId.Equals(member.ServerId))
            {
                throw new InvalidDataException("Channel and Member are not in the same server");
            }

            var rolesInServer = await _unitOfWork.Roles.GetRolesByServerIdAsync(channel.ServerId);
            List<Role> roles = new List<Role>();
            foreach (var r in request.roleIds)
            {
                foreach (var role in rolesInServer)
                {
                    if (role.Id.Equals(r.id))
                    {
                        roles.Add(role);
                    }
                }
            }
            if (roles.Count == 0)
            {
                throw new InvalidOperationException("No roles is been added");
            }

            foreach (var role in roles)
            {
                await _roleService.AddRoleToChannel(request.channelId, role.Id, userId);
            }

            if (request.endAt < request.startAt)
            {
                throw new InvalidDataException("Invalid Start and End of Event");
            }

            if (request.endAt < DateTime.UtcNow)
            {
                throw new InvalidDataException("Invalid End Date of Event");
            }
            Event e = new Event
            {
                StartAt = request.startAt,
                EndAt = request.endAt,
                ChannelId = channel.Id,
                CreatedAt = DateTime.UtcNow,
                Description = request.description,
                Title = request.title,
                ServerId = member.ServerId,
                Status = true,
                CreatorId = member.Id
            };
            var createdEvent = await _unitOfWork.Events.CreateAsync(e);
            await _unitOfWork.Events.UpdateAsync(createdEvent);

            await _hubContext.Clients.Group(channel.ServerId.ToString()).CreateEvent(channel.ServerId, createdEvent.Title);

            return ToCreateResponse(createdEvent);
        }



        private EventCreateResponse ToCreateResponse(Event e)
        {
            EventCreateResponse response = new EventCreateResponse
            {
                updatedAt = e.UpdatedAt,
                createdAt = e.CreatedAt,
                description = e.Description,
                title = e.Title,
                serverId = e.ServerId,
                status = e.Status,
                channelId = e.ChannelId,
                creatorId = e.CreatorId,
                endAt = e.EndAt,
                id = e.Id,
                startAt = e.StartAt
            };
            return response;
        }



        public async Task<string> DeleteAsync(Guid id)
        {
            var eve = await _unitOfWork.Events.GetByIdAsync(id);
            if (eve == null)
            {
                throw new InvalidDataException("Event is not found");
            }

            // check if Event is on going or not 
            var isOnGoing = false;
            if(eve.StartAt < DateTime.Now && eve.EndAt > DateTime.Now)
            {
                isOnGoing = true;
            }

            if(isOnGoing)
            {
                throw new InvalidOperationException("Cannot delete on-going Event");
            }
            // handle permission 
            //if (!userId.Equals(server.OwnerId))
            //{
            //    var hasManageChannelPermission = await HasManageEmojiPermissionAsync(emo.ServerId, userId);
            //    {
            //        if (!hasManageChannelPermission)
            //        {
            //            throw new UnauthorizedAccessException("Permission is denied: You don't permission to DELETE event in this server");
            //        }
            //    }
            //}
            await _unitOfWork.Events.DeleteAsync(eve);
            await _hubContext.Clients.Group(eve.ServerId.ToString()).CreateEvent(eve.ServerId, eve.Title);

            return "Event is deleted successfully";
        }

        public async Task<List<EventCreateResponse>> GetAllInChannelAsync(Guid channelId, QueryEvent query)
        {
            var channel = await _unitOfWork.Channels.GetByIdAsync(channelId);
            if (channel == null)
            {
                throw new InvalidDataException("Channel is not found");
            }
            var queryEvents = await _unitOfWork.Events.SearchInChannelAsync(channelId, query.SearchTerm);

            if (queryEvents == null)
            {
                return new List<EventCreateResponse>();
            }

            queryEvents = query.IsDescending ? queryEvents.OrderByDescending(e => e.Title).ToList() : queryEvents.OrderBy(e => e.Title).ToList();
            List<EventCreateResponse> response = new List<EventCreateResponse>();
            foreach (var e in queryEvents)
            {
                response.Add( ToCreateResponse(e));
            }

            var paginatedEvents = response
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            return paginatedEvents;
        }

        public async Task<List<EventCreateResponse>> GetAllInServerAsync(Guid serverId, QueryEvent query)
        {
            if (query.SearchTerm == null)
            {
                query.SearchTerm = "";
            }
            var server = await _unitOfWork.Servers.GetByIdAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            var queryEvents = await _unitOfWork.Events.SearchInServerAsync(serverId, query.SearchTerm);

            if (queryEvents == null)
            {
                return new List<EventCreateResponse>();
            }

            queryEvents = query.IsDescending ? queryEvents.OrderByDescending(e => e.Title).ToList() : queryEvents.OrderBy(e => e.Title).ToList();
            List<EventCreateResponse> response = new List<EventCreateResponse>();
            foreach (var e in queryEvents)
            {
                response.Add(ToCreateResponse(e));
            }

            var paginatedEvents = response
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            return paginatedEvents;
        }

        public async Task<EventCreateResponse> GetByIdAsync(Guid id)
        {
            var eve = await _unitOfWork.Events.GetByIdAsync(id);
            if (eve == null)
            {
                throw new InvalidDataException("Event is not found");
            }
            return ToCreateResponse(eve);
        }

        public async Task<string> UpdateAsync(Guid id, EventUpdateRequest request)
        {
            var eve = await _unitOfWork.Events.GetByIdAsync(id);
            if (eve == null)
            {
                throw new InvalidDataException("Event is not found");
            }

            // check if Event is on going or not 
            var isOnGoing = false;
            if (eve.StartAt < DateTime.Now && eve.EndAt > DateTime.Now)
            {
                isOnGoing = true;
            }

            if (isOnGoing)
            {
                throw new InvalidOperationException("Cannot update on-going Event");
            }

            var existedEvent = await _unitOfWork.Events.GetByTitleAsync(request.title); 
            if(existedEvent != null)
            {
                throw new InvalidOperationException("New name has existed - update failed");
            }
            if (request.endAt < request.startAt)
            {
                throw new InvalidDataException("Invalid Start and End of Event");
            }
            if (request.title != null)
            {
                eve.Title = request.title;
            }

            if (request.description != null)
            {
                eve.Description = request.description;
            }
            if (request.startAt != DateTime.MinValue)
            {
                eve.StartAt = request.startAt;
            }

            if (request.endAt != DateTime.MinValue)
            {
                eve.EndAt = request.endAt;
            }

            var updatedEvent = await _unitOfWork.Events.UpdateAsync(eve);
            await _hubContext.Clients.Group(eve.ServerId.ToString()).CreateEvent(eve.ServerId, eve.Title);

            return "Update sucessfully";
        }
    }
}
