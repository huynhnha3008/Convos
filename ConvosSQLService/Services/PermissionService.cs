using BusinessObjects.DTOs;
using BusinessObjects.Models;
using BusinessObjects.QueryObject;
using Newtonsoft.Json;
using Repositories.Interfaces;
using Services.Interfaces;
using System.Xml.Schema;


namespace Services
{
    public class PermissionService : IPermissionService
    {
        private readonly StackExchange.Redis.IDatabase _redisDatabase;
        private readonly IUnitOfWork _unitOfWork;

        public PermissionService(StackExchange.Redis.IConnectionMultiplexer redisDb, IUnitOfWork unitOfWork)
        {
            _redisDatabase = redisDb.GetDatabase();
            _unitOfWork = unitOfWork;
        }

        public async Task<PermissionCreateResponse> DeleteAsync(Guid id)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
            if (permission == null)
            {
                throw new InvalidDataException("Process deleting failed: Permission is not found");
            }
            //realtime
            var deletePermission = await _unitOfWork.Permissions.DeleteAsync(permission);

            return ToPermissionRs(deletePermission);
        }

        private PermissionCreateResponse ToPermissionRs (Permission permission)
        {
            PermissionCreateResponse rs = new PermissionCreateResponse
            {
                Id = permission.Id,
                Code = permission.Code,
                Description = permission.Description,
                IsServer = permission.IsServer,
                Name = permission.Name,
            };
            return rs;
        }
        public async Task<List<PermissionCreateResponse>> GetAllAsync(QueryPermission query)
        {

            var list = await _unitOfWork.Permissions.SearchAsync(query.SearchTerm);
            if (list == null || !list.Any())
            {
                return new List<PermissionCreateResponse>();
            }
            var permissions = list.Where(p => p.IsServer).ToList();

            if (!query.IsServer)
            {
                permissions = list.Where(p => !p.IsServer).ToList();
            }

            switch (query.SortBy.ToString())
            {
                case "Name":
                    permissions = query.IsDescending ? permissions.OrderByDescending(c => c.Name).ToList() : permissions.OrderBy(c => c.Name).ToList();
                    break;
                case "Description":
                    permissions = query.IsDescending ? permissions.OrderByDescending(c => c.Description).ToList() : permissions.OrderBy(c => c.Description).ToList();
                    break;
                case "Code":
                    permissions = query.IsDescending ? permissions.OrderByDescending(c => c.Code).ToList() : permissions.OrderBy(c => c.Code).ToList();
                    break;
                default:
                    permissions = permissions.OrderBy(s => s.Name).ToList();
                    break;
            }

            var paginatedpermissions = permissions
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            var rs = new List<PermissionCreateResponse>();
            foreach (var permission in paginatedpermissions)
            {
                rs.Add(ToPermissionRs(permission));
            }
            return rs;
        }

        public async Task<PermissionCreateResponse> GetByIdAsync(Guid id)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync (id);
            if (permission == null)
            {
                throw new InvalidDataException("Permission is not found");
            }
            return ToPermissionRs(permission);
        }

        public async Task<UserPermissionResponse> GetUserPermissions(Guid serverId, Guid userId)
        {
            var redisKey = $"userPermission:{userId}:{serverId}";
            var userPermissionJson = await _redisDatabase.StringGetAsync(redisKey);

            if (!userPermissionJson.HasValue)
            {
                return null;
            }

           return JsonConvert.DeserializeObject<UserPermissionResponse>(userPermissionJson);
        }

        public async Task<List<RolePermissionResponse>> GetUserGlobalPermission(Guid userId, Guid serverId)
        {
            var redisKey = $"userPermission:{userId}:{serverId}:global";
            var permissionJson = await _redisDatabase.StringGetAsync(redisKey);

            if (!permissionJson.HasValue)
            {
                return new List<RolePermissionResponse>();
            }

            return JsonConvert.DeserializeObject<List<RolePermissionResponse>>(permissionJson);
        }

        public async Task<List<RolePermissionResponse>> GetUserChannelPermission(Guid userId, Guid serverId, Guid channelId)
        {
            var redisKey = $"userPermission:{userId}:{serverId}:channel:{channelId}";
            var permissionJson = await _redisDatabase.StringGetAsync(redisKey);

            if (!permissionJson.HasValue)
            {
                return new List<RolePermissionResponse>();
            }

            return JsonConvert.DeserializeObject<List<RolePermissionResponse>>(permissionJson);
        }


        public async Task<List<PermissionCreateResponse>> SearchAsync(QueryPermission query)
        {
            if(query.SearchTerm == null)
            {
                query.SearchTerm = "";
            }
            var list = await _unitOfWork.Permissions.SearchAsync(query.SearchTerm);
            if(list == null || !list.Any())
            {
                return new List<PermissionCreateResponse>();
            }
            var permissions = list.Where(p => p.IsServer).ToList();

            if (!query.IsServer)
            { 
                 permissions = list.Where(p => !p.IsServer).ToList();
            }

            switch (query.SortBy.ToString())
            {
                case "Name":
                    permissions = query.IsDescending ? permissions.OrderByDescending(c => c.Name).ToList() : permissions.OrderBy(c => c.Name).ToList();
                    break;
                case "Description":
                    permissions = query.IsDescending ? permissions.OrderByDescending(c => c.Description).ToList() : permissions.OrderBy(c => c.Description).ToList();
                    break;
                case "Code":
                    permissions = query.IsDescending ? permissions.OrderByDescending(c => c.Code).ToList() : permissions.OrderBy(c => c.Code).ToList();
                    break;
                default:
                    permissions = permissions.OrderBy(s => s.Name).ToList();
                    break;
            }

            var paginatedpermissions = permissions
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            var rs = new List<PermissionCreateResponse>();
            foreach (var permission in paginatedpermissions)
            {
                rs.Add(ToPermissionRs(permission));
            }
            return rs;
        }

        public async Task<PermissionCreateResponse> UpdateAsync(Guid id, PermissionUpdateRequest request)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
            if (permission == null)
            {
                throw new InvalidDataException("Permission not found");
            }
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            var existedNamePermission = permissions.FirstOrDefault(p => p.Name.Equals(request.Name));
            if (existedNamePermission != null && existedNamePermission != permission)
            {
                throw new InvalidOperationException($"A Role with input Name {request.Name} has existed");
            }

            var existedCodePermission = permissions.FirstOrDefault(p => p.Code.Equals(request.Code));
            if (existedCodePermission != null && existedCodePermission != permission)
            {
                throw new InvalidOperationException($"A Role with input Code {request.Code} has existed");
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                permission.Description = request.Description;
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                permission.Name = request.Name;
            }

           

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                permission.Code = request.Code;
            }
          

            var updatedPermission = await _unitOfWork.Permissions.UpdateAsync(permission);

            return ToPermissionRs(updatedPermission);
        }

        public async Task<PermissionCreateResponse> CreateAsync(PermissionCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new ArgumentException("Code is required fields.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Name is required fields.");
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                throw new ArgumentException("Description is required fields.");
            }
            var permissions = await _unitOfWork.Permissions.GetAllAsync();

            var existingPermissionByName = permissions.FirstOrDefault(p => p.Name == request.Name);
            if (existingPermissionByName != null)
            {
                throw new InvalidOperationException("A permission with the same name already exists.");
            }

            var existingPermissionByCode = permissions.FirstOrDefault(p => p.Code == request.Code);
            if (existingPermissionByCode != null)
            {
                throw new InvalidOperationException("A permission with the same code already exists.");
            }

            Permission permission = new Permission
            {
                Code = request.Code,
                Name = request.Name,
                IsServer = request.IsServer,
                Description = request.Description
            };

            var createdPerm = await _unitOfWork.Permissions.CreateAsync(permission);

            // add to rolePermission 
            if(createdPerm.IsServer)
            {
                HashSet<Guid> roleIds = new HashSet<Guid>();
                var rolepermissions = await _unitOfWork.RolePermissions.GetAllAsync();
                foreach (var role in rolepermissions)
                {
                    roleIds.Add(role.RoleId);
                }
                foreach (var roleId in roleIds)
                {
                    RolePermission rs = new RolePermission();
                    rs.PermissionId = permission.Id;
                    rs.RoleId = roleId;
                    rs.IsGranted = false;

                    await _unitOfWork.RolePermissions.CreateAsync(rs);
                }
            } 
            else
            {
                var crpList = await _unitOfWork.ChannelRolePermissions.GetAllAsync();
                HashSet<Guid> channelIds = new HashSet<Guid>();
                foreach (var crp in crpList)
                {
                    channelIds.Add(crp.ChannelId);
                }
                foreach (var channelId in channelIds)
                {
                    var rolesInChannel  = await GetRolesInChannel(channelId);

                    foreach(var role in rolesInChannel)
                    {

                            ChannelRolePermission newChannelRolePermission = new ChannelRolePermission
                            {
                                RoleId = role.Id,
                                ChannelId = channelId,
                                PermissionId = permission.Id,
                                IsGranted = false
                            };
                            await _unitOfWork.ChannelRolePermissions.CreateAsync(newChannelRolePermission);
                    }
                }

            }
           

            return ToPermissionRs(createdPerm);
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

        private async Task<bool> HasManageChannelPermissionAsync(Guid serverId, Guid userId)
        {
            var userPermission = await GetUserPermissions(serverId, userId);
            if (userPermission == null)
            {
                return false;
            }
            var hasManageChannelPermission = userPermission.permissions
                .Any(p => p.Code.Equals(PermissionEnum.MANAGE_CHANNELS.ToString()));

            if (!hasManageChannelPermission)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> CheckServerPermission(Guid serverId, Guid userId, string permissionCode)
        {
            var userPermission = await GetUserPermissions(serverId, userId);
            if (userPermission == null)
            {
                return false;
            }
            var hasPermission = userPermission.permissions
                .Any(p => p.Code.Equals(permissionCode, StringComparison.OrdinalIgnoreCase));

            if (!hasPermission)
            {
                return false;
            }
            return true;
        }


        public async Task<bool> CheckPermission(Guid serverId, Guid userId, string permissionCode, Guid channelId)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                return false;
            }
            if(server.OwnerId.Equals(userId))
            {
                return true;
            }
            var userPermission = await GetUserGlobalPermission(userId, serverId);
            if (userPermission == null)
            {
                return false;
            }

            if (userPermission.Any(p => p.Code.Equals(permissionCode, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            else
            {
                var channelPerm = await GetUserChannelPermission(userId, serverId, channelId);
                if (channelPerm == null)
                {
                    return false;
                }

                if (channelPerm.Any(p => p.Code.Equals(permissionCode, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    return true;
                }
            }
            return false;
        }


    }
}
