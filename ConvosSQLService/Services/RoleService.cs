using BusinessObjects.DTOs;
using BusinessObjects.DTOs.RoleDto;
using BusinessObjects.Models;
using BusinessObjects.QueryObject;
using DocumentFormat.OpenXml.Office2010.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.SignalR;
using Services.SignalR.Interfaces;
using Vonage.Users;

namespace Services
{
    public class RoleService : IRoleService
    {
        private readonly IHubContext<RoleHubs, IRoleHubs> _hubContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;
        private readonly StackExchange.Redis.IDatabase _redisDatabase;


        public RoleService(IHubContext<RoleHubs, IRoleHubs> hubContext, IUnitOfWork unitOfWork, IPermissionService permissionService, StackExchange.Redis.IConnectionMultiplexer redis)
        {
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;
            _redisDatabase = redis.GetDatabase();
        }

        public async Task<List<Guid>> GetAllRoleIdByServerIdAsync(Guid serverId)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            var serverRoles = await _unitOfWork.Roles.GetRolesByServerIdAsync(serverId);
            if (serverRoles == null)
            {
                return new List<Guid>();
            }

            var rs = new List<Guid>();
            foreach (var role in serverRoles)
            {
                rs.Add(role.Id);
            }
            return rs;
        }

        public async Task<IEnumerable<RoleCreateResponse>> GetAllByServerIdAsync(Guid serverId, QueryRole query)
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
            var serverRoles = await _unitOfWork.Roles.GetRolesByServerIdAsync(serverId);
            if (serverRoles == null)
            {
                return new List<RoleCreateResponse>();
            }

            // filter Name
            var roleList = serverRoles.Where(r => r.Name.Contains(query.SearchTerm)).ToList();
            if (roleList.Count == 0)
            {
                return new List<RoleCreateResponse>();
            }
            //filter mentionable
            var roles = roleList.Where(r => r.Mentionable);

            if (!query.IsMentionable)
            {
                roles = roleList.Where(r => !r.Mentionable);
            }


            switch (query.SortBy.ToString())
            {
                case "Name":
                    roles = query.IsDescending ? roles.OrderByDescending(c => c.Name).ToList() : roles.OrderBy(c => c.Name).ToList();
                    break;
                case "CreateAt":
                    roles = query.IsDescending ? roles.OrderByDescending(c => c.CreatedAt).ToList() : roles.OrderBy(c => c.CreatedAt).ToList();
                    break;
                case "Color":
                    roles = query.IsDescending ? roles.OrderByDescending(c => c.Color).ToList() : roles.OrderBy(c => c.Color).ToList();
                    break;
                default:
                    roles = roles.OrderBy(s => s.Name).ToList();
                    break;
            }

            var paginatedRoles = roles
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            var rs = new List<RoleCreateResponse>();
            foreach (var role in paginatedRoles)
            {
                rs.Add(MapToResponse(role));
            }
            return rs;
        }

        public async Task<RoleCreateResponse> GetByIdAsync(Guid id)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if (role == null)
                throw new InvalidDataException("Role is not found");

            return MapToResponse(role);
        }

        public async Task<string> ChangeRolePositionInServerAsync(Guid roleId, int newPosition, Guid serverId, Guid userId)
        {
            var server = await _unitOfWork.Servers.GetByIdAsync(serverId);

            if (server == null) return "Server not found!";

            var targetRole = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (targetRole == null) return "Role not found";



            var existedMember = server.ServerMembers.FirstOrDefault(sm => sm.UserId.Equals(userId));
            if (existedMember == null)
            {
                throw new InvalidOperationException("You are not belong to this server");
            }

            if (!userId.Equals(server.OwnerId))
            {

                var hasManageChannelPermission = await HasManageRolePermissionAsync(server.Id, userId);

                if (!hasManageChannelPermission)
                {
                    throw new UnauthorizedAccessException("Permission is denied: You don't have permission to UPDATE role in this server");
                }

                if (newPosition < targetRole.Position)
                {
                    return "Cant not change position with Role that higher than yours";
                }

            }

            if (newPosition >= server.Roles.Count())
            {
                newPosition = server.Roles.Count() - 1;
            }
            var role = server.Roles.FirstOrDefault(r => r.Id.Equals(roleId));

            if (role == null) return "Role not found in the server.";

            var oldPosition = role.Position;

            if (oldPosition == newPosition) return "New position is the same as the old position.";

            var serverRoles = server.Roles.OrderBy(r => r.Position).ToList();

            if (oldPosition > newPosition)
            {
                var rolesToShift = serverRoles
                    .Where(r => r.Position >= newPosition && r.Position < oldPosition)
                    .ToList();

                foreach (var r in rolesToShift) r.Position++;
                role.Position = newPosition;
            }
            else if (oldPosition < newPosition)
            {
                var rolesToShift = serverRoles
                    .Where(r => r.Position > oldPosition && r.Position <= newPosition)
                    .ToList();

                foreach (var r in rolesToShift) r.Position--;
                role.Position = newPosition;
            }

            await _unitOfWork.Servers.UpdateServerRolePositionsAsync(serverId, serverRoles);

            string serverIdUpper = serverId.ToString().ToUpper();
            await _hubContext.Clients.Group(serverIdUpper)
            .OnRoleHierarchyUpdated(serverId.ToString());

            return "Role positions in the server have been updated successfully.";
        }

        private async Task<bool> HasManageRolePermissionAsync(Guid serverId, Guid userId)
        {
            var userPermission = await _permissionService.GetUserGlobalPermission(userId, serverId);
            if (userPermission == null)
            {
                return false;
            }
            var hasManageRolePermission = userPermission
                .Any(p => p.Code.Equals(PermissionEnum.MANAGE_ROLES.ToString()));

            if (!hasManageRolePermission)
            {
                return false;
            }
            return true;
        }

        public async Task<RoleCreateResponse> CreateAsync(RoleCreateRequest roleRequest, Guid userId)
        {
            var server = await _unitOfWork.Servers.GetByIdAsync(roleRequest.ServerId);
            if (server == null)
            {
                throw new InvalidOperationException("Server is not found.");
            }

            if (!userId.Equals(server.OwnerId))
            {
                var hasManageChannelPermission = await HasManageRolePermissionAsync(server.Id, userId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't have permission to CREATE role in this server");
                    }
                }
            }

            var existingRole = await _unitOfWork.Roles.FindByNameAndServerIdAsync(roleRequest.Name, roleRequest.ServerId);
            if (existingRole != null)
            {
                throw new InvalidOperationException("A role with the same name already exists in this server.");
            }



            if (roleRequest.Name.Length <= 0)
            {
                throw new InvalidOperationException("Name is required!.");
            }

            if (roleRequest.Color.Length <= 0)
            {
                throw new InvalidOperationException("Color is required!.");
            }

            var everyoneRole = server.Roles.FirstOrDefault(r => r.Name == "@everyone");

            int position;
            if (server.Roles == null || !server.Roles.Any())
            {
                position = 1;
            }
            else
            {
                position = everyoneRole.Position;
                everyoneRole.Position = server.Roles.Count() + 1;
            }

            var role = new Role
            {
                Name = roleRequest.Name,
                Color = roleRequest.Color,
                CreatedAt = DateTime.UtcNow,
                Mentionable = true,
                Position = position,
                ServerId = roleRequest.ServerId,
            };
            Role createdRole = await _unitOfWork.Roles.CreateAsync(role);
            await AssignDefaultEveryonePermissions(createdRole.Id);
            await _unitOfWork.Roles.UpdateAsync(everyoneRole);

            string serverIdUpper = roleRequest.ServerId.ToString().ToUpper();
            await _hubContext.Clients.Group(serverIdUpper)
            .OnRoleCreated(serverIdUpper, createdRole.Name);
            return MapToResponse(createdRole);

        }

        public async Task<int> AssignPermissionsToRoleAsync(Guid roleId)
        {
            // Retrieve all permissions and filter for server permissions
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            var serverPermissions = permissions.Where(p => p.IsServer).ToList();

            // Retrieve the role by ID
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null)
            {
                return 0; // Return 0 if the role does not exist
            }

            // Define the default permissions list
            var defaultPermissions = new HashSet<string>
    {
        PermissionEnum.CREATE_INVITE.ToString(),
        PermissionEnum.MANAGE_MESSAGES.ToString(),
        PermissionEnum.CHANGE_NICKNAME.ToString(),
        PermissionEnum.CONNECT.ToString()
    };

            // Add permissions to the role
            foreach (var permission in serverPermissions)
            {
                // Check if the permission is in the default permissions list
                bool isGranted = defaultPermissions.Contains(permission.Code);
                {
                    RolePermission rolePermission = new RolePermission
                    {
                        RoleId = roleId,
                        Role = role,
                        PermissionId = permission.Id,
                        IsGranted = isGranted,
                        Permission = permission
                    };

                    role.RolePermissions.Add(rolePermission);
                }

            }

            // Update the role with new permissions
            var updatedRole = await _unitOfWork.Roles.UpdateAsync(role);

            await AddRoleChannelPermission(roleId);

            return 1; // Return 1 to indicate success
        }

        // server 's permission
        public async Task AssignDefaultEveryonePermissions(Guid roleId)
        {
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            var serverPermissions = permissions.Where(p => p.IsServer).ToList();

            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);

            if (role != null)
            {
                foreach (var permission in serverPermissions)
                {
                    if (
                         permission.Code.Equals(PermissionEnum.CREATE_INVITE.ToString()) ||
                         permission.Code.Equals(PermissionEnum.MANAGE_MESSAGES.ToString()) ||
                         permission.Code.Equals(PermissionEnum.CHANGE_NICKNAME.ToString()) ||
                         permission.Code.Equals(PermissionEnum.CONNECT.ToString()) ||
                         permission.Code.Equals(PermissionEnum.MANAGE_EMOJIS.ToString())
                         )

                    {
                        RolePermission rolePermission = new RolePermission
                        {
                            RoleId = roleId,
                            Role = role,
                            PermissionId = permission.Id,
                            IsGranted = true,
                            Permission = permission
                        };
                        await _unitOfWork.RolePermissions.CreateAsync(rolePermission);
                    }
                    else
                    {
                        RolePermission rolePermission = new RolePermission
                        {
                            RoleId = roleId,
                            PermissionId = permission.Id,
                            IsGranted = false
                        };
                        await _unitOfWork.RolePermissions.CreateAsync(rolePermission);
                    }
                }
                var membersRole = await _unitOfWork.ServerMembers.GetAllByRoleIdAsync(roleId);
                if (membersRole != null)
                {
                    foreach (var member in membersRole)
                    {
                        await SetUserGlobalPermission(member.UserId, role.ServerId);
                    }
                }
            }
        }

        private async Task AddRoleChannelPermission(Guid roleId)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null)
                throw new InvalidOperationException("Role not found");

            var server = await _unitOfWork.Servers.GetByIdAsync(role.ServerId);
            var channelList = server.Channels;

            if (role.ChannelRolePermissions == null)
            {
                role.ChannelRolePermissions = new List<ChannelRolePermission>();
            }

            IEnumerable<Permission> permissions = await _unitOfWork.Permissions.GetAllAsync();
            IEnumerable<Permission> channelPermissions = permissions.Where(p => !p.IsServer).ToList();
            foreach (var permission in channelPermissions)
            {
                foreach (var channel in channelList)
                {
                    ChannelRolePermission rolePermission = new ChannelRolePermission
                    {
                        RoleId = roleId,
                        ChannelId = channel.Id,
                        PermissionId = permission.Id,
                        IsGranted = false
                    };
                    await _unitOfWork.ChannelRolePermissions.CreateAsync(rolePermission);
                }
            }
        }


        public async Task<string> UpdateAsync(Guid id, Guid userId, RoleUpdateRequest roleUpdateRequest)
        {
            var existingRole = await _unitOfWork.Roles.GetByIdAsync(id);
            if (existingRole == null)
            {
                throw new InvalidDataException("Role not found.");
            }

            if (existingRole.Name.Equals("@everyone"))
            {
                throw new InvalidOperationException("Cannot update default role - @everyone");
            }

            var server = await _unitOfWork.Servers.GetServerOnlyAsync(existingRole.ServerId);

            if (!userId.Equals(server.OwnerId))
            {
                var hasManageChannelPermission = await HasManageRolePermissionAsync(server.Id, userId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't have permission to UPDATE role in this server");
                    }
                }
            }

            existingRole.Name = roleUpdateRequest.Name;
            existingRole.Color = roleUpdateRequest.Color;
            Role updatedRole = await _unitOfWork.Roles.UpdateAsync(existingRole);
            if (updatedRole != null)
            {
                string serverIdUpper = updatedRole.ServerId.ToString().ToUpper();
                await _hubContext.Clients.Group(serverIdUpper)
                .OnRoleUpdated(serverIdUpper, updatedRole.Name);

                return "Update role failed";
            }
            throw new InvalidOperationException("Role update failed.");
        }


        public async Task<List<RoleCreateResponse>> SearchAsync(Guid serverId, QueryRole query)
        {
            if (query.SearchTerm == null)
            {
                query.SearchTerm = "";
            }
            var server = await _unitOfWork.Servers.GetByIdAsync(serverId);
            if (server == null)
            {
            }
            var serverRoles = await _unitOfWork.Roles.GetRolesByServerIdAsync(serverId);
            if (serverRoles == null)
            {
                return new List<RoleCreateResponse>();
            }

            // filter Name
            var roleList = serverRoles.Where(r => r.Name.Contains(query.SearchTerm)).ToList();

            //filter mentionable
            var roles = roleList.Where(r => r.Mentionable);

            if (!query.IsMentionable)
            {
                roles = roleList.Where(r => r.Mentionable);
            }


            switch (query.SortBy.ToString())
            {
                case "Name":
                    roles = query.IsDescending ? roles.OrderByDescending(c => c.Name).ToList() : roles.OrderBy(c => c.Name).ToList();
                    break;
                case "CreateAt":
                    roles = query.IsDescending ? roles.OrderByDescending(c => c.CreatedAt).ToList() : roles.OrderBy(c => c.CreatedAt).ToList();
                    break;
                case "Color":
                    roles = query.IsDescending ? roles.OrderByDescending(c => c.Color).ToList() : roles.OrderBy(c => c.Color).ToList();
                    break;
                default:
                    roles = roles.OrderBy(s => s.Name).ToList();
                    break;
            }

            var paginatedRoles = roles
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            var rs = new List<RoleCreateResponse>();
            foreach (var role in paginatedRoles)
            {
                rs.Add(MapToResponse(role));
            }
            return rs;
        }

        public async Task<string> DeleteAsync(Guid id, Guid userId)
        {
            bool isAllow = false;
            var existingRole = await _unitOfWork.Roles.GetByIdAsync(id);
            if (existingRole == null)
            {
                throw new InvalidDataException("Role not found.");
            }
            if (existingRole.Name.Equals("@everyone"))
            {
                throw new InvalidOperationException("You cannot DELETE role @everyone");
            }
            var server = await _unitOfWork.Servers.GetByIdAsync(existingRole.ServerId);
            var member = server.ServerMembers.FirstOrDefault(sm => sm.UserId.Equals(userId));
            if (member == null)
            {
                throw new InvalidDataException("You are not belong to this server");
            }
            var memberRoleList = await _unitOfWork.MemberRoles.GetAllByMemberId(member.Id);
            if (memberRoleList == null)
            {
                throw new UnauthorizedAccessException("Permission is denied: You don't have permission to DELETE role in this server");
            }


            if (!userId.Equals(server.OwnerId))
            {
                var hasManageChannelPermission = await HasManageRolePermissionAsync(server.Id, userId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't have permission to DELETE role in this server");
                    }
                }
            }
            else
            {
                isAllow = true;
            }

            foreach (var role in memberRoleList)
            {
                if (role.Role.Position < existingRole.Position)
                {
                    isAllow = true;
                    break;
                }
            }

            if (isAllow)
            {
                Role deletedRole = await _unitOfWork.Roles.DeleteAsync(existingRole);
                string serverIdUpper = deletedRole.ServerId.ToString().ToUpper();
                await _hubContext.Clients.Group(serverIdUpper)
                .OnRoleDeleted(serverIdUpper, deletedRole.Id.ToString());
                return "Role deleted successfully";
            }
            else
            {
                throw new InvalidOperationException("You cannot DELETE a role with position above your role");
            }
        }

        private RoleCreateResponse MapToResponse(Role role)
        {
            return new RoleCreateResponse
            {
                Id = role.Id,
                Name = role.Name,
                Color = role.Color,
                CreatedAt = role.CreatedAt,
                Mentionable = role.Mentionable,
                Position = role.Position,
                ServerId = role.ServerId
            };
        }

        private List<RoleCreateResponse> MapToResponseList(IEnumerable<Role> roles)
        {
            var responseList = new List<RoleCreateResponse>();
            foreach (var role in roles)
            {
                responseList.Add(MapToResponse(role));
            }
            return responseList;
        }


        /// <summary>
        /// only use for a member to assign role to others (assignedRole should lower level of priority than the assigner is role)
        /// </summary>
        /// <param name="serverMemberId"></param>
        /// <param name="RoleId"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task<string> AddMemberRole(Guid serverMemberId, Guid RoleId, Guid currentUserId)
        {
            var targetServerMember = await _unitOfWork.ServerMembers.GetByIdAsync(serverMemberId);
            if (targetServerMember == null)
            {
                throw new InvalidOperationException("ServerMember not found!");
            }


            if (targetServerMember.MemberRoles.Any(mr => mr.RoleId == RoleId))
            {
                throw new InvalidOperationException("Role has been assigned to this member already");
            }


            var server = await _unitOfWork.Servers.GetServerIncludeMembersAsync(targetServerMember.ServerId);
            var role = await _unitOfWork.Roles.GetByIdAsync(RoleId);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found!");
            }
            var existedMember = server.ServerMembers.FirstOrDefault(sm => sm.UserId.Equals(currentUserId));
            if (existedMember == null)
            {
                throw new InvalidOperationException("You are not belong to this server");
            }

            var memberRoles = await _unitOfWork.MemberRoles.GetAllByMemberId(existedMember.Id);
            if (memberRoles == null)
            {
                throw new InvalidOperationException("Member has no role in the server");
            }
            List<Role> roles = new List<Role>();
            foreach (var item in memberRoles)
            {
                var tmp = await _unitOfWork.Roles.GetByIdAsync(item.RoleId);
                roles.Add(tmp);
            }
            if (roles == null || roles.Count == 0)
            {
                throw new InvalidDataException("Member has no role in the server");
            }
            var lowestPositionRole = roles.OrderBy(r => r.Position).FirstOrDefault();
            if (!currentUserId.Equals(server.OwnerId))
            {

                var hasManageChannelPermission = await HasManageRolePermissionAsync(server.Id, currentUserId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't have permission to UPDATE role in this server");
                    }
                }
                if (lowestPositionRole.Position >= role.Position)
                {
                    throw new InvalidOperationException("Permission is denied: You cannot update permission of a Role has priority HIGHER than (or equal) your Role");
                }
            }

            MemberRole memberRole = new MemberRole
            {
                ServerMemberId = serverMemberId,
                RoleId = RoleId
            };

            MemberRole createdMemberRole = await _unitOfWork.MemberRoles.CreateAsync(memberRole);

            // set redis
            await SetUserGlobalPermission(targetServerMember.UserId, targetServerMember.ServerId);
            var channelEffected = await _unitOfWork.Channels.GetAllByRoleIdAsync(RoleId);
            foreach (var channel in channelEffected)
            {
                await SetUserChannelPermission(targetServerMember.UserId, channel.Id);
            }

            string serverId = role.ServerId.ToString();

            await _hubContext.Clients.Group(serverId)
            .OnUserAssignedToRole(serverId, targetServerMember.Nickname.ToString(), role.Name.ToString());

            string SignalRoleId = RoleId.ToString();

            await _hubContext.Clients.Group(SignalRoleId)
                .UpdateChannelListOnChangePermission(SignalRoleId, currentUserId.ToString());

            return $"Assigned role {role.Name} to {targetServerMember.Nickname} successfully";
        }
        public async Task<string> RemoveMemberRole(Guid serverMemberId, Guid roleId, Guid currentUserId)
        {
            var serverMember = await _unitOfWork.ServerMembers.GetByIdAsync(serverMemberId);
            if (serverMember == null)
            {
                throw new InvalidOperationException("ServerMember not found!");
            }

            if (!serverMember.MemberRoles.Any(mr => mr.RoleId == roleId))
            {
                throw new InvalidOperationException("Target member don't have this role");
            }
            var server = await _unitOfWork.Servers.GetByIdAsync(serverMember.ServerId);
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found!");
            }

            if (role.Name.Equals("@everyone"))
            {
                throw new InvalidOperationException("Cannot unassign default role - @everyone");
            }

            var memberRole = await _unitOfWork.MemberRoles.GetByMemberIdAndRoleAsync(serverMemberId, roleId);

            if (memberRole == null)
            {
                throw new InvalidOperationException("MemberRole not found!");
            }
            var existedMember = server.ServerMembers.FirstOrDefault(sm => sm.UserId.Equals(currentUserId));
            if (existedMember == null)
            {
                throw new InvalidOperationException("You are not belong to this server");
            }

            var memberRoles = await _unitOfWork.MemberRoles.GetAllByMemberId(existedMember.Id);
            if (memberRoles == null)
            {
                throw new InvalidOperationException("Member has no role in the server");
            }
            List<Role> roles = new List<Role>();
            foreach (var item in memberRoles)
            {
                var tmp = await _unitOfWork.Roles.GetByIdAsync(item.RoleId);
                roles.Add(tmp);
            }
            if (roles == null || roles.Count == 0)
            {
                throw new InvalidDataException("Member has no role in the server");
            }
            var lowestPositionRole = roles.OrderBy(r => r.Position).FirstOrDefault();
            if (!currentUserId.Equals(server.OwnerId))
            {

                var hasManageChannelPermission = await HasManageRolePermissionAsync(server.Id, currentUserId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't have permission to UPDATE role in this server");
                    }
                }
                if (lowestPositionRole.Position >= role.Position)
                {
                    throw new InvalidOperationException("Permission is denied: You cannot update permission of a Role has priority HIGHER than (or equal) your Role");
                }
            }

            await _unitOfWork.MemberRoles.DeleteAsync(memberRole);

            string serverId = role.ServerId.ToString();

            await _hubContext.Clients.Group(serverId)
                .OnUserUnassignedFromRole(serverId, serverMember.Nickname.ToString(), role.Name.ToString());

            string SignalRoleId = roleId.ToString();

            await _hubContext.Clients.Group(SignalRoleId)
                .UpdateChannelListOnChangePermission(SignalRoleId, currentUserId.ToString());


            return $"Unassigned role {role.Name} to {serverMember.Nickname} successfully";
        }

        public async Task AssignDefaultEveryonePermissionsChannel(Guid channelId, Guid roleId)
        {
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            var channelPermissions = permissions.Where(p => !p.IsServer).ToList();
            var channel = await _unitOfWork.Channels.GetByIdAsync(channelId)
                ?? throw new InvalidDataException("Channel not found");
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId)
                ?? throw new InvalidDataException("Role not found for assigning channel permissions");

            bool isViewChannelGranted = !channel.IsPrivate;
            var defaultGrantedPermissions = new HashSet<string>
    {
        PermissionEnum.SEND_MESSAGES_CHANNEL.ToString(),
        PermissionEnum.ATTACH_FILES_CHANNEL.ToString(),
        PermissionEnum.MENTION_ALL.ToString(),
        PermissionEnum.SEND_VOICE_MESSAGES_CHANNEL.ToString()
    };

            foreach (var permission in channelPermissions)
            {
                bool isGranted = defaultGrantedPermissions.Contains(permission.Code) ||
                                 (permission.Code == PermissionEnum.VIEW_CHANNEL.ToString() && isViewChannelGranted);

                var rolePermission = new ChannelRolePermission
                {
                    RoleId = roleId,
                    Role = role,
                    ChannelId = channelId,
                    Channel = channel,
                    PermissionId = permission.Id,
                    IsGranted = isGranted,
                    Permission = permission
                };

                await _unitOfWork.ChannelRolePermissions.CreateAsync(rolePermission);
            }

            var membersRole = await _unitOfWork.ServerMembers.GetAllByRoleIdAsync(roleId);
            if (membersRole != null)
            {
                foreach (var member in membersRole)
                {
                    await SetUserChannelPermission(member.UserId, channelId);
                }
            }
        }


        public async Task<int> UpdatePermissionsToRoleAsync(Guid roleId, List<UpdateRolePermissionDTO> updateRoleDTOs, Guid currentUserId)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null)
            {
                throw new InvalidDataException("Role is not found");
            }
            var server = await _unitOfWork.Servers.GetByIdAsync(role.ServerId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            var existedMember = server.ServerMembers.FirstOrDefault(sm => sm.UserId.Equals(currentUserId));
            if (existedMember == null)
            {
                throw new InvalidOperationException("You are not belong to this server");
            }

            var memberRoles = await _unitOfWork.MemberRoles.GetAllByMemberId(existedMember.Id);
            if (memberRoles == null)
            {
                throw new InvalidOperationException("Member has no role in the server");
            }
            List<Role> roles = new List<Role>();
            foreach (var item in memberRoles)
            {
                var tmp = await _unitOfWork.Roles.GetByIdAsync(item.RoleId);
                roles.Add(tmp);
            }
            if (roles == null || roles.Count == 0)
            {
                throw new InvalidDataException("Member has no role in the server");
            }
            var lowestPositionRole = roles.OrderBy(r => r.Position).FirstOrDefault();


            if (!currentUserId.Equals(server.OwnerId))
            {

                var hasManageChannelPermission = await HasManageRolePermissionAsync(server.Id, currentUserId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't have permission to UPDATE role in this server");
                    }
                }
                if (lowestPositionRole.Position >= role.Position)
                {
                    throw new InvalidOperationException("Permission is denied: You cannot update permission of a Role has priority HIGHER than (or equal) your Role");
                }
            }

            List<RolePermission> existedRolePermissions = new List<RolePermission>();
            existedRolePermissions = await _unitOfWork.RolePermissions.GetAllByRoleId(roleId);
            foreach (var updateRolePermission in updateRoleDTOs)
            {
                if (existedRolePermissions.Contains(await _unitOfWork.RolePermissions.GetRolePermissionById(roleId, updateRolePermission.PermissionId)))
                {
                    foreach (var existedRolePermission in existedRolePermissions)
                    {
                        if (updateRolePermission.PermissionId.Equals(existedRolePermission.PermissionId))
                        {
                            existedRolePermission.IsGranted = updateRolePermission.IsGranted;
                        }
                    }
                }
                else
                {
                    return 0;
                }
            }

            await _unitOfWork.RolePermissions.UpdateRangeAsync(existedRolePermissions);

            // realtime
            foreach (var update in updateRoleDTOs)
            {
                await _hubContext.Clients.Group(server.Id.ToString())
                    .OnUpdatePermission(server.Id.ToString(), roleId.ToString(), update.PermissionId.ToString());
            }

            // update redis permission for all user contains role
            var serverMembersRole = await _unitOfWork.ServerMembers.GetAllByRoleIdAsync(roleId);
            if (serverMembersRole != null)
            {
                foreach (var member in serverMembersRole)
                {
                    await SetUserGlobalPermission(member.UserId, member.ServerId);
                }
            }

            return 1;
        }

        public async Task<int> UpdatePermissionsToChannelRoleAsync(Guid roleId, List<UpdateChannelRolePermissionDTO> updateRoleDTOs, Guid channelId, Guid currentUserId)
        {

            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null)
            {
                throw new InvalidDataException("Role is not found");
            }
            var channel = await _unitOfWork.Channels.GetByIdAsync(channelId);
            if (channel == null)
            {
                throw new InvalidDataException("Channel is not found");

            }
            bool isRoleInChannel = false;
            // check role assigned to target channel or not
            foreach (var channelRole in channel.ChannelRolePermissions)
            {
                if (channelRole.RoleId.Equals(roleId))
                {
                    isRoleInChannel = true;
                    break;
                }
            }
            if (!isRoleInChannel)
            {
                throw new InvalidOperationException($"{role.Name} is not assigned to {channel.Name}");
            }

            var server = await _unitOfWork.Servers.GetByIdAsync(role.ServerId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            var existedMember = server.ServerMembers.FirstOrDefault(sm => sm.UserId.Equals(currentUserId));
            if (existedMember == null)
            {
                throw new InvalidOperationException("You are not belong to this server");
            }

            var memberRoles = await _unitOfWork.MemberRoles.GetAllByMemberId(existedMember.Id);
            if (memberRoles == null)
            {
                throw new InvalidOperationException("Member has no role in the server");
            }
            // list contains all Role that user have in this server
            List<Role> userRoles = new List<Role>();
            foreach (var item in memberRoles)
            {
                var tmp = await _unitOfWork.Roles.GetByIdAsync(item.RoleId);
                userRoles.Add(tmp);
            }
            if (userRoles == null || userRoles.Count == 0)
            {
                throw new InvalidDataException("Member has no role in the server");
            }

            var channelRolePermissions = await _unitOfWork.ChannelRolePermissions.GetAllRolePermissionsAsync(channelId);
            if (channelRolePermissions == null)
            {
                throw new InvalidOperationException($"Role {role.Name}: no channel is permission is found");
            }

            // take all the role Id from target channel
            HashSet<Guid> roleIdList = new HashSet<Guid>();
            foreach (var channelRole in channelRolePermissions)
            {
                roleIdList.Add(channelRole.RoleId);
            }

            // get object role from roleId in roleIdList
            List<Role> rolesInChannel = new List<Role>();
            foreach (var tmp in roleIdList)
            {
                rolesInChannel.Add(await _unitOfWork.Roles.GetByIdAsync(tmp));
            }

            // join 2 list

            var userRolesInChannel = userRoles.Where(role => rolesInChannel.Any(r => r.Id.Equals(role.Id))).ToList();
            if (userRolesInChannel.Count == 0)
            {
                throw new InvalidOperationException("Conflict between member is roles and roles in current channel");
            }

            var lowestUserPositionRole = userRolesInChannel.OrderBy(r => r.Position).FirstOrDefault();

            if (!currentUserId.Equals(server.OwnerId))
            {

                var hasManageChannelPermission = await HasManageRolePermissionAsync(server.Id, currentUserId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't have permission to UPDATE role in this server");
                    }
                }

                if (lowestUserPositionRole.Position >= role.Position)
                {
                    throw new InvalidOperationException("Permission is denied: You cannot update permissions of a Role has priority HIGHER than (or equal) your Role");
                }
            }

            List<ChannelRolePermission> existedChannelRolePermissions = new List<ChannelRolePermission>();
            existedChannelRolePermissions = await _unitOfWork.ChannelRolePermissions.GetChannelRolePermissionsByRoleIdAndChannelId(roleId, channelId);
            foreach (var updateChannelRolePermission in updateRoleDTOs)
            {
                if (existedChannelRolePermissions.Contains(await _unitOfWork.ChannelRolePermissions.GetByChannelRolePermissionAsync(channelId, roleId, updateChannelRolePermission.PermissionId)))
                {
                    foreach (var existedChannelRolePermission in existedChannelRolePermissions)
                    {
                        if (updateChannelRolePermission.PermissionId.Equals(existedChannelRolePermission.PermissionId))
                        {
                            existedChannelRolePermission.IsGranted = updateChannelRolePermission.IsGranted;
                        }
                    }
                }
                else
                {
                    return 0;
                }
            }

            await _unitOfWork.ChannelRolePermissions.UpdateRangeAsync(existedChannelRolePermissions);


            // realtime
            foreach (var update in updateRoleDTOs)
            {
                await _hubContext.Clients.Group(server.Id.ToString())
                    .OnUpdatePermission(server.Id.ToString(), roleId.ToString(), update.PermissionId.ToString());

                string SignalRoleId = roleId.ToString();

                await _hubContext.Clients.Group(SignalRoleId)
                    .UpdateChannelListOnChangePermission(SignalRoleId, currentUserId.ToString());
            }



            // update redis permission for all user contains role
            var serverMembersRole = await _unitOfWork.ServerMembers.GetAllByRoleIdAsync(roleId);
            if (serverMembersRole != null)
            {
                foreach (var member in serverMembersRole)
                {
                    await SetUserChannelPermission(member.UserId, channelId);
                }
            }

            return 1;
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


        public async Task AddRoleToChannel(Guid channelId, Guid roleId, Guid currentUserId)
        {

            var channel = await _unitOfWork.Channels.GetByIdAsync(channelId);
            if (channel == null)
            {
                throw new InvalidDataException("Channel is not found");
            }

            var rolesInChannel = await GetRolesInChannel(channelId);
            if (rolesInChannel == null)
            {
                throw new InvalidDataException("No roles found in this channel");
            }

            Role role = null;
            foreach (var r in rolesInChannel)
            {
                if (r.Id.Equals(roleId))
                {
                    role = r;
                }
            }
            if (role != null)
            {
                return;
            }
            role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null)
            {
                throw new InvalidDataException("Role is not found");
            }
            if (!role.ServerId.Equals(channel.ServerId))
            {
                throw new InvalidOperationException("The server of this channel does not have this role");
            }

            var server = await _unitOfWork.Servers.GetServerOnlyAsync(channel.ServerId);
            if (!currentUserId.Equals(server.OwnerId))
            {

                var hasManageChannelPermission = await HasManageRolePermissionAsync(server.Id, currentUserId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You do not have permission to continue this session.");
                    }
                }
            }

            await AssignDefaultEveryonePermissionsChannel(channelId, roleId);


            // update redis permission for all user contains role
            var serverMembersRole = await _unitOfWork.ServerMembers.GetAllByRoleIdAsync(roleId);
            if (serverMembersRole != null)
            {
                foreach (var member in serverMembersRole)
                {
                    await SetUserChannelPermission(member.UserId, channelId);
                }
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


        public async Task RemoveRoleFromChannel(Guid channelId, Guid roleId, Guid currentUserId)
        {

            var channel = await _unitOfWork.Channels.GetSimpleChannelAsync(channelId);
            if (channel == null)
            {
                throw new InvalidDataException("Channel is not found");
            }
            var rolesInChannel = await GetRolesInChannel(channelId);
            if (rolesInChannel == null)
            {
                throw new InvalidDataException("No roles found in this channel");
            }

            Role role = null;
            foreach (var r in rolesInChannel)
            {
                if (r.Id.Equals(roleId))
                {
                    role = r;
                }
            }
            if (role == null)
            {
                throw new InvalidDataException("Role is not found in this channel");
            }

            if (role.Name.Equals("@everyone"))
            {
                throw new InvalidOperationException("Cannot remove role @everyone from channel");
            }
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(channel.ServerId);
            if (!currentUserId.Equals(server.OwnerId))
            {

                var hasManageChannelPermission = await HasManageRolePermissionAsync(server.Id, currentUserId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You do not have permission to continue this session.");
                    }
                }
            }
            var channelRolePermissions = await _unitOfWork.ChannelRolePermissions.GetChannelRolePermissionsByRoleIdAndChannelId(roleId, channelId);
            if (channelRolePermissions != null)
            {
                foreach (var crp in channelRolePermissions)
                {
                    await _unitOfWork.ChannelRolePermissions.DeleteAsync(crp);
                }
            }


            // update redis permission for all user contains role
            var serverMembersRole = await _unitOfWork.ServerMembers.GetAllByRoleIdAsync(roleId);
            if (serverMembersRole != null)
            {
                foreach (var member in serverMembersRole)
                {

                    await _redisDatabase.KeyDeleteAsync($"userPermission:{member.UserId}:{channel.ServerId}:channel:{channelId}");

                }
            }
        }


        public async Task AssignRolePermissionsChannel(Guid channelId, Guid roleId)
        {
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            var channelPermissions = permissions.Where(p => !p.IsServer).ToList();
            var channel = await _unitOfWork.Channels.GetByIdAsync(channelId)
                ?? throw new InvalidDataException("Channel not found");
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId)
                ?? throw new InvalidDataException("Role not found for assigning channel permissions");

            if (!role.ServerId.Equals(channel.ServerId))
            {
                throw new InvalidOperationException("The server of this channel does not have this role");
            }

            var defaultGrantedPermissions = new HashSet<string>
    {
        PermissionEnum.SEND_MESSAGES_CHANNEL.ToString(),
        PermissionEnum.ATTACH_FILES_CHANNEL.ToString(),
        PermissionEnum.MENTION_ALL.ToString(),
        PermissionEnum.SEND_VOICE_MESSAGES_CHANNEL.ToString(),
        PermissionEnum.VIEW_CHANNEL.ToString()
    };

            foreach (var permission in channelPermissions)
            {
                bool isGranted = defaultGrantedPermissions.Contains(permission.Code);


                var rolePermission = new ChannelRolePermission
                {
                    RoleId = roleId,
                    Role = role,
                    ChannelId = channelId,
                    Channel = channel,
                    PermissionId = permission.Id,
                    IsGranted = isGranted,
                    Permission = permission
                };

                await _unitOfWork.ChannelRolePermissions.CreateAsync(rolePermission);
            }

            // update redis permission for all user contains role
            var serverMembersRole = await _unitOfWork.ServerMembers.GetAllByRoleIdAsync(roleId);
            if (serverMembersRole != null)
            {
                foreach (var member in serverMembersRole)
                {
                    await SetUserChannelPermission(member.UserId, channelId);
                }
            }
        }

        public async Task<List<RoleCreateResponse>> GetAllByMemberIdAsync(Guid memberId)
        {
            var member = await _unitOfWork.ServerMembers.GetByIdAsync(memberId);
            if (member == null)
            {
                throw new InvalidDataException("Server Member is not found");
            }
            var memberRoles = await _unitOfWork.MemberRoles.GetAllByMemberId(memberId);
            if (memberRoles == null)
            {
                throw new InvalidDataException("Server Member is not found");
            }
            List<Role> roles = new List<Role>();
            foreach (var item in memberRoles)
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(item.RoleId);
                roles.Add(role);
            }

            return MapToResponseList(roles);
        }

        public async Task<string> AssignRoleToMemberWithoutChecking(Guid serverMemberId, Guid RoleId)
        {
            var targetServerMember = await _unitOfWork.ServerMembers.GetByIdAsync(serverMemberId);
            if (targetServerMember == null)
            {
                throw new InvalidOperationException("ServerMember not found!");
            }


            if (targetServerMember.MemberRoles.Any(mr => mr.RoleId == RoleId))
            {
                throw new InvalidOperationException("Role has been assigned to this member already");
            }

            var existedMemberRole = await _unitOfWork.MemberRoles.GetByMemberIdAndRoleAsync(serverMemberId, RoleId);
            if (existedMemberRole != null)
            {
                return "The target member already has this role no changes made";
            }

            var server = await _unitOfWork.Servers.GetByIdAsync(targetServerMember.ServerId);
            var role = await _unitOfWork.Roles.GetByIdAsync(RoleId);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found!");
            }

            MemberRole memberRole = new MemberRole
            {
                ServerMemberId = serverMemberId,
                RoleId = RoleId
            };

            MemberRole createdMemberRole = await _unitOfWork.MemberRoles.CreateAsync(memberRole);


            string serverId = role.ServerId.ToString().ToUpper();
            await _hubContext.Clients.Group(serverId)
            .OnUserAssignedToRole(serverId, targetServerMember.Nickname.ToString(), role.Name.ToString());
            return $"Assigned role {role.Name} to {targetServerMember.Nickname} successfully";
        }
    }
}
