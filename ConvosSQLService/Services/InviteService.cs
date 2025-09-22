using BusinessObjects.DTOs;
using BusinessObjects.Models;
using BusinessObjects.QueryObject;
using Microsoft.AspNetCore.SignalR;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.SignalR;
using Services.SignalR.Interfaces;
using System.Security.Cryptography;


namespace Services
{
    public class InviteService : IInviteService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ServerHub, IServerHub> _serverHub;
        private readonly IPermissionService _permissionService;

        public InviteService(IUnitOfWork unitOfWork, IHubContext<ServerHub, IServerHub> hubContext, IPermissionService permissionService)
        {
            _unitOfWork = unitOfWork;
            _serverHub = hubContext;
            _permissionService = permissionService;
        }
        private async Task<bool> HasCreateInvitePermissionAsync(Guid serverId, Guid userId)
        {
            var userPermission = await _permissionService.GetUserGlobalPermission(userId, serverId);
            if (userPermission == null )
            {
                return false;
            }
            var hasCreateInvitePermission = userPermission
                .Any(p => p.Code.Equals(PermissionEnum.CREATE_INVITE.ToString()));

            if (!hasCreateInvitePermission)
            {
                return false;
            }
            return true;
        }

        private InviteDetailResponse ToInviteDetailResponse(Invite invite)
        {
            InviteDetailResponse inv = new InviteDetailResponse
            {
                Id = invite.Id,
                Code = invite.Code,
                CreatedAt = invite.CreatedAt,
                CreatorId = invite.CreatorId,
                ExpiryDate = invite.ExpiryDate,
                MaxUses = invite.MaxUses,
                ServerId = invite.ServerId,
                UpdatedAt = invite.UpdatedAt,
                Uses = invite.Uses,
                ServerMember = new MemberInviteResponse
                {
                    
                    Banned = invite.ServerMember.Banned,
                    Deafened = invite.ServerMember.Deafened,
                    JoinedAt = invite.ServerMember.JoinedAt,
                    MemberId = invite.ServerMember.UserId,
                    Muted = invite.ServerMember.Muted,
                    Nickname = invite.ServerMember.Nickname
                }
            };
            return inv;
        }
        public async Task<InviteDetailResponse> CreateAsync(InviteCreateRequest inviteCreateRequest, Guid userId)
        {
            var server = await _unitOfWork.Servers.GetServerIncludeMembersAsync(inviteCreateRequest.ServerId);
            if(server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            var member =  server.ServerMembers.SingleOrDefault(sm => sm.UserId.Equals(userId));
            if(member == null)
            {
                throw new InvalidOperationException("You are NOT belong to this server");
            }

            // chekc permission
            if (!userId.Equals(server.OwnerId))
            {
                var hasManageChannelPermission = await HasCreateInvitePermissionAsync(server.Id, userId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't have permission to CREATE invite in this server");
                    }
                }
            }

            Invite invite = new Invite
            {
                Code = GenerateInviteCode(),
                CreatedAt = DateTime.UtcNow,
                CreatorId = member.Id,
                ExpiryDate = DateTime.Now.AddDays(7),
                MaxUses = 7,
                ServerId = server.Id,
                Uses = 0,
                Status = true
            };

            var createdInvite =  await _unitOfWork.Invites.CreateAsync(invite);
            await _serverHub.Clients.Group(invite.ServerId.ToString()).CreateInvite(invite.ServerId, invite.Code);
            return ToInviteDetailResponse(createdInvite);

        }

        private string GenerateInviteCode()
        {
            const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var randomBytes = new byte[8];

            RandomNumberGenerator.Fill(randomBytes); // Thay thế RNGCryptoServiceProvider

            var inviteCode = new char[8];
            for (int i = 0; i < 8; i++)
            {
                inviteCode[i] = Base62Chars[randomBytes[i] % Base62Chars.Length];
            }

            return new string(inviteCode);
        }


        public async Task<string> UpdateAsync(Guid id, InviteUpdateRequest request)
        {
            if (request.MaxUses < 0) {
                throw new InvalidOperationException("Input max uses must be equal or large than 0");
            }
            var invite = await _unitOfWork.Invites.GetByIdAsync(id);
            if (invite == null)
            {
                throw new InvalidDataException("Invite is not found");
            }

            if (invite.Status == false)
            {
                throw new InvalidDataException("Invite is not found");
            }
            invite.MaxUses = request.MaxUses;
            invite.UpdatedAt = DateTime.UtcNow;
            invite.ExpiryDate = DateTime.Now.AddDays(7);
            var updatedInvite = await _unitOfWork.Invites.UpdateAsync(invite);
            return "Update successfully";

        }

        public async Task<List<InviteDetailResponse>> GetServerInvitesAsync(Guid serverId, QueryInvite query)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if(server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            // server is invites
            var serverInvites = await _unitOfWork.Invites.GetAllAsync(serverId);
            if (serverInvites == null)
            {
                return new List<InviteDetailResponse>();
            }

            // result query invites

            var availableServerInvites = serverInvites.Where(i => i.Status == true).ToList();
            // invites search contain code
            var searchInvites = await _unitOfWork.Invites.SearchAsync(query.Code);

            //server's invites that contain code
            var invites = availableServerInvites.Where(invite => searchInvites.Any(i => i.Id == invite.Id)).ToList();


            List<InviteDetailResponse> result = new List<InviteDetailResponse>();
            switch (query.SortBy.ToString())
            {
                case "Code":
                    invites = query.IsDescending ? invites.OrderByDescending(i => i.Code).ToList() : invites.OrderBy(i => i.Code).ToList();
                    break;
                case "CreateAt":
                    invites = query.IsDescending ? invites.OrderByDescending(i => i.CreatedAt).ToList() : invites.OrderBy(i => i.CreatedAt).ToList();
                    break;
                case "MaxUses":
                    invites = query.IsDescending ? invites.OrderByDescending(i => i.MaxUses).ToList() : invites.OrderBy(i => i.MaxUses).ToList();
                    break;
                case "ExpiryDate":
                    invites = query.IsDescending ? invites.OrderByDescending(i => i.ExpiryDate).ToList() : invites.OrderBy(i => i.ExpiryDate).ToList();
                    break;
            }

            var paginatedInvites = invites
           .Skip((query.PageNumber - 1) * query.PageSize)
           .Take(query.PageSize)
           .ToList();

            var rs = new List<InviteDetailResponse>();
            foreach (var inv in paginatedInvites)
            {
                rs.Add(ToInviteDetailResponse(inv));
            }
            return rs;
        }


        public async Task<InviteDetailResponse> GetByIdAsync(Guid id)
        {
            var invite = await _unitOfWork.Invites.GetByIdAsync(id);
            if(invite == null)
            {
                throw new InvalidDataException("Invite is not found");
            }
            if (invite.Status == false)
            {
                throw new InvalidDataException("Invite is not found");
            }
            return ToInviteDetailResponse(invite);
        }

        public async Task<string> DeleteAsync(Guid id, Guid userId)
        {
            var invite = await _unitOfWork.Invites.GetByIdAsync(id);
            if(invite == null)
            {
                throw new InvalidDataException("Invite is not found");
            }
            if (invite.Status == false)
            {
                throw new InvalidDataException("Invite is not found");
            }
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(invite.ServerId);
            // chekc permission
            if (!userId.Equals(server.OwnerId))
            {
                var hasManageInvitePermission = await HasCreateInvitePermissionAsync(server.Id, userId);
                {
                    if (!hasManageInvitePermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You  don't have permission to DELETE invite in this server");
                    }
                }
            }

            invite.Status = false;
            await _unitOfWork.Invites.UpdateAsync(invite);
            await _serverHub.Clients.Group(invite.ServerId.ToString()).DeleteInvite(invite.ServerId, invite.Code);
            return "Delete successfully";
        }

        public async Task<List<InviteDetailResponse>> SearchAsync(Guid serverId, QueryInvite query)
        {
            if (query.Code == null)
            {
                query.Code = "";
            }
            var server = await _unitOfWork.Servers.GetByIdAsync(serverId);
            if(server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            // server's invites
            var serverInvites = await _unitOfWork.Invites.GetAllAsync(serverId);
            
            var availableServerInvites = serverInvites.Where(i => i.Status == true).ToList();
            // invites search contain code
            var searchInvites = await _unitOfWork.Invites.SearchAsync(query.Code);

            //server's invites that contain code
            var invites = availableServerInvites.Where( invite => searchInvites.Any(i => i.Id == invite.Id)).ToList();

            switch (query.SortBy.ToString())
            {
                case "Code":
                    invites = query.IsDescending ? invites.OrderByDescending(i => i.Code).ToList() : invites.OrderBy(i => i.Code).ToList();
                    break;
                case "CreateAt":
                    invites = query.IsDescending ? invites.OrderByDescending(i => i.CreatedAt).ToList() : invites.OrderBy(i => i.CreatedAt).ToList();
                    break;
                case "MaxUses":
                    invites = query.IsDescending ? invites.OrderByDescending(i => i.MaxUses).ToList() : invites.OrderBy(i => i.MaxUses).ToList();
                    break;
                case "ExpiryDate":
                    invites = query.IsDescending ? invites.OrderByDescending(i => i.ExpiryDate).ToList() : invites.OrderBy(i => i.ExpiryDate).ToList();
                    break;
            }

            var paginatedInvites = invites
           .Skip((query.PageNumber - 1) * query.PageSize)
           .Take(query.PageSize)
           .ToList();

            var rs = new List<InviteDetailResponse>();
            foreach (var inv in paginatedInvites)
            {
                rs.Add(ToInviteDetailResponse(inv));
            }
            return rs;
        }
    }
}

