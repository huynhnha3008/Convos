using BusinessObjects.DTOs;
using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;
using Services.SignalR.Interfaces;
using Services.SignalR;
using System.Data;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using BusinessObjects.QueryObject;
using BusinessObjects.Models;
using BusinessObjects.DTOs.ServerDto;
using Newtonsoft.Json;
using BusinessObjects.DTOs.ServersDTO;
using BusinessObjects.DTOs.EventDto;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Services
{
    public class ServerService : IServerService
    {
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<ServerHub, IServerHub> _hubContext;
        private readonly IPermissionService _permissionService;
        private readonly FirebaseService _firebaseService;
        private readonly IRoleService _roleService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly StackExchange.Redis.IDatabase _redisDatabase;

        public ServerService(IRoleService roleService, ICategoryService categoryService, IUnitOfWork unitOfWork,
                              IHubContext<ServerHub, IServerHub> hubContext, IPermissionService permissionService,
                              FirebaseService firebaseService, StackExchange.Redis.IConnectionMultiplexer redis)
        {
            _categoryService = categoryService;
            _roleService = roleService;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _permissionService = permissionService;
            _firebaseService = firebaseService;
            _redisDatabase = redis.GetDatabase();
        }

        public async Task<string> CreateAsync(ServerCreateRequest serverRequest, IFormFile? IconFile, Guid currentUserId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (user == null)
            {
                throw new InvalidDataException("User is not found");
            }

            string iconUrl = "";
            if (IconFile != null)
            {
                using (var stream = IconFile.OpenReadStream())
                {
                    iconUrl = await _firebaseService.UploadAvatarAsync(stream, IconFile.FileName);
                }
            }

            Server createdServer = new Server();
            switch (serverRequest.Type)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    createdServer = new Server
                    {
                        OwnerId = currentUserId,
                        CreatedAt = DateTime.Now,
                        Icon = iconUrl,
                        Name = serverRequest.Name,
                        UpdatedAt = DateTime.Now
                    };

                    createdServer = await _unitOfWork.Servers.CreateAsync(createdServer);
                    await AddDefaultCategories(createdServer.Id, serverRequest.Type);

                    if (serverRequest.Type == 4)
                    {
                        var infoCategoryRequest = new CategoryCreateRequest
                        {
                            Name = "Information",
                            ServerId = createdServer.Id,
                            IsPrivate = false
                        };

                        var infoCategory = await _categoryService.CreateAsync(infoCategoryRequest, Guid.NewGuid(), serverRequest.Type);
                        //createdServer.Categories.Add(infoCategory);
                    }
                    break;
            }

            // Add owner
            var owner = new ServerMember
            {
                User = user,
                Banned = false,
                Deafened = false,
                JoinedAt = DateTime.Now,
                Muted = false,
                Nickname = user.DisplayName,
                UserId = user.Id,
                ServerId = createdServer.Id
            };

            var firstMember = await _unitOfWork.ServerMembers.CreateAsync(owner);
            createdServer.ServerMembers.Add(firstMember);

            // Add everyone role
            var everyoneRole = new Role
            {
                Name = "@everyone",
                Color = "#FFFFFF",
                ServerId = createdServer.Id,
                CreatedAt = DateTime.Now,
                Position = 1,
                Mentionable = true,
            };

            var createdEveryoneRole = await _unitOfWork.Roles.CreateAsync(everyoneRole);

            // assign role @everyone for owner
            MemberRole memberRole = new MemberRole
            {
                RoleId = createdEveryoneRole.Id,
                ServerMemberId = firstMember.Id,
            };
            await _unitOfWork.MemberRoles.CreateAsync(memberRole);
            // Assign permissions
            await _roleService.AssignDefaultEveryonePermissions(createdEveryoneRole.Id);
            foreach (var channel in createdServer.Channels)
            {
                await _roleService.AssignDefaultEveryonePermissionsChannel(channel.Id, createdEveryoneRole.Id);
                channel.CreatorId = firstMember.Id;
            }

            foreach (var cate in createdServer.Categories)
            {
                cate.CreatorId = firstMember.Id;
            }
            await _unitOfWork.Servers.UpdateAsync(createdServer);

            return $"Server {createdServer.Name} has been created successfully";
        }

        private async Task AddDefaultCategories(Guid serverId, int serverType)
        {
            var textCategoryRequest = new CategoryCreateRequest
            {
                Name = "Text Channels",
                ServerId = serverId,
                IsPrivate = false
            };

            await _categoryService.CreateAsync(textCategoryRequest, Guid.NewGuid(), serverType);
            //createdServer.Categories.Add(textCategory);

            var voiceCategoryRequest = new CategoryCreateRequest
            {
                Name = "Voice Channels",
                ServerId = serverId,
                IsPrivate = false
            };

            await _categoryService.CreateAsync(voiceCategoryRequest, Guid.NewGuid(), serverType);
            //createdServer.Categories.Add(voiceCategory);
        }

        public async Task<List<ServerGetAllResponse>> GetAllAsync(QueryServer query)
        {
            if (query.SearchTerm == null)
            {
                query.SearchTerm = "";
            }
            var result = await _unitOfWork.Servers.SearchAsync(query.SearchTerm);
            if (result == null || !result.Any())
            {
                return new List<ServerGetAllResponse>();
            }

            switch (query.SortBy.ToString())
            {
                case "Name":
                    result = query.IsDescending ? result.OrderByDescending(s => s.Name) : result.OrderBy(s => s.Name);
                    break;
                case "CreateAt":
                    result = query.IsDescending ? result.OrderByDescending(s => s.CreatedAt) : result.OrderBy(s => s.CreatedAt);
                    break;
                default:
                    result = result.OrderBy(s => s.Name);
                    break;
            }


            var paginatedServers = result
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            List<ServerGetAllResponse> serverList = new List<ServerGetAllResponse>();
            foreach (var s in paginatedServers)
            {
                ServerGetAllResponse rs = new ServerGetAllResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Icon = s.Icon,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    TotalMembers = s.ServerMembers.Count(),
                    TotalChannels = s.Channels.Count()
                };
                serverList.Add(rs);
            }

            return serverList;

        }
        public async Task<List<ServerBackupResponse>> GetAllServersByUserIdAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw new InvalidDataException("User is not found");

            var serverIds = await _unitOfWork.ServerMembers.GetAllServerIdByUserIdAsync(userId);
            if (serverIds == null || !serverIds.Any())
                throw new InvalidDataException("User does not belong to any server");

            var serverDetails = new List<ServerBackupResponse>();

            foreach (var serverId in serverIds)
            {
        
                var member = await _unitOfWork.ServerMembers
                    .GetByUserIdAndServerIdIncludeRolesPermissionsAsync(new ServerMemberCreateRequest
                    {
                        ServerId = serverId,
                        UserId = userId
                    });

                if (member == null || member.Banned) continue;

                var detail = await GetServerDetailByIdAsync(serverId);
                serverDetails.Add(detail);

                var server = await _unitOfWork.Servers.GetServerIncludeCateChannelAsync(serverId);
                var allChannels = server.Channels;

        
                foreach (var channel in allChannels)
                {
                    var allowedRoleIds = (await GetRolesInChannel(channel.Id))
                        .Select(r => r.Id)
                        .ToHashSet();

                    var userRolesInChannel = member.MemberRoles
                        .Select(mr => mr.Role)
                        .Where(r => allowedRoleIds.Contains(r.Id))
                        .ToList();

                    if (!userRolesInChannel.Any()) continue;

                    var highestPriorityRole = userRolesInChannel.OrderBy(r => r.Position).FirstOrDefault();
                    if (highestPriorityRole == null) continue;

                    var permissions = highestPriorityRole.ChannelRolePermissions
                        .Where(p =>
                            p.ChannelId == channel.Id &&
                            p.IsGranted &&
                            !p.Permission.IsServer)
                        .Select(p => p.Permission.Code)
                        .Distinct()
                        .Select(code => new RolePermissionResponse { Code = code })
                        .ToList();

                    var redisKey = $"userPermission:{userId}:{serverId}:channel:{channel.Id}";
                    await _redisDatabase.StringSetAsync(redisKey, JsonConvert.SerializeObject(permissions));
                }

                // Set global permission
                await SetUserGlobalPermission(userId, serverId);
            }

    
            foreach (var server in serverDetails)
            {
                var member = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(server.Id, userId);
                if (member == null) continue;
                if (userId == server.OwnerId) continue;

                var allChannelIds = await GetAllChannelIdsByServerId(server.Id);
                var channelsToRemove = new HashSet<Guid>();

                foreach (var channelId in allChannelIds)
                {
                    if (!await HasViewChannelPermissionAsync(server.Id, userId, channelId))
                    {
                        channelsToRemove.Add(channelId);
                    }
                }

                server.Channels = server.Channels
                    .Where(c => !channelsToRemove.Contains(c.Id))
                    .ToList();

                server.Categories = server.Categories
                    .Where(cate =>
                    {
                        if (cate.IsPrivate && !cate.CreatorId.Equals(member.Id))
                        {
                            cate.Channels = cate.Channels
                                .Where(c => !channelsToRemove.Contains(c.Id))
                                .ToList();

                            return cate.Channels.Any();
                        }

                        cate.Channels = cate.Channels
                            .Where(c => !channelsToRemove.Contains(c.Id))
                            .ToList();

                        return true;
                    })
                    .ToList();
            }

            return serverDetails;
        }


        private async Task<bool> HasViewChannelPermissionAsync(Guid serverId, Guid userId, Guid channelId)
        {
            var channelPermission = await _permissionService.GetUserChannelPermission(userId, serverId, channelId);
            if (channelPermission == null)
            {
                return false;
            }

            if (channelPermission == null)
            {
                return false;
            }

            // Check exist VIEW_CHANNEL ?
            var hasViewChannelPermission = channelPermission
                .Any(p => p.Code.Equals("VIEW_CHANNEL", StringComparison.OrdinalIgnoreCase));


            var member = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(serverId, userId);
            if (member == null)
            {
                return false;
            }
            var channel = await _unitOfWork.Channels.GetSimpleChannelAsync(channelId);
            if (channel == null)
            {
                throw new InvalidDataException("Channel is not found");
            }

            // check creator
            if (member.Id.Equals(channel.CreatorId))
            {
                return true;
            }
            return hasViewChannelPermission;
        }





        public async Task<Server> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Servers.GetByIdAsync(id);
        }

        private async Task<bool> HasManageServerPermissionAsync(Guid id, Guid userId)
        {
            var userPermission = await _permissionService.GetUserGlobalPermission(userId, id);
            if (userPermission == null)
            {
                return false;
            }
            var hasManageServerPermission = userPermission
                .Any(p => p.Code.Equals(PermissionEnum.MANAGE_SERVER.ToString()));

            if (!hasManageServerPermission)
            {
                return false;
            }
            return true;
        }
        public async Task<string> UpdateAsync(Guid id, Guid userId, ServerUpdateRequest request, IFormFile? IconFile)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(id);
            if (server == null)
            {
                throw new InvalidOperationException("ServerId does not match any server");
            }
            var member = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(id, userId);
            if (member == null)
            {
                throw new InvalidDataException("Member is not found in this server");
            }
            if (member.Banned)
            {
                throw new InvalidOperationException("You are BANNED from this Server.");
            }
            if (!userId.Equals(server.OwnerId))
            {
                var isManageServerGranted = await HasManageServerPermissionAsync(id, userId);
                if (!isManageServerGranted)
                {
                    throw new UnauthorizedAccessException("You do not have permission to UPDATE this server.");
                }
            }
            string iconUrl = server.Icon;
            if (IconFile != null)
            {
                using (var stream = IconFile.OpenReadStream())
                {
                    iconUrl = await _firebaseService.UploadAvatarAsync(stream, IconFile.FileName);
                }
            }

            server.Name = request.Name;
            if (iconUrl != server.Icon)
            {
                server.Icon = iconUrl;
            }
            server.UpdatedAt = DateTime.UtcNow;
            var updatedServer = await _unitOfWork.Servers.UpdateAsync(server);

            await _hubContext.Clients.Groups(server.Id.ToString())
                                     .UpdateServer(server.Id, updatedServer.Name, updatedServer.Icon);

            return "Updated successfully";
        }


        public async Task<string> DeleteAsync(Guid id, Guid userId)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(id);
            if (server == null)
            {
                throw new InvalidDataException("Server not found");
            }

            var member = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(id, userId);
            if (member == null)
            {
                throw new InvalidDataException("Member is not found in this server");
            }
            if (member.Banned)
            {
                throw new InvalidOperationException("You are BANNED from this Server.");
            }

            if (!userId.Equals(server.OwnerId))
            {
                var isManageServerGranted = await HasManageServerPermissionAsync(id, userId);
                if (!isManageServerGranted)
                {
                    throw new UnauthorizedAccessException("Permission denied: You do not have permission to DELETE this server.");
                }
            }


            await _unitOfWork.Servers.DeleteAsync(server);

            await _hubContext.Clients.Group(server.Id.ToString()).DeleteServer(server.Id, server.Name);

            return $"Server {server.Name} has been deleted successfully";
        }


        public async Task<List<ServerBackupResponse>> SearchAsync(QueryServer query)
        {
            if (query.SearchTerm == null)
            {
                query.SearchTerm = "";
            }
            var result = await _unitOfWork.Servers.SearchAsync(query.SearchTerm);
            if (result == null || !result.Any())
            {
                throw new InvalidOperationException("No servers found");
            }

            switch (query.SortBy.ToString())
            {
                case "Name":
                    result = query.IsDescending ? result.OrderByDescending(s => s.Name) : result.OrderBy(s => s.Name);
                    break;
                case "CreateAt":
                    result = query.IsDescending ? result.OrderByDescending(s => s.CreatedAt) : result.OrderBy(s => s.CreatedAt);
                    break;
                default:
                    result = result.OrderBy(s => s.Name);
                    break;
            }


            List<ServerBackupResponse> list = new List<ServerBackupResponse>();
            foreach (var item in result)
            {
                list.Add(await GetServerDetailByIdAsync(item.Id));
            }

            var paginatedServers = list
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();
            return paginatedServers;
        }

        private ServerCreateResponse ConvertToDto(Server server)
        {
            return new ServerCreateResponse
            {
                OwnerId = server.OwnerId,
                Name = server.Name,
                Icon = server.Icon,
                CreatedAt = server.CreatedAt,
                UpdatedAt = server.UpdatedAt,
                MembersCount = server.ServerMembers?.Count ?? 0
            };
        }


        public async Task<MemberServerDetailResponse> GetMemberServerDetail(Guid serverId, Guid userId)
        {
            var server = await _unitOfWork.Servers.GetServerIncludeMembersChannelsAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            var serverRoles = await _unitOfWork.Roles.GetRolesByServerIdAsync(serverId);

            var member = server.ServerMembers.FirstOrDefault(sm => sm.UserId.Equals(userId));
            if (member == null)
                throw new InvalidDataException("You do not belong to this server");

            // neva null (at least member has 1 role @everyone
            var memberRoleList = await _unitOfWork.MemberRoles.GetAllByMemberId(member.Id);

            //list of role of member
            var memberRoles = serverRoles.Where(role => memberRoleList.Any(mr => mr.RoleId.Equals(role.Id))).ToList();

            var memberHighestRole = memberRoles.Min(r => r.Position);

            var grantedPermissions = new HashSet<Permission>();
            var userChannelPermissions = new List<ChannelPermission>();

            foreach (var memRole in memberRoleList)
            {
                var rolePermissions = await _unitOfWork.RolePermissions.GetAllByRoleId(memRole.RoleId);
                foreach (var permission in rolePermissions)
                {
                    if (permission.IsGranted)
                        grantedPermissions.Add(permission.Permission);
                }
            }

            // map object to User Server is permission
            var userServerPermissions = grantedPermissions
                .Select(p => new RolePermissionResponse { Code = p.Code })
                .ToList();

            // list all channelId in server
            List<Guid> channelIdList = new List<Guid>();
            foreach (var channel in server.Channels)
            {
                channelIdList.Add(channel.Id);
            }


            //channel Role permission
            List<RoleInChannelPermissions> roleChannelPermission = new List<RoleInChannelPermissions>();

            foreach (var channel in channelIdList)
            {
                List<RolePermissionResponse> roles = new List<RolePermissionResponse>();
                var rolesInChannel = await GetRolesInChannel(channel);
                if (rolesInChannel != null)
                {
                    var joinRoles = memberRoles.Where(role => rolesInChannel.Any(r => r.Id.Equals(role.Id))).ToList();
                    var lowestPosition = joinRoles.Min(r => r.Position);
                    var highestRole = joinRoles.FirstOrDefault(r => r.Position == lowestPosition);

                    foreach (var channelRole in highestRole.ChannelRolePermissions)
                    {
                        if (!channelRole.Permission.IsServer && channelRole.IsGranted)
                        {
                            roles.Add(new RolePermissionResponse
                            {
                                Code = channelRole.Permission.Code,
                            });
                        }
                    }
                    roleChannelPermission.Add(new RoleInChannelPermissions
                    {
                        channelId = channel,
                        permissions = roles
                    });
                }
            }

            //set user permission to redis
            await SetUserPermission(userId, serverId);
            return new MemberServerDetailResponse
            {
                server = await GetServerDetailByIdAsync(serverId),
                userServerPermissions = userServerPermissions,
                userChannelPermissions = roleChannelPermission
            };
        }

        private async Task<List<Role>> GetRolesInChannel(Guid channelId)
        {
            var channelRoles = await _unitOfWork.ChannelRolePermissions.GetAllRolePermissionsAsync(channelId);

            List<Role> roles = new List<Role>();
            HashSet<Guid> addedRoleIds = new HashSet<Guid>();

            foreach (var channelRole in channelRoles)
            {
                if (addedRoleIds.Add(channelRole.RoleId)) // Adds and returns true if RoleId was not already added do 
                {
                    var role = await _unitOfWork.Roles.GetByIdAsync(channelRole.RoleId);
                    roles.Add(role);
                }
            }

            return roles;
        }


        public async Task<ServerBackupResponse> GetServerDetailByIdAsync(Guid id)
        {
            var server = await _unitOfWork.Servers.GetByIdAsync(id);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            ServerBackupResponse serverDetail = new ServerBackupResponse();
            List<RoleServerDetailReponse> roleList = new List<RoleServerDetailReponse>();
            foreach (var role in server.Roles)
            {
                roleList.Add(await ToRoleDetailResponse(role));
            }
            List<EmojiServerDetailResponse> emojis = new List<EmojiServerDetailResponse>();
            foreach (var emoji in server.Emojis)
            {
                emojis.Add(new EmojiServerDetailResponse
                {
                    Id = emoji.Id,
                    Image = emoji.Image,
                    Name = emoji.Name,
                    ServerMemberId = emoji.ServerMemberId,
                });
            }
            List<ChannelServerDetailResponse> channelServerList = new List<ChannelServerDetailResponse>();

            var sortedChannels = server.Channels
                           .Where(c => c.CategoryId == null)
                           .OrderBy(c => c.Type)
                           .ThenBy(c => c.Position)
                           .ToList();
            foreach (var channel in sortedChannels)
            {
                if (channel.CategoryId == null)
                {
                    channelServerList.Add(ToChannelServerResponse(channel));
                }
            }

            List<ServerMemberDTO> serverMembers = new List<ServerMemberDTO>();
            foreach (var sm in server.ServerMembers)
            {
                ServerMemberDTO tmp = new ServerMemberDTO
                {
                    MemberId = sm.Id,
                    Avatar = sm.User.Avatar,
                    UserName = sm.User.Username,
                    Nickname = sm.Nickname,
                    Status = sm.User.Status.ToString()
                };
                serverMembers.Add(tmp);
            }

            List<CategoryCreateResponse> categoryList = new List<CategoryCreateResponse>();
            var sortedCategories = server.Categories
                                        .Select(category =>
                                        {
                                            category.Channels = category.Channels
                                                                        .OrderBy(ch => ch.Type)
                                                                        .ThenBy(ch => ch.Position)
                                                                        .ToList();
                                            return category;
                                        })
                                        .ToList();

            foreach (var category in sortedCategories)
            {
                List<ChannelServerDetailResponse> channelCategoryList = new List<ChannelServerDetailResponse>();

                foreach (var channel in category.Channels)
                {
                    channelCategoryList.Add(ToChannelServerResponse(channel));
                }
                CategoryCreateResponse categoryCreateResponse = new CategoryCreateResponse
                {
                    Id = category.Id,
                    Channels = channelCategoryList,
                    CreatedAt = category.CreatedAt,
                    Name = category.Name,
                    Position = category.Position,
                    UpdatedAt = category.UpdatedAt,
                    IsPrivate = category.IsPrivate,
                    CreatorId = category.CreatorId
                };
                categoryList.Add(categoryCreateResponse);
            }

            var events = await _unitOfWork.Events.GetAllInServerAsync(id);
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


            //soundboard
            var soundboards = await _unitOfWork.SoundBoards.GetAllServerSoundAsync(server.Id);
            List<SoundBoardCreateResponse> soundBoardList = new List<SoundBoardCreateResponse>();
            foreach (var soundboard in soundboards)
            {
                soundBoardList.Add(ToSoundBoardResponse(soundboard));
            }


            serverDetail.Id = server.Id;
            serverDetail.OwnerId = server.OwnerId;
            serverDetail.Name = server.Name;
            serverDetail.Icon = server.Icon;
            serverDetail.CreatedAt = server.CreatedAt;
            serverDetail.UpdatedAt = server.UpdatedAt;
            serverDetail.Roles = roleList;
            serverDetail.Categories = categoryList;
            serverDetail.Channels = channelServerList;
            serverDetail.Emojis = emojis;
            serverDetail.SoundBoards = soundBoardList;
            serverDetail.Events = eventList;
            serverDetail.ServerMembers = serverMembers;

            return serverDetail;
        }

        private SoundBoardCreateResponse ToSoundBoardResponse(SoundBoard sound)
        {
            ServerCreateResponse serverRes = new ServerCreateResponse
            {
                CreatedAt = sound.Server.CreatedAt,
                Icon = sound.Server.Icon,
                Name = sound.Server.Name,
                OwnerId = sound.Server.OwnerId,
                UpdatedAt = sound.Server.UpdatedAt,
                MembersCount = sound.Server.ServerMembers.Count,
            };
            SoundBoardCreateResponse soundb = new SoundBoardCreateResponse
            {
                Id = sound.Id,
                Emoji = sound.Emoji,
                Name = sound.Name,
                server = serverRes,
                ServerId = sound.ServerId,
                Sound = sound.Sound,
                ServerMemberId = sound.ServerMemberId
            };
            return soundb;
        }

        private ChannelServerDetailResponse ToChannelServerResponse(Channel channel)
        {
            ChannelServerDetailResponse channelRes = new ChannelServerDetailResponse
            {
                Id = channel.Id,
                CategoryId = channel.CategoryId,
                CreatedAt = channel.CreatedAt,
                IsPrivate = channel.IsPrivate,
                Name = channel.Name,
                Position = channel.Position,
                Type = channel.Type,
                UpdatedAt = channel.UpdatedAt
            };
            return channelRes;
        }
        private async Task<RoleServerDetailReponse> ToRoleDetailResponse(Role role)
        {
            var server = await _unitOfWork.Servers.GetByIdAsync(role.ServerId);
            //    var ownerMem =  server.ServerMembers.FirstOrDefault(sm => sm.MemberId.Equals(server.OwnerId));
            List<MemberRoleServerDetailReponse> memRoleList = new List<MemberRoleServerDetailReponse>();

            var memberRoles = await _unitOfWork.MemberRoles.GetMembersByRoleIdAsync(role.Id);
            List<ServerMemberDetailResponse> memberDetailResList = new List<ServerMemberDetailResponse>();
            List<ServerMember> serverMembers = await _unitOfWork.ServerMembers.GetAllByRoleIdAsync(role.Id);

            foreach (var serverMember in serverMembers)
            {
                List<InviteServerDetailReponse> inviteRes = new List<InviteServerDetailReponse>();
                InviteUsageServerDetailResponse memUsedInvite = new InviteUsageServerDetailResponse();
                var inviteList = serverMember.Invites;
                if (inviteList != null)
                {
                    foreach (var invite in inviteList)
                    {
                        InviteServerDetailReponse inv = new InviteServerDetailReponse
                        {
                            Id = invite.Id,
                            Code = invite.Code,
                            CreatedAt = invite.CreatedAt,
                            ExpiryDate = invite.ExpiryDate,
                            MaxUses = invite.MaxUses,
                            UpdatedAt = invite.UpdatedAt,
                            Uses = invite.Uses,
                            Status = invite.Status
                        };
                        inviteRes.Add(inv);
                    }
                    if (!serverMember.UserId.Equals(server.OwnerId))
                    {
                        var inviteUsed = await _unitOfWork.InvitesUsages.GetInviteUsageByServerMemberIdAsync(serverMember.Id);
                        if (inviteUsed == null)
                        {
                            memUsedInvite.JoinCode = "";
                            memUsedInvite.JoinAt = server.CreatedAt;
                        }
                        memUsedInvite.JoinCode = inviteUsed.Invite.Code; //check if invite code got deleted
                        memUsedInvite.JoinAt = inviteUsed.UsedAt;
                    }
                    else
                    {
                        // owner ko join bang code
                        memUsedInvite.JoinCode = "";
                        memUsedInvite.JoinAt = server.CreatedAt;
                    }
                }

                ServerMemberDetailResponse tmp = new ServerMemberDetailResponse
                {
                    Banned = serverMember.Banned,
                    Deafened = serverMember.Deafened,
                    JoinedAt = serverMember.JoinedAt,
                    Muted = serverMember.Muted,
                    Nickname = serverMember.Nickname,
                    MemberId = serverMember.UserId,
                    Invites = inviteRes,
                    InvitesUsage = memUsedInvite,
                };
                memberDetailResList.Add(tmp);
            }



            RoleServerDetailReponse response = new RoleServerDetailReponse
            {
                Id = role.Id,
                Color = role.Color,
                CreatedAt = role.CreatedAt,
                Mentionable = role.Mentionable,
                Name = role.Name,
                Position = role.Position,
                permissions = await GetRolePermissions(role.Id, server.Id),
                ServerMembers = memberDetailResList
            };
            return response;
        }
        public async Task<RoleDetailPermissionResponse> GetRolePermissions(Guid roleId, Guid serverId)
        {
            var server = await _unitOfWork.Servers.GetByIdAsync(serverId);

            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);

            if (role == null)
            {
                throw new InvalidOperationException("Role not found in the server.");
            }

            RoleDetailPermissionResponse roleDetailPermission = new RoleDetailPermissionResponse();
            // server Role permission
            List<RolePermissionResponse> serverRolePermissions = new List<RolePermissionResponse>();
            foreach (var permission in role.RolePermissions)
            {
                if (permission.Permission.IsServer && permission.IsGranted)
                {
                    serverRolePermissions.Add(new RolePermissionResponse
                    {
                        Code = permission.Permission.Code,
                    });
                }
            }

            //channel Role permission
            // list all channelId in server
            List<Guid> channelIdList = new List<Guid>();
            foreach (var channel in server.Channels)
            {
                channelIdList.Add(channel.Id);
            }
            //channel Role permission
            List<ChannelPermission> channelRolePermission = new List<ChannelPermission>();

            foreach (var channel in channelIdList)
            {
                // in  1 channel
                List<RolePermissionResponse> permissions = new List<RolePermissionResponse>();

                var currentChannelRolePermissions = role.ChannelRolePermissions.Where(crp => crp.ChannelId.Equals(channel)).ToList();
                foreach (var permission in currentChannelRolePermissions)
                {
                    // 1 permission 
                    if (permission.IsGranted && !permission.Permission.IsServer)
                    {
                        permissions.Add(new RolePermissionResponse
                        {
                            Code = permission.Permission.Code,
                        });
                    }
                }
                channelRolePermission.Add(new ChannelPermission
                {
                    ChannelId = channel,
                    permissions = permissions
                });
            }

            roleDetailPermission.channelPermissions = channelRolePermission;
            roleDetailPermission.serverPermissions = serverRolePermissions;
            return roleDetailPermission;
        }

        public async Task<UserPermissionResponse> SetUserPermission(Guid userId, Guid serverId)
        {
            ServerMemberCreateRequest serverMemberCreate = new ServerMemberCreateRequest()
            {
                ServerId = serverId,
                UserId = userId
            };

            ServerMember serverMember = await _unitOfWork.ServerMembers.FindByUserIdAndServerIdAsync(serverMemberCreate);

            if (serverMember == null)
            {
                throw new InvalidOperationException("This user is not in the server.");
            }

            HashSet<Guid> permissionIds = new HashSet<Guid>();


            // server permission
            List<RolePermissionResponse> permissions = new List<RolePermissionResponse>();

            foreach (var memberRole in serverMember.MemberRoles)
            {
                foreach (var rolePermission in memberRole.Role.RolePermissions)
                {
                    if (rolePermission.IsGranted && permissionIds.Add(rolePermission.Permission.Id))
                    {
                        permissions.Add(new RolePermissionResponse
                        {
                            Code = rolePermission.Permission.Code
                        });
                    }
                }
            }

            // Channel permissions
            List<ChannelPermission> channelPermissions = new List<ChannelPermission>();

            var serverChannels = await _unitOfWork.Channels.GetAllByServerIdAsync(serverId);

            foreach (var channel in serverChannels)
            {
                List<RolePermissionResponse> channelRolePermissions = new List<RolePermissionResponse>();

                var rolesInChannel = await GetRolesInChannel(channel.Id);
                var userRolesInChannel = serverMember.MemberRoles
                    .Select(mr => mr.Role)
                    .Where(role => rolesInChannel.Any(r => r.Id.Equals(role.Id)))
                    .ToList();

                if (userRolesInChannel.Any())
                {
                    //  role with the highest priority (lowest Position value)
                    var highestPriorityRole = userRolesInChannel.OrderBy(r => r.Position).FirstOrDefault();
                    if (highestPriorityRole == null)
                    {
                        throw new InvalidOperationException("No role include in this Server id: " + serverId);
                    }
                    var currentChannelRolePermission = highestPriorityRole.ChannelRolePermissions.Where(crp => crp.ChannelId.Equals(channel.Id)).ToList();

                    var seenPermissionCodes = new HashSet<string>();

                    foreach (var channelRolePermission in currentChannelRolePermission)
                    {
                        var code = channelRolePermission.Permission.Code;

                        if (!channelRolePermission.Permission.IsServer &&
                            channelRolePermission.IsGranted &&
                            seenPermissionCodes.Add(code))
                        {
                            channelRolePermissions.Add(new RolePermissionResponse
                            {
                                Code = code
                            });
                        }
                    }


                    if (channelRolePermissions.Any())
                    {
                        channelPermissions.Add(new ChannelPermission
                        {
                            ChannelId = channel.Id,
                            permissions = channelRolePermissions
                        });
                    }
                }
            }

            // Create and return the response object
            var userPermissionResponse = new UserPermissionResponse
            {
                serverId = serverId,
                userId = userId,
                permissions = permissions,
                channelPermissions = channelPermissions
            };

            var userPermissionJson = JsonConvert.SerializeObject(userPermissionResponse);

            var redisKey = $"userPermission:{userId}:{serverId}";
            await _redisDatabase.StringSetAsync(redisKey, userPermissionJson);

            return userPermissionResponse;
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

            var serverMember = await _unitOfWork.ServerMembers
                .GetByUserIdAndServerIdIncludeRolesPermissionsAsync(new ServerMemberCreateRequest
                {
                    ServerId = channel.ServerId,
                    UserId = userId
                });

            if (serverMember == null)
                throw new InvalidOperationException("This user is not in the server.");

            var allowedRoleIds = (await GetRolesInChannel(channelId))
                .Select(r => r.Id)
                .ToHashSet();

            var userRolesInChannel = serverMember.MemberRoles
                .Select(mr => mr.Role)
                .Where(r => allowedRoleIds.Contains(r.Id))
                .ToList();

            if (!userRolesInChannel.Any())
                return;

            var highestPriorityRole = userRolesInChannel.OrderBy(r => r.Position).First();


            var permissions = highestPriorityRole.ChannelRolePermissions
                .Where(p =>
                    p.ChannelId == channelId &&
                    p.IsGranted &&
                    !p.Permission.IsServer)
                .Select(p => p.Permission.Code)
                .Distinct()
                .Select(code => new RolePermissionResponse { Code = code })
                .ToList();

            var redisKey = $"userPermission:{userId}:{channel.ServerId}:channel:{channelId}";
            await _redisDatabase.StringSetAsync(redisKey, JsonConvert.SerializeObject(permissions));
        }


        public async Task<Server> GetServerPermissionsAsync(Guid serverId)
        {
            var server = await _unitOfWork.Servers.GetServerIncludePermissionsAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            return server;
        }

        public async Task<ServerBackupResponse> GetServerNSetRedisPermissionsAsync(Guid userId, Guid serverId)
        {
            var server = await GetServerDetailByIdAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            await SetUserPermission(userId, serverId);
            return server;
        }

        private async Task<List<Guid>> GetAllChannelIdsByServerId(Guid serverId)
        {
            var server = await _unitOfWork.Servers.GetServerIncludeCateChannelAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            List<Guid> channelIds = new List<Guid>();
            // channel without category
            foreach (var channel in server.Channels)
            {
                channelIds.Add(channel.Id);
            }
            return channelIds;
        }

        public async Task<ServerBackupResponse> GetServerChannelRoleResponseAsync(Guid serverId, Guid userId)
        {
            var server = await _unitOfWork.Servers.GetServerIncludeMembersChannelsAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            ServerMember member = null;
            foreach (var mem in server.ServerMembers)
            {
                if (mem.UserId.Equals(userId))
                {
                    member = mem;
                    break;
                }
            }
            if (member == null)
            {
                throw new InvalidDataException("You are not belong to this server");
            }

            if (member.Banned)
            {
                throw new InvalidOperationException("You had been banned from this server");
            }

            var rs = new ServerBackupResponse();

            rs = await GetServerDetailByIdAsync(serverId);
            // Setting user permissions


            foreach (var channel in server.Channels)
            {
                var allowedRoleIds = (await GetRolesInChannel(channel.Id))
                    .Select(r => r.Id)
                    .ToHashSet();

                var userRolesInChannel = member.MemberRoles
                    .Select(mr => mr.Role)
                    .Where(r => allowedRoleIds.Contains(r.Id))
                    .ToList();

                if (!userRolesInChannel.Any()) continue;

                var highestPriorityRole = userRolesInChannel.OrderBy(r => r.Position).FirstOrDefault();
                if (highestPriorityRole == null) continue;

                var permissions = highestPriorityRole.ChannelRolePermissions
                    .Where(p =>
                        p.ChannelId == channel.Id &&
                        p.IsGranted &&
                        !p.Permission.IsServer)
                    .Select(p => p.Permission.Code)
                    .Distinct()
                    .Select(code => new RolePermissionResponse { Code = code })
                    .ToList();

                var redisKey = $"userPermission:{userId}:{serverId}:channel:{channel.Id}";
                await _redisDatabase.StringSetAsync(redisKey, JsonConvert.SerializeObject(permissions));
            }


            await SetUserGlobalPermission(userId, serverId);



            if (!userId.Equals(rs.OwnerId))
            {
                var channelsToRemove = new List<Guid>();
                var allChannelIds = await GetAllChannelIdsByServerId(server.Id);

                foreach (var channelId in allChannelIds)
                {
                    var isVisible = await HasViewChannelPermissionAsync(server.Id, userId, channelId);
                    if (!isVisible)
                    {
                        channelsToRemove.Add(channelId);
                    }
                }


                rs.Channels = rs.Channels
                    .Where(c => !channelsToRemove.Contains(c.Id))
                    .ToList();


                //within category
                rs.Categories = rs.Categories
                    .Where(cate =>
                    {
                        // Nếu category private
                        if (cate.IsPrivate)
                        {
                            if (!cate.CreatorId.Equals(member.Id))
                            {

                                cate.Channels = cate.Channels
                                    .Where(c => !channelsToRemove.Contains(c.Id))
                                    .ToList();

                                return cate.Channels.Any();
                            }

                        }

                        //  category not private
                        cate.Channels = cate.Channels
                            .Where(c => !channelsToRemove.Contains(c.Id))
                            .ToList();

                        return true;

                    })
                    .ToList();

            }

            return rs;
        }

        public async Task<RoleHierarchyModel> GetRoleHierarchyModelAsync(Guid serverId)
        {
            var server = await GetServerPermissionsAsync(serverId);

            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            var roleHierarchyModel = new RoleHierarchyModel
            {
                ServerId = server.Id,
                Roles = new List<RoleWithMembersModel>()
            };

            var orderedRoles = server.Roles.OrderBy(role => role.Position).ToList();

            foreach (var role in orderedRoles)
            {

                var permissions = role.RolePermissions.Select(rp => new PermissionHierarchyModel
                {
                    Id = rp.PermissionId,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description,
                    IsGranted = rp.IsGranted
                }).ToList();


                var members = role.MemberRoles.Select(mr => new MemberModel
                {
                    MemberId = mr.ServerMemberId,
                    MemberName = mr.ServerMember.Nickname,
                    UserName = mr.ServerMember.User.Username,
                    Avatar = mr.ServerMember.User.Avatar
                }).ToList();


                var roleWithMembers = new RoleWithMembersModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Position = role.Position,
                    Color = role.Color,
                    Permissions = permissions,
                    Members = members
                };


                roleHierarchyModel.Roles.Add(roleWithMembers);
            }
            return roleHierarchyModel;
        }

        public async Task<List<ServerBackupResponse>> GetAllServersCategoriesChannelByUserIdAsync(Guid userId)
        {

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidDataException("User is not found");
            }

            var serverIds = await _unitOfWork.ServerMembers.GetAllServerIdByUserIdAsync(userId);
            if (serverIds == null)
            {
                throw new InvalidDataException("User does not belong to any server");
            }


            var serverDetails = new List<ServerBackupResponse>();
            foreach (var id in serverIds)
            {
                var member = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(id, userId);
                if (!member.Banned)
                {
                    var detail = await GetServersDetailCategoriesAndChannelByIdAsync(id);
                    serverDetails.Add(detail);

                    // Setting user permissions
                    await SetUserPermission(userId, id);
                }
            }

            // check visible for channels without category
            foreach (var server in serverDetails)
            {
                var member = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(server.Id, userId);
                if (member == null)
                {
                    throw new InvalidDataException($"Current member is not belong to server {server.Name} anymore");
                }
                if (userId.Equals(server.OwnerId)) continue;

                var channelsToRemove = new List<Guid>();
                var allChannelIds = await GetAllChannelIdsByServerId(server.Id);

                foreach (var channelId in allChannelIds)
                {
                    var isVisible = await HasViewChannelPermissionAsync(server.Id, userId, channelId);
                    if (!isVisible)
                    {
                        channelsToRemove.Add(channelId);
                    }
                }

                // delete channel without visible permission from server  (outside category)
                server.Channels = server.Channels
                .Where(c => !channelsToRemove.Contains(c.Id))
                .ToList();

                //within category
                server.Categories = server.Categories
                    .Where(cate =>
                    {
                        // if category private
                        if (cate.IsPrivate)
                        {
                            if (!cate.CreatorId.Equals(member.Id))
                            {

                                cate.Channels = cate.Channels
                                    .Where(c => !channelsToRemove.Contains(c.Id))
                                    .ToList();

                                return cate.Channels.Any();
                            }

                        }

                        //  category not private
                        cate.Channels = cate.Channels
                            .Where(c => !channelsToRemove.Contains(c.Id))
                            .ToList();

                        return true;

                    })
                    .ToList();

            }

            return serverDetails;
        }
        public async Task<ServerBackupResponse> GetServersDetailCategoriesAndChannelByIdAsync(Guid id)
        {
            var server = await _unitOfWork.Servers.GetByIdAsync(id);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            ServerBackupResponse serverDetail = new ServerBackupResponse();

            List<ChannelServerDetailResponse> channelServerList = new List<ChannelServerDetailResponse>();

            var sortedChannels = server.Channels
                           .Where(c => c.CategoryId == null)
                           .OrderBy(c => c.Type)
                           .ThenBy(c => c.Position)
                           .ToList();
            foreach (var channel in sortedChannels)
            {
                if (channel.CategoryId == null)
                {
                    channelServerList.Add(ToChannelServerResponse(channel));
                }
            }


            List<CategoryCreateResponse> categoryList = new List<CategoryCreateResponse>();
            var sortedCategories = server.Categories
                                        .Select(category =>
                                        {
                                            category.Channels = category.Channels
                                                                        .OrderBy(ch => ch.Type)
                                                                        .ThenBy(ch => ch.Position)
                                                                        .ToList();
                                            return category;
                                        })
                                        .ToList();

            foreach (var category in sortedCategories)
            {
                List<ChannelServerDetailResponse> channelCategoryList = new List<ChannelServerDetailResponse>();

                foreach (var channel in category.Channels)
                {
                    channelCategoryList.Add(ToChannelServerResponse(channel));
                }
                CategoryCreateResponse categoryCreateResponse = new CategoryCreateResponse
                {
                    Id = category.Id,
                    Channels = channelCategoryList,
                    CreatedAt = category.CreatedAt,
                    Name = category.Name,
                    Position = category.Position,
                    UpdatedAt = category.UpdatedAt,
                    IsPrivate = category.IsPrivate,
                    CreatorId = category.CreatorId
                };
                categoryList.Add(categoryCreateResponse);
            }





            serverDetail.Id = server.Id;
            serverDetail.OwnerId = server.OwnerId;
            serverDetail.Name = server.Name;
            serverDetail.Icon = server.Icon;
            serverDetail.CreatedAt = server.CreatedAt;
            serverDetail.UpdatedAt = server.UpdatedAt;
            serverDetail.Categories = categoryList;
            serverDetail.Channels = channelServerList;


            return serverDetail;
        }
    }
}