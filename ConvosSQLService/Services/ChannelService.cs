using BusinessObjects.DTOs;
using BusinessObjects.DTOs.ChannelDto;
using BusinessObjects.DTOs.EventDto;
using BusinessObjects.DTOs.RealTImeDto;
using BusinessObjects.Models;
using BusinessObjects.QueryObject;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.SignalR;
using Services.SignalR.Interfaces;

namespace Services
{
    public class ChannelService : IChannelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ServerHub, IServerHub> _hubContext;
        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;
        private readonly StackExchange.Redis.IDatabase _redisDatabase;

        public ChannelService(IUnitOfWork unitOfWork, IHubContext<ServerHub, IServerHub> hubContext, IPermissionService permissionService,
                              IRoleService roleService, StackExchange.Redis.IConnectionMultiplexer redis)

        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _permissionService = permissionService;
            _roleService = roleService;
            _redisDatabase = redis.GetDatabase();
        }


        public async Task<Channel> CreateAutoAsync(Channel channel)
        {
            int CategoryChannelSize = 0;
            var cate = await _unitOfWork.Categories.GetByIdAsync(channel.CategoryId.Value);

            if (cate.Channels == null || !cate.Channels.Any())
            {
                CategoryChannelSize = 1;
            }
            else
            {
                CategoryChannelSize = cate.Channels.Count() + 1;
            }
            channel.CreatedAt = DateTime.Now;
            channel.Position = CategoryChannelSize;
            channel.UpdatedAt = DateTime.Now;
            return await _unitOfWork.Channels.CreateAsync(channel);
        }

        private async Task<bool> HasManageChannelPermissionAsync(Guid serverId, Guid userId)
        {
            var userPermission = await _permissionService.GetUserGlobalPermission(userId, serverId);
            if (userPermission == null)
            {
                return false;
            }
            var hasManageChannelPermission = userPermission
                .Any(p => p.Code.Equals(PermissionEnum.MANAGE_CHANNELS.ToString()));

            if (!hasManageChannelPermission)
            {
                return false;
            }
            return true;
        }

        private ChannelRealtimeResponse ToRealtimeResponse (Channel channel)
        {
            var rs = new ChannelRealtimeResponse
            {
                Id = channel.Id,
                CreatedAt = channel.CreatedAt,
                IsPrivate = channel.IsPrivate,
                Name = channel.Name,
                Position = channel.Position,
                Type = (int)channel.Type,
                UpdatedAt = channel.UpdatedAt,
            };
            if(channel.CategoryId != null)
            {
                rs.CategoryId = channel.CategoryId.Value;
            }
            return rs;
        }

        public async Task<ChannelDetailResponse> CreateCustomAsync(ChannelCreateRequest request, Guid userId)
        {

            Channel channel = new Channel();
            ServerMember creatorMem = null;
            int position = 1;
            if (request.Type < 0 || request.Type > 4)
            {
                throw new InvalidOperationException("Only 3 Types of channel are accepted: 0. Text;  1. Voice;  2. Stage;  3. Whiteboard;  4. Docs");
            }
            if (request.CategoryId != null)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId.Value);
                if (category == null)
                {
                    throw new InvalidDataException("Category is not found");
                }
                var server = await _unitOfWork.Servers.GetServerIncludeMembersAsync(category.ServerId);

                foreach (var m in server.ServerMembers)
                {
                    if (m.UserId.Equals(userId))
                    {
                        creatorMem = m;
                    }
                }

                if (!userId.Equals(server.OwnerId))
                {
                    var hasManageChannelPermission = await HasManageChannelPermissionAsync(category.ServerId, userId);
                    {
                        if (!hasManageChannelPermission)
                        {
                            throw new UnauthorizedAccessException("Permission denied: You do not have permission to CREATE channel in this server");
                        }
                    }
                }
                var allChannelsInCategory = category.Channels.OrderBy(c => c.Type).ThenBy(c => c.Position).ToList();
                foreach (var existingChannel in allChannelsInCategory)
                {
                    existingChannel.Position = position++;
                }

                channel.Position = position;
                channel.CategoryId = category.Id;
                channel.ServerId = category.ServerId;

            }
            else if (request.ServerId != null && request.CategoryId == null)
            {
                var server = await _unitOfWork.Servers.GetByIdAsync(request.ServerId.Value);
                if (server == null)
                {
                    throw new InvalidDataException("Server is not found");
                }

                foreach (var m in server.ServerMembers)
                {
                    if (m.UserId.Equals(userId))
                    {
                        creatorMem = m;
                    }
                }

                if (!userId.Equals(server.OwnerId))
                {
                    var hasManageChannelPermission = await HasManageChannelPermissionAsync(server.Id, userId);
                    {
                        if (!hasManageChannelPermission)
                        {
                            throw new UnauthorizedAccessException("Permission denied: You do not have permission to CREATE channel in this server");
                        }
                    }
                }
                var allChannelsInServer = server.Channels.Where(c => c.CategoryId == null).OrderBy(c => c.Type).ThenBy(c => c.Position).ToList();

                foreach (var existingChannel in allChannelsInServer)
                {
                    existingChannel.Position = position++;
                }

                channel.Position = position;
                channel.ServerId = server.Id;

            }
            else
            {
                throw new InvalidDataException("No Server or Category is found to create Channel");
            }

            if (creatorMem == null)
            {
                throw new UnauthorizedAccessException("You are not belong to this server");
            }

            channel.CreatedAt = DateTime.Now;
            channel.Name = request.Name;
            channel.IsPrivate = request.IsPrivate; //overide if category is private
            channel.CreatorId = creatorMem.Id;

            if (request.Type == 0)
            {
                channel.Type = ChannelType.Text;
            }
            else if (request.Type == 1)
            {
                channel.Type = ChannelType.Voice;
            }
            else if (request.Type == 2)
            {
                channel.Type = ChannelType.Stage;
            }
            else if (request.Type == 3)
            {
                channel.Type = ChannelType.Whiteboard;
            }
            else
            {
                channel.Type = ChannelType.Docs;
            }

            var createdChannel = await _unitOfWork.Channels.CreateAsync(channel);

            var serverRoles = await _unitOfWork.Roles.GetRolesByServerIdAsync(createdChannel.ServerId);
            var everyoneRole = serverRoles.FirstOrDefault(r => r.Name.Equals("@everyone"));
            if (everyoneRole == null)
            {
                throw new InvalidOperationException("Role @everyone is not existed in this server");
            }

            // create role Guest for individuals exclude in chosen roles to join event
            if (createdChannel.Type.ToString().Equals("Stage"))
            {
                int rolePosition = everyoneRole.Position;
                everyoneRole.Position = serverRoles.Count() + 1;
                Role guest = new Role
                {
                    Name = $"Guest_[{createdChannel.Id}]",
                    Color = "#FFFFFF",
                    ServerId = createdChannel.ServerId,
                    CreatedAt = DateTime.Now,
                    Position = rolePosition,
                    Mentionable = true,
                };
                var createdGuest = await _unitOfWork.Roles.CreateAsync(guest);
                await _roleService.AssignDefaultEveryonePermissions(createdGuest.Id);
                await _roleService.AssignRolePermissionsChannel(createdChannel.Id, createdGuest.Id);
            }

            // set permission base on status IsPrivate of category

            if(createdChannel.CategoryId != null)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(createdChannel.CategoryId.Value);
                if(category.IsPrivate)
                {
                    createdChannel.IsPrivate = true; // auto set private to channel if added to private category

                    await _roleService.AssignDefaultEveryonePermissionsChannel(createdChannel.Id, everyoneRole.Id);
                    await _unitOfWork.Channels.UpdateAsync(createdChannel);
                }
            }


            await _roleService.AssignDefaultEveryonePermissionsChannel(createdChannel.Id, everyoneRole.Id);

            await ReorderChannelsByType(createdChannel.ServerId);

            await _hubContext.Clients.Group(createdChannel.ServerId.ToString()).CreateChannel(createdChannel.ServerId, ToRealtimeResponse(createdChannel));

            return await GetChannelDetailResponse(createdChannel);
        }


        public async Task DeleteAsync(Guid id, Guid userId)
        {

            var channel = await _unitOfWork.Channels.GetByIdAsync(id);
            if (channel == null)
            {
                throw new InvalidDataException("Channel is not found");
            }

            var server = await _unitOfWork.Servers.GetServerOnlyAsync(channel.ServerId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            if (!userId.Equals(server.OwnerId))
            {
                var hasManageChannelPermission = await HasManageChannelPermissionAsync(channel.ServerId, userId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't permission to DELETE channel in this server");
                    }
                }
            }
            //delete all Guest Role in target Stage Channel
            if (channel.Type.ToString().Equals("Stage"))
            {
                // #rolein channel
                var roles = await GetRolesInChannel(channel.Id);
                // 1 channel co nhiu even -> nhieu role Guest
                var guestRoles = roles.Where(r => r.Name.Contains("Guest_")).ToList();
                foreach (var role in guestRoles)
                {
                    await _unitOfWork.Roles.DeleteAsync(role);
                }
            }

            await _unitOfWork.Channels.DeleteAsync(channel);

            await ReorderChannelsByType(channel.ServerId);
            await _hubContext.Clients.Group(channel.ServerId.ToString()).DeleteChannel(channel.ServerId, id.ToString());

        }


        private async Task<List<Role>> GetRolesInChannel(Guid channelId)
        {
            var channelRoles = await _unitOfWork.ChannelRolePermissions.GetAllRolePermissionsAsync(channelId);

            List<Role> roles = new List<Role>();
            HashSet<Guid> addedRoleIds = new HashSet<Guid>();

            foreach (var channelRole in channelRoles)
            {
                if (addedRoleIds.Add(channelRole.RoleId)) // Adds and returns true if RoleId was not already added
                {
                    var role = await _unitOfWork.Roles.GetByIdAsync(channelRole.RoleId);
                    roles.Add(role);
                }
            }

            return roles;
        }

        public async Task<List<ChannelDetailResponse>> GetAllByServerIdAsync(Guid serverId, QueryChannel query)
        {
            var server = await _unitOfWork.Servers.GetByIdAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            if (query.SearchTerm == null)
            {
                query.SearchTerm = "";
            }


            var queryChannels = await _unitOfWork.Channels.SearchAsync(query.SearchTerm);
            if (queryChannels == null)
            {
                return new List<ChannelDetailResponse>();
            }

            var channels = queryChannels.Where(c => c.ServerId.Equals(serverId)).ToList();

            switch (query.SortBy.ToString())
            {
                case "Name":
                    channels = query.IsDescending ? channels.OrderByDescending(c => c.Name).ToList() : channels.OrderBy(c => c.Name).ToList();
                    break;
                case "CreateAt":
                    channels = query.IsDescending ? channels.OrderByDescending(c => c.CreatedAt).ToList() : channels.OrderBy(c => c.CreatedAt).ToList();
                    break;
                default:
                    channels = channels.OrderBy(s => s.Name).ToList();
                    break;
            }

            var paginatedChannels = channels
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            var rs = new List<ChannelDetailResponse>();
            foreach (var channel in paginatedChannels)
            {
                rs.Add(await GetChannelDetailResponse(channel));
            }
            return rs;
        }


        private async Task ReorderChannelsByType(Guid serverId)
        {
            var server = await _unitOfWork.Servers.GetServerIncludeCateChannelAsync(serverId);
            if (server == null)
            {
                return;
            }

            int position = 1;

            // take all channels not belong to categories
            var uncategorizedChannels = server.Channels.Where(c => c.CategoryId == null).ToList();
            if (uncategorizedChannels.Any())
            {
                var orderedUncategorizedChannels = uncategorizedChannels
                    .OrderBy(c => c.Type)
                    .ThenBy(c => c.Position)
                    .ToList();

                foreach (var channel in orderedUncategorizedChannels)
                {
                    channel.Position = position++;
                    await _unitOfWork.Channels.UpdateAsync(channel);
                }
            }

            foreach (var category in server.Categories)
            {
                if (category.Channels != null && category.Channels.Any())
                {
                    var channelsInCategory = category.Channels
                        .OrderBy(c => c.Type)
                        .ThenBy(c => c.Position)
                        .ToList();

                    position = 1;

                    foreach (var channel in channelsInCategory)
                    {
                        channel.Position = position++;
                        await _unitOfWork.Channels.UpdateAsync(channel);
                    }
                }
            }
        }
        private async Task<ChannelDetailResponse> GetChannelDetailResponse(Channel channel)
        {

            var server = await _unitOfWork.Servers.GetServerIncludeRolesMemberAsync(channel.ServerId);
            var serverRoleList = await GetRolesInChannel(channel.Id);
            var channelRoles = await _unitOfWork.ChannelRolePermissions.GetAllRolePermissionsAsync(channel.Id);
            List<ChannelRoleWithPermissionResponse> channelPermissionList = new List<ChannelRoleWithPermissionResponse>();
            foreach (var role in serverRoleList)
            {
                var rolePermission = channelRoles.Where(cr => cr.RoleId.Equals(role.Id)).ToList();
                if (rolePermission != null)
                {
                    List<ChannelPermissionResponse> channelPermissions = new List<ChannelPermissionResponse>();

                    foreach (var permission in rolePermission)
                    {
                        if (permission.IsGranted)
                            channelPermissions.Add(new ChannelPermissionResponse
                            {
                                PermissionId = permission.PermissionId,
                                Code = permission.Permission.Code,
                                Description = permission.Permission.Description,
                                Name = permission.Permission.Name,
                            });
                    }
                    channelPermissionList.Add(new ChannelRoleWithPermissionResponse
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        Permission = channelPermissions
                    });
                }
            }

            var events = await _unitOfWork.Events.GetAllInChannelAsync(channel.Id);
            if (events == null)
            {
                events = new List<Event>();
            }
            List<EventCreateResponse> eventList = new List<EventCreateResponse>();

            foreach (var e in events)
            {
                EventCreateResponse eventRs = new EventCreateResponse
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

                eventList.Add(eventRs);
            };


            var response = new ChannelDetailResponse
            {
                Id = channel.Id,
                Name = channel.Name,
                Type = channel.Type,
                ServerId = channel.ServerId,
                CategoryId = channel.CategoryId,
                ChannelRoles = channelPermissionList,
                CreatedAt = channel.CreatedAt,
                IsPrivate = channel.IsPrivate,
                Position = channel.Position,
                UpdatedAt = channel.UpdatedAt,
                Events = eventList
            };

            return response;
        }


        public async Task<ChannelDetailResponse> GetByIdAsync(Guid id)
        {
            var channel = await _unitOfWork.Channels.GetByIdAsync(id);
            if (channel == null)
            {
                throw new InvalidDataException("Channel is not found");
            }

            return await GetChannelDetailResponse(channel);
        }

        private ChannelCreateResponse ChannelConvertToResponse(Channel channel)
        {
            ChannelCreateResponse response = new ChannelCreateResponse
            {
                Id = channel.Id,
                Name = channel.Name,
                Type = channel.Type,
                Position = channel.Position,
                ServerId = channel.ServerId,
                IsPrivate = channel.IsPrivate,
                CreatedAt = channel.CreatedAt,
                UpdatedAt = channel.UpdatedAt,
                ChannelRolePermissions = channel.ChannelRolePermissions,
                CreatorId = channel.CreatorId
            };
            if (channel.CategoryId != null)
            {
                response.CategoryId = channel.CategoryId.Value;
            }
            return response;
        }


        public async Task<List<ChannelDetailResponse>> SearchAsync(Guid serverId, QueryChannel query)
        {
            if (query.SearchTerm == null)
            {
                query.SearchTerm = "";
            }

            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            var channelList = await _unitOfWork.Channels.SearchAsync(query.SearchTerm);
            if (channelList == null)
            {
                return new List<ChannelDetailResponse>();
            }

            var channels = channelList.Where(c => c.ServerId.Equals(serverId)).ToList();

            switch (query.SortBy.ToString())
            {
                case "Name":
                    channels = query.IsDescending ? channels.OrderByDescending(c => c.Name).ToList() : channels.OrderBy(c => c.Name).ToList();
                    break;
                case "CreateAt":
                    channels = query.IsDescending ? channels.OrderByDescending(c => c.CreatedAt).ToList() : channels.OrderBy(c => c.CreatedAt).ToList();
                    break;
                default:
                    channels = channels.OrderBy(s => s.Name).ToList();
                    break;
            }

            var paginatedChannels = channels
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            var rs = new List<ChannelDetailResponse>();
            foreach (var channel in paginatedChannels)
            {
                rs.Add(await GetChannelDetailResponse(channel));
            }
            return rs;
        }

        public async Task<ChannelDetailResponse> UpdateAsync(Guid id, Guid userId, ChannelUpdateRequest request)
        {
            var channel = await _unitOfWork.Channels.GetByIdAsync(id);
            if (channel == null)
            {
                throw new InvalidDataException("Channel is not found");
            }
            var server = await _unitOfWork.Servers.GetServerIncludeMembersAsync(channel.ServerId);

            ServerMember creator = null;
            foreach (var m in server.ServerMembers)
            {
                if (m.UserId.Equals(userId))
                {
                    creator = m;
                }
            }
            if (creator == null)
            {
                throw new UnauthorizedAccessException("You are not belong to this server");
            }
            if (!userId.Equals(server.OwnerId) || !channel.CreatorId.Equals(creator.Id))
            {
                var hasManageChannelPermission = await HasManageChannelPermissionAsync(channel.ServerId, userId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't permission to UPDATE channel in this server");
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                channel.Name = request.Name;
            }

            await SetChannelPrivacyAsync(id, request.IsPrivate);

            channel.UpdatedAt = DateTime.UtcNow;
            var updatedChannel = await _unitOfWork.Channels.UpdateAsync(channel);
            await _hubContext.Clients.Group(channel.ServerId.ToString()).UpdateChannel(channel.ServerId, ToRealtimeResponse(updatedChannel));

            return await GetChannelDetailResponse(updatedChannel);
        }

     
        public async Task SetChannelPrivacyAsync(Guid channelId, bool isPrivate)
        {
            var channel = await _unitOfWork.Channels.GetByIdAsync(channelId);

            // case from public to private channel 
            // #todo update role permissions to redis, call signalR for all roles that had been changed permission view_channel from true to false
            var rolesInChannel = await GetRolesInChannel(channelId);
            var everyoneRole = rolesInChannel.FirstOrDefault(r => r.Name.Equals("@everyone"));
            if (!channel.IsPrivate == isPrivate && everyoneRole != null)
            {
                // * channel.IsPrivate = false -> true
                if (isPrivate)
                {
                    channel.IsPrivate = true;

                    var viewChannelPerms = channel.ChannelRolePermissions
                    .FirstOrDefault(crp => crp.Permission.Code == "VIEW_CHANNEL" && crp.IsGranted && crp.RoleId.Equals(everyoneRole.Id));
                    
                    if(viewChannelPerms != null)
                    {
                        viewChannelPerms.IsGranted = false;
                    }

                    channel.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    channel.IsPrivate = false;

                    var permission = await _unitOfWork.Permissions.GetByPermissionCode("VIEW_CHANNEL");

                    var viewChannelPerm = await _unitOfWork.ChannelRolePermissions.GetByChannelRolePermissionAsync(channelId, everyoneRole.Id, permission.Id);

                    viewChannelPerm.IsGranted = true;

                    channel.UpdatedAt = DateTime.UtcNow;
                }

                    var members = await _unitOfWork.ServerMembers.GetAllByRoleIdAsync(everyoneRole.Id);
                    if (members != null)
                    {
                        foreach (var member in members)
                        {
                            await SetUserChannelPermission(member.UserId, channel.Id);
                        }
                    }

                    await _hubContext.Clients.Group(everyoneRole.Id.ToString()).ChannelPermissionUpdated(channelId, isPrivate, everyoneRole.Id);


            }
        }
        private async Task SetUserGlobalPermission(Guid userId, Guid serverId)
        {
            ServerMember serverMember = await _unitOfWork.ServerMembers.GetByUserIdAndServerIdIncludeRolesPermissionsAsync(new ServerMemberCreateRequest
            {
                ServerId = serverId,
                UserId = userId
            });

            if (serverMember == null)
            {
                throw new InvalidOperationException("This user is not in the server.");
            }

            var permissionIds = new HashSet<Guid>();
            var permissions = new List<RolePermissionResponse>();

            foreach (var memberRole in serverMember.MemberRoles)
            {
                foreach (var rolePermission in memberRole.Role.RolePermissions)
                {
                    if (rolePermission.IsGranted && rolePermission.Permission.IsServer && permissionIds.Add(rolePermission.Permission.Id))
                    {
                        permissions.Add(new RolePermissionResponse
                        {
                            Code = rolePermission.Permission.Code
                        });
                    }
                }
            }

            var redisKey = $"userPermission:{userId}:{serverId}:global";
            await _redisDatabase.StringSetAsync(redisKey, JsonConvert.SerializeObject(permissions));
        }

        private async Task SetUserChannelPermission(Guid userId, Guid channelId)
        {
            var channel = await _unitOfWork.Channels.GetSimpleChannelAsync(channelId);
            if (channel == null) throw new InvalidDataException("Channel not found.");


            ServerMember serverMember = await _unitOfWork.ServerMembers.GetByUserIdAndServerIdIncludeRolesPermissionsAsync(new ServerMemberCreateRequest
            {
                ServerId = channel.ServerId,
                UserId = userId
            });

            if (serverMember == null)
            {
                throw new InvalidOperationException("This user is not in the server.");
            }

            var rolesInChannel = await GetRolesInChannel(channelId);

            var userRolesInChannel = serverMember.MemberRoles
                .Select(mr => mr.Role)
                .Where(role => rolesInChannel.Any(r => r.Id.Equals(role.Id)))
                .ToList();

            if (!userRolesInChannel.Any()) return;

            var highestPriorityRole = userRolesInChannel.OrderBy(r => r.Position).FirstOrDefault();
            if (highestPriorityRole == null) return;

            var permissions = new List<RolePermissionResponse>();
            var seenCodes = new HashSet<string>();

            foreach (var channelRolePermission in highestPriorityRole.ChannelRolePermissions.Where(p => p.ChannelId == channelId))
            {
                if (!channelRolePermission.Permission.IsServer &&
                    channelRolePermission.IsGranted &&
                    seenCodes.Add(channelRolePermission.Permission.Code))
                {
                    permissions.Add(new RolePermissionResponse
                    {
                        Code = channelRolePermission.Permission.Code
                    });
                }
            }

            var redisKey = $"userPermission:{userId}:{channel.ServerId}:channel:{channelId}";
            await _redisDatabase.StringSetAsync(redisKey, JsonConvert.SerializeObject(permissions));
        }

        public async Task<string> ChangePositionAsync(Guid channelId, int newPosition, Guid userId)
        {
            if (newPosition < 0)
            {
                throw new InvalidDataException("Invalid input position");
            }
            if (newPosition == 0)
            {
                newPosition = 1;
            }
            var channel = await _unitOfWork.Channels.GetByIdAsync(channelId);
            if (channel == null)
            {
                throw new InvalidDataException("Channel is not found");
            }
            if (channel.Position == newPosition)
            {
                throw new InvalidOperationException("New position is the same as old position");
            }
            var server = await _unitOfWork.Servers.GetByIdAsync(channel.ServerId);
            if (!userId.Equals(server.OwnerId))
            {
                var hasManageChannelPermission = await HasManageChannelPermissionAsync(server.Id, userId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't permission to CHANGE channel is position in this server");
                    }
                }
            }
            // list channel in server ( not belong in any category)
            if (channel.CategoryId == null)
            {
                var channels = server.Channels.Where(c => c.CategoryId == null).ToList();
                if (newPosition > channels.Count)
                {
                    newPosition = channels.Count;
                }
                switch (channel.Type.ToString())
                {
                    case "Text":
                        var textChannels = server.Channels.Where(c => c.Type.ToString() == "Text").OrderBy(c => c.Position).ToList();
                        await ShiftingChannelPosition(textChannels, newPosition, channel, "Text");
                        break;
                    case "Voice":
                        var voiceChannels = server.Channels.Where(c => c.Type.ToString() == "Voice").OrderBy(c => c.Position).ToList();
                        await ShiftingChannelPosition(voiceChannels, newPosition, channel, "Voice");
                        break;

                    case "Stage":
                        var stageChannels = server.Channels.Where(c => c.Type.ToString() == "Stage").OrderBy(c => c.Position).ToList();
                        await ShiftingChannelPosition(stageChannels, newPosition, channel, "Stage");
                        break;
                }
                await _unitOfWork.Servers.UpdateAsync(server);
            }
            else
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(channel.CategoryId.Value);
                if (newPosition > category.Channels.Count)
                {
                    newPosition = category.Channels.Count;
                }
                switch (channel.Type.ToString())
                {
                    case "Text":
                        var textChannels = category.Channels.Where(c => c.Type.ToString() == "Text").OrderBy(c => c.Position).ToList();
                        await ShiftingChannelPosition(textChannels, newPosition, channel, "Text");
                        break;
                    case "Voice":
                        var voiceChannels = category.Channels.Where(c => c.Type.ToString() == "Voice").OrderBy(c => c.Position).ToList();
                        await ShiftingChannelPosition(voiceChannels, newPosition, channel, "Voice");
                        break;

                    case "Stage":
                        var stageChannels = category.Channels.Where(c => c.Type.ToString() == "Stage").OrderBy(c => c.Position).ToList();
                        await ShiftingChannelPosition(stageChannels, newPosition, channel, "Stage");
                        break;
                }


            }

            await _hubContext.Clients.Group(channel.ServerId.ToString()).ChangeChannelPosition(channel.ServerId, channelId, newPosition);
            return "Channel positions in this category have been updated successfully.";

        }

        private async Task ShiftingChannelPosition(List<Channel> channels, int newPosition, Channel targetChannel, string channelType)
        {

            if (targetChannel.Position > newPosition)
            {
                if (newPosition <= channels[0].Position)
                {
                    newPosition = channels[0].Position;
                }
                var channelsToShift = channels
                    .Where(c => c.Position >= newPosition && c.Position < targetChannel.Position)
                    .ToList();

                foreach (var c in channelsToShift)
                {
                    c.Position++;
                    await _unitOfWork.Channels.UpdateAsync(c);
                }

            }
            else if (targetChannel.Position < newPosition)
            {
                if (newPosition >= channels[channels.Count() - 1].Position)
                {
                    newPosition = channels[channels.Count() - 1].Position;
                }
                var channelsToShift = channels
                    .Where(c => c.Position > targetChannel.Position && c.Position <= newPosition)
                    .ToList();

                foreach (var c in channelsToShift)
                {
                    c.Position--;
                    await _unitOfWork.Channels.UpdateAsync(c);
                }
            }
            targetChannel.Position = newPosition;
            await _unitOfWork.Channels.UpdateAsync(targetChannel);
        }

        public Task<List<Channel>> GetAllByRoleIdAsync(Guid roleId)
        {
            return _unitOfWork.Channels.GetAllByRoleIdAsync(roleId);
        }
    }
}
