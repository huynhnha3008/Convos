using BusinessObjects.DTOs;
using BusinessObjects.DTOs.RoleDto;
using BusinessObjects.DTOs.ServerDto;
using BusinessObjects.DTOs.ServerMemberDto;
using BusinessObjects.DTOs.UserDto;
using BusinessObjects.Models;
using BusinessObjects.QueryObject;
using Microsoft.AspNetCore.SignalR;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.SignalR;
using Services.SignalR.Interfaces;




namespace Services
{
    public class ServerMemberService : IServerMemberService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;
        private readonly IHubContext<ServerHub, IServerHub> _serverHub;

        public ServerMemberService(IUnitOfWork unitOfWork, IPermissionService permissionService, IHubContext<ServerHub,IServerHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;
            _serverHub= hubContext;
        }

        public async Task<List<ServerMemberResponse>> GetAllAsync(Guid serverId, QueryMember query)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if(server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            if(query.SearchTerm == null)
            {
                query.SearchTerm = "";
            }
            var queryMembers = await _unitOfWork.ServerMembers.SearchAsync(query.SearchTerm);

            var availableMember = queryMembers.Where(sm => sm.Banned == false).ToList();

            var members = availableMember.Where(sm => sm.ServerId.Equals(serverId)).ToList();
            if(members == null || members.Count == 0)
            {
                return new List<ServerMemberResponse>();
            }

            switch (query.SortBy.ToString())
            {
                case "Name":
                    members = query.IsDescending ? members.OrderByDescending(c => c.Nickname).ToList() : members.OrderBy(c => c.Nickname).ToList();
                    break;
                case "Muted":
                    members = query.IsDescending ? members.OrderByDescending(c => c.Muted).ToList() : members.OrderBy(c => c.Muted).ToList();
                    break;
                case "Deafened":
                    members = query.IsDescending ? members.OrderByDescending(c => c.Deafened).ToList() : members.OrderBy(c => c.Deafened).ToList();
                    break;
                default:
                    members = members.OrderBy(sm => sm.Nickname).ToList();
                    break;
            }

            var paginatedMembers = members
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            var result = new List<ServerMemberResponse>();
            foreach(var member in paginatedMembers)
            {
                result.Add(await GetServerMemberResponseAsync(serverId, member.Id));
            }
            return result;
        }


        private ServerMemberCreateResponse ToCreateResponseMember (ServerMember serverMember)
        {
            ServerMemberCreateResponse response = new ServerMemberCreateResponse
            {
                Banned = serverMember.Banned,
                Deafened = serverMember.Deafened,
                JoinedAt = serverMember.JoinedAt,
                MemberId = serverMember.UserId,
                Muted = serverMember.Muted,
                Nickname = serverMember.Nickname,
                ServerId = serverMember.ServerId,
            };
            return response;
        }
        public async Task<string> UpdateAsync(ServerMemberCreateRequest request, Guid userId)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(request.ServerId);
            if(server == null)
            {
                throw new InvalidOperationException("Server is not found");
            }
            var memList = await _unitOfWork.ServerMembers.GetAllAsync(request.ServerId);
            var serverMem = memList.SingleOrDefault(m => m.UserId.Equals(request.UserId));
            if (serverMem == null)
            {
                throw new InvalidOperationException("No member is found in this Server");
            }
            if (!userId.Equals(server.OwnerId))
            {
                if(userId.Equals(request.UserId))
                {
                    var userPermission = await _permissionService.GetUserGlobalPermission(serverMem.UserId, server.Id);
                    var hasChangeNamePermission = userPermission
                       .Any(p => p.Code.Equals(PermissionEnum.CHANGE_NICKNAME.ToString()));

                    if (!hasChangeNamePermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: you don't have permission to CHANGE nickname in this server");
                    }
                } else
                {
                    var userPermission = await _permissionService.GetUserGlobalPermission(server.Id, serverMem.UserId);
                    var hasChangeMemberNamePermission = userPermission
                       .Any(p => p.Code.Equals(PermissionEnum.MANAGE_NICKNAMES.ToString()));

                    if (!hasChangeMemberNamePermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: you don't have permission to CHANGE this member is nickname");
                    }
                }
                
            }
            serverMem.Nickname = request.NickName;
            var updatedMem = await _unitOfWork.ServerMembers.UpdateAsync(serverMem);
            await _serverHub.Clients.Group(serverMem.ServerId.ToString()).UpdateMemberName(serverMem.ServerId ,updatedMem.Id, updatedMem.Nickname);
            return "Updated successfully";
        }

        public async Task<ServerMemberResponse> GetByIdAsync(Guid id)
        {
            var member = await _unitOfWork.ServerMembers.GetByIdAsync(id);
            if(member == null)
            {
                throw new InvalidDataException("Member is not found");
            }
            return await GetServerMemberResponseAsync(member.ServerId, member.Id);
        }

        public async Task<string> LeaveServerAsync(Guid serverId, Guid userId)
        {
            ServerMemberCreateRequest request = new ServerMemberCreateRequest
            {
                ServerId = serverId,
                UserId = userId
            };
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if(server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            if(userId.Equals(server.OwnerId))
            {
                throw new InvalidOperationException("Owner cannot leave server. Try DELETE instead");
            }
            var member = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(serverId, userId);
            if(member == null)
            {
                throw new InvalidDataException("Server Member is not found");
            }
            if(member.Banned)
            {
                throw new InvalidDataException("Server is not found (banned)");
            }
            var leftMem = await _unitOfWork.ServerMembers.DeleteAsync(member);
            await _serverHub.Clients.Group(member.ServerId.ToString()).UpdateMemberList();

            return $"{leftMem.Nickname} left server!";
        }

        public async Task<string> KickMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId)
        {

            // Check if the target user is the same as the current user
            if (targetUserId == currentUserId)
            {
                throw new InvalidOperationException("You cannot kick yourself from the server.");
            }

            var server  = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if(server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            if(server.OwnerId.Equals(targetUserId))
            {
                throw new InvalidOperationException("Cannot kick the Owner");
            }

            ServerMemberCreateRequest request = new ServerMemberCreateRequest
            {
                ServerId = serverId,
                UserId = targetUserId
            };
            var targetMem = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(serverId, targetUserId);
            if(targetMem == null)
            {
                throw new InvalidDataException("Member is not found");
            }
            else if(targetMem.Banned)
            {
                throw new InvalidDataException("Member is already banned");
            }
            


            if(!currentUserId.Equals(server.OwnerId))
            {
                var userPermission = await _permissionService.GetUserGlobalPermission(currentUserId, serverId);

                if(userPermission == null)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to KICK this member");
                }
                var hasKickPermission = userPermission
                   .Any(p => p.Code.Equals(PermissionEnum.KICK_MEMBERS.ToString()));

                if (!hasKickPermission)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to KICK this member");
                }
            }
            await _unitOfWork.ServerMembers.DeleteAsync(targetMem);
            await _serverHub.Clients.Group(serverId.ToString()).KickMember(serverId, targetMem.Id);

            return $"{targetMem.Nickname} has been kicked out of server successfully";
        }

        public async Task<string> BanMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId)
        {
            // Check if the target user is the same as the current user
            if (targetUserId == currentUserId)
            {
                throw new InvalidOperationException("You cannot ban yourself from the server.");
            }

            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            if (server.OwnerId.Equals(targetUserId))
            {
                throw new InvalidOperationException("Cannot ban the Owner");
            }
            ServerMemberCreateRequest request = new ServerMemberCreateRequest
            {
                ServerId = serverId,
                UserId = targetUserId
            };
            var targetMem = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(serverId, targetUserId);
            if (targetMem == null)
            {
                throw new InvalidDataException("Member is not found");
            }
            else if (targetMem.Banned)
            {
                throw new InvalidOperationException("Member is already banned");
            }
             // check server owner
            if (!currentUserId.Equals(server.OwnerId))
            {
                var userPermission = await _permissionService.GetUserGlobalPermission(currentUserId, serverId);

                if (userPermission == null)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to BAN this member");
                }
                var hasBanPermission = userPermission
               .Any(p => p.Code.Equals(PermissionEnum.BAN_MEMBERS.ToString()));

                if (!hasBanPermission)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to BAN this member");
                }
            }

            await _unitOfWork.ServerMembers.BanMemberAsync(targetMem);
            await _serverHub.Clients.Group(targetMem.ServerId.ToString()).BanMember(targetMem.ServerId, targetMem.Id);

            return $"{targetMem.Nickname} has been banned from server successfully";
        }

        public async Task<string> MuteMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                throw new InvalidOperationException("Server is not found");
            }
            ServerMemberCreateRequest request = new ServerMemberCreateRequest
            {
                ServerId = serverId,
                UserId = targetUserId
            };
            var member = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(serverId, targetUserId);
            if (member == null)
            {
                throw new InvalidDataException("Process muting failed: Member is not found");
            }
            else if (member.Banned)
            {
                throw new InvalidOperationException("Process muting failed: Member is already banned");
            }
            else if (member.Muted)
            {
                throw new InvalidOperationException("Process muting failed: Member is already muted");
            }

            if (!currentUserId.Equals(server.OwnerId))
            {
                var userPermission = await _permissionService.GetUserGlobalPermission(currentUserId, serverId);

                if (userPermission == null)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to MUTE this member");
                }
                var hasManageServerPermission = userPermission
               .Any(p => p.Code.Equals(PermissionEnum.MUTE_MEMBERS.ToString()));

                if (!hasManageServerPermission)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to MUTE this member");
                }
            }
            member.Muted = true;

            var mutedMem = await _unitOfWork.ServerMembers.UpdateAsync(member);
            // realtime
            await _serverHub.Clients.Group(mutedMem.ServerId.ToString()).MuteMember(mutedMem.ServerId, mutedMem.Id);

            return $"{mutedMem.Nickname} has been muted";
        }

        public async Task<string> DeafenMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                throw new InvalidOperationException("Process deafening failed: Server is not found");
            }
            ServerMemberCreateRequest request = new ServerMemberCreateRequest
            {
                ServerId = serverId,
                UserId = targetUserId
            };
            var member = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(serverId, targetUserId);
            if (member == null)
            {
                throw new InvalidDataException("Process deafening failed: Member is not found");
            }
            else if (member.Banned)
            {
                throw new InvalidOperationException("Process deafening failed: Member is already banned");
            }
            else if (member.Deafened)
            {
                throw new InvalidOperationException("Process deafening failed: Member is already deafened");
            }
           

            if (!currentUserId.Equals(server.OwnerId))
            {
                var userPermission = await _permissionService.GetUserGlobalPermission(currentUserId, serverId);

                if (userPermission == null)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to DEAFEN this member");
                }

                var hasManageServerPermission = userPermission
                   .Any(p => p.Code.Equals(PermissionEnum.DEAFEN_MEMBERS.ToString()));

                if (!hasManageServerPermission)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to DEAFEN this member");
                }
            }
            member.Deafened = true;
            var deafendMem = await _unitOfWork.ServerMembers.UpdateAsync(member);
            // realtime
            await _serverHub.Clients.Group(deafendMem.ServerId.ToString()).DeafenMember(deafendMem.ServerId, deafendMem.Id);

            return $"{deafendMem.Nickname} has been deafened successfully";
        }

        public async Task<string> UnBanMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if(server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            if(targetUserId.Equals(currentUserId))
            {
                throw new InvalidOperationException("You cannot UNBAN yourself. Please contact server'owner or administrator");
            }
            ServerMemberCreateRequest request = new ServerMemberCreateRequest
            {
                ServerId = serverId,
                UserId = targetUserId
            };
            var targetMem = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(serverId, targetUserId);
            if (targetMem == null)
            {
                throw new InvalidDataException("Server Member is not found");
            }
            else if (!targetMem.Banned)
            {
                throw new InvalidOperationException("Process deafening failed: Member is not being banned");
            }

            if (!currentUserId.Equals(server.OwnerId))
            {
                var userPermission = await _permissionService.GetUserGlobalPermission(currentUserId, serverId);

                if (userPermission == null)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to UNBAN this member");
                }

                var hasManageServerPermission = userPermission
                   .Any(p => p.Code.Equals(PermissionEnum.BAN_MEMBERS.ToString()));

                if (!hasManageServerPermission)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to UNBAN this member");
                }
            }

            var unbanMem = await _unitOfWork.ServerMembers.DeleteAsync(targetMem);
            // reio-temu
            await _serverHub.Clients.Group(unbanMem.ServerId.ToString()).UnbanMember(unbanMem.ServerId, unbanMem.Id);


            return $"{unbanMem.Nickname} has been unbanned and no longer member of this server";
        }

        public async Task<string> UnMuteMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            ServerMemberCreateRequest request = new ServerMemberCreateRequest
            {
                ServerId = serverId,
                UserId = targetUserId
            };
            var member = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(serverId, targetUserId);
            if (member == null)
            {
                throw new InvalidDataException("Process unmuting failed: Member is not found");
            }
            else if (member.Banned)
            {
                throw new InvalidOperationException("Process unmuting failed: Member is already banned");
            }
            else if(!member.Muted)
            {
                throw new InvalidOperationException("Process unmuting failed: Member is not being muted");
            }

            if (!currentUserId.Equals(server.OwnerId))
            {
                var userPermission = await _permissionService.GetUserGlobalPermission(currentUserId, serverId);
                
                if(userPermission == null)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to UNMUTE this member");

                }
                var hasManageServerPermission = userPermission
                   .Any(p => p.Code.Equals(PermissionEnum.MUTE_MEMBERS.ToString()));

                if (!hasManageServerPermission)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to UNMUTE this member");
                }
            }
            member.Muted = false;

            var unmutedMem = await _unitOfWork.ServerMembers.UpdateAsync(member);
            // realtime
            await _serverHub.Clients.Group(unmutedMem.ServerId.ToString()).UnmuteMember(unmutedMem.ServerId, unmutedMem.Id);

            return $"{unmutedMem.Nickname} has been unmuted";
        }

        public async Task<string> UnDeafenMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                throw new InvalidOperationException("Server is not found");
            }
            ServerMemberCreateRequest request = new ServerMemberCreateRequest
            {
                ServerId = serverId,
                UserId = targetUserId
            };
            var member = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(serverId, targetUserId);
            if (member == null)
            {
                throw new InvalidDataException("Process undeafening failed: Member is not found");
            }
            else if (member.Banned)
            {
                throw new InvalidOperationException("Process undeafening failed: Member is already banned");
            }
            else if (!member.Deafened)
            {
                throw new InvalidOperationException("Process undeafening failed: Member is not being deafened");
            }
            if (!currentUserId.Equals(server.OwnerId))

            {
                var userPermission = await _permissionService.GetUserGlobalPermission(currentUserId, serverId);

                if (userPermission == null)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to UNDEAFEN this member");

                }
                var hasManageServerPermission = userPermission
               .Any(p => p.Code.Equals(PermissionEnum.DEAFEN_MEMBERS.ToString()));

                if (!hasManageServerPermission)
                {
                    throw new UnauthorizedAccessException("Permission is denied: you don't have permission to UNDEAFEN this member");
                }
            }
            member.Deafened = false;
            var undeafendMem = await _unitOfWork.ServerMembers.UpdateAsync(member);
            // realtime
            await _serverHub.Clients.Group(undeafendMem.ServerId.ToString()).UndeafenMember(undeafendMem.ServerId, undeafendMem.Id);

            return $"{undeafendMem.Nickname} has been undeafened";
        }

        public async Task<ServerMemberResponse> GetServerMemberResponseAsync(Guid serverId, Guid serverMemberId)
        {
            var server  = await _unitOfWork.Servers.GetServerIncludeMembersAsync(serverId);
            if(server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            //  member in server ?
            var member = server.ServerMembers.FirstOrDefault(sm => sm.Id.Equals(serverMemberId));
            if (member == null)
            {
                throw new InvalidDataException("Member is not found in this server");
            }

            if (member.Banned)
            {
                throw new InvalidOperationException("Member is banned from this server");
            }

            //invite response
            List<InviteServerDetailReponse> listInviteRs = new List<InviteServerDetailReponse>();
            var inviteList = await _unitOfWork.Invites.GetAllMemberInvitesAsync(member.Id);
            if (inviteList == null || inviteList.Count == 0)
            {
                inviteList = new List<Invite>();
                listInviteRs = null;
            } else
            {
                foreach (var invite in inviteList)
                {
                    listInviteRs.Add(ToInviteResponse(invite));
                }
            }

            // check is server owner
            bool isServerOwner = member.UserId.Equals(server.OwnerId);
            //invite usage 
            InviteUsageServerDetailResponse inviteUsageRs = new InviteUsageServerDetailResponse();

            if (!isServerOwner)
            {
                var inviteUsed = await _unitOfWork.InvitesUsages.GetInviteUsageByServerMemberIdAsync(member.Id);

                    inviteUsageRs = new InviteUsageServerDetailResponse
                    {
                        JoinCode = inviteUsed.Invite.Code,
                        JoinAt = inviteUsed.UsedAt,
                    };
              
            }
            else
            {
                inviteUsageRs = new InviteUsageServerDetailResponse
                {
                    JoinCode = "",
                    JoinAt = server.CreatedAt,
                };
            }
            
            // memberRole 
            var currentMem = await _unitOfWork.ServerMembers.GetByIdAsync(member.Id);
            List<MemberRoleResponse> memberRoles = new List<MemberRoleResponse>();
            foreach(var memRole in currentMem.MemberRoles)
            {
                memberRoles.Add(await ToMemberRoleResponse(memRole));
            }

            ServerMemberResponse response = new ServerMemberResponse()
            {
                Id = member.Id,
                Nickname = member.Nickname,
                JoinedAt = member.JoinedAt,
                Muted = member.Muted,
                Banned = member.Banned,
                Deafened = member.Deafened,
                User = ToUserModel(currentMem.User),
                Invites = listInviteRs,
                InvitesUsage = inviteUsageRs,
                MemberRoles = memberRoles,
            };
            return response;
        }

        private InviteServerDetailReponse ToInviteResponse(Invite invite)
        {
            InviteServerDetailReponse rs = new InviteServerDetailReponse
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
            return rs;
        }
        private UserModel ToUserModel(User user)
        {
            var usermodel = new UserModel
            {
                Id = user.Id,
                Username = user.Username,
                About = user.About,
                //PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                Banner = user.Banner,
                Birthdate = user.Birthdate,
                DisplayName = user.DisplayName,
                //Email = user.Email,
                Hashtag = user.Hashtag,
                //IsVerified = user.IsVerified,
                JoinedAt = user.JoinedAt,
                //Role = user.Role,
                Status = user.Status,
                Pronouns = user.Pronouns,
                //Password = user.Password,
            };
            return usermodel;
        }

        private async Task<MemberRoleResponse> ToMemberRoleResponse(MemberRole memberRole)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(memberRole.RoleId);
            MemberRoleResponse rs = new MemberRoleResponse
            {
                roleId = role.Id,
                RoleName = role.Name,
                RoleColor = role.Color,
                //permissions = await GetRolePermissions(role.Id, role.ServerId)
            };
            return rs;
        }

        public async Task<RoleDetailPermissionResponse> GetRolePermissions(Guid roleId, Guid serverId)
        {
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

            var server = await _unitOfWork.Servers.GetServerIncludeChannelAsync(serverId);

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

                foreach (var permission in role.ChannelRolePermissions)
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

        public async Task<UserDetailDto> GetMemberUserDetailAsync(Guid currentUserId, Guid memberId)
        {
        
            var targetMember = await _unitOfWork.ServerMembers.GetMemberIncludeUserAsync(memberId);
            if(targetMember == null)
            {
                throw new InvalidDataException("Member is not found");
            }
            var targetUser = targetMember.User;

            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if(currentUser == null)
            {
                throw new InvalidDataException("User is not found");
            }

            List<string> mutualFriends = new List<string>();
            List<string> mutualServers = new List<string>();

            // handle self pointing
            if (!currentUser.Username.Equals(targetUser.Username))
            {
                // Get mutual friends
                mutualFriends = await _unitOfWork.FriendShips.GetMutualFriendsAsync(currentUserId, targetUser.Id);

                // Get mutual servers
                mutualServers = (await _unitOfWork.FriendShips.GetMutualServersAsync(currentUserId, targetUser.Id))
                           .Select(serverId => serverId.ToString())
                           .ToList();
            }

            //roles
            var roleRs = new List<RoleDTO>();
            var memberRoles = await _unitOfWork.MemberRoles.GetAllByMemberId(targetMember.Id);
            var roles = new List<Role>();
            if(memberRoles.Count() != 1)
            { 
                foreach(var memberRole in memberRoles)
                {
                   
                    roles.Add(memberRole.Role);
                }

                // sort 
                var sortedRoles = roles.OrderBy(r => r.Position).ToList();
                foreach (var r in sortedRoles)
                {
                    RoleDTO tmp = new RoleDTO
                    {
                        Color = r.Color,
                        Name = r.Name,
                    };
                    roleRs.Add(tmp);
                }
            }
            return new UserDetailDto
            {
                userId = targetUser.Id,
                DisplayName = targetUser.DisplayName,
                Username = targetUser.Username,
                Hashtag = targetUser.Hashtag,
                Banner = targetUser.Banner,
                Status = targetUser.Status.ToString(),
                About = targetUser.About,
                JoinedAt = targetUser.JoinedAt,
                MutualFriends = mutualFriends,
                roles = roleRs,
                MutualServers = mutualServers
            };
        }

        public async Task<List<BannedMemberResponse>> GetBannedMembersAsync(Guid serverId)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            var serverMembers = await _unitOfWork.ServerMembers.GetAllAsync(serverId);

            var bannedMembers = serverMembers.Where(sm => sm.Banned == true).ToList();

            if (bannedMembers == null || bannedMembers.Count == 0)
            {
                return new List<BannedMemberResponse>();
            }


            var result = new List<BannedMemberResponse>();
            foreach (var member in bannedMembers)
            {
                result.Add(await ToBannedMemberResponse(serverId, member.Id));
            }
            return result;
        }

        private async Task<BannedMemberResponse> ToBannedMemberResponse(Guid serverId,  Guid serverMemberId)
        {
            var server = await _unitOfWork.Servers.GetServerIncludeMembersAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            //  member in server ?
            var member = server.ServerMembers.FirstOrDefault(sm => sm.Id.Equals(serverMemberId));
            if (member == null)
            {
                throw new InvalidDataException("Member is not found in this server");
            }

            // check is server owner
            bool isServerOwner = member.UserId.Equals(server.OwnerId);
            //invite usage 
            InviteUsageServerDetailResponse inviteUsageRs = new InviteUsageServerDetailResponse();

            if (!isServerOwner)
            {
                var inviteUsed = await _unitOfWork.InvitesUsages.GetInviteUsageByServerMemberIdAsync(member.Id);

                inviteUsageRs = new InviteUsageServerDetailResponse
                {
                    JoinCode = inviteUsed.Invite.Code,
                    JoinAt = inviteUsed.UsedAt,
                };

            }
            else
            {
                inviteUsageRs = new InviteUsageServerDetailResponse
                {
                    JoinCode = "",
                    JoinAt = server.CreatedAt,
                };
            }

            var rs = new BannedMemberResponse
            {
                Id = serverMemberId,
                Nickname = member.Nickname,
                Banned = member.Banned,
                Deafened = member.Deafened,
                Muted = member.Muted,
                JoinedAt = member.JoinedAt,
                UserId = member.UserId,
                InvitesUsage = inviteUsageRs,
                Avatar = member.User.Avatar
            };
            return rs;

        }
    }
}
