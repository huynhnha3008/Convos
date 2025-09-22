using BusinessObjects.DTOs;
using BusinessObjects.Models;
using Microsoft.AspNetCore.SignalR;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.SignalR;
using Services.SignalR.Interfaces;
namespace Services
{
    public class InviteUsageService : IInviteUsageService
    {
 
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ServerHub, IServerHub> _serverHub;
        private readonly IRoleService _roleService;

        public InviteUsageService (IUnitOfWork unitOfWork, IHubContext<ServerHub, IServerHub> serverContext, IRoleService roleService)
        {

            _unitOfWork = unitOfWork;
            _serverHub = serverContext;
            _roleService = roleService;
        }

        public async Task<string> JoinServerByInviteCodeAsync(InviteUsageCreateRequest request, Guid userId)
        {
            var invite = await _unitOfWork.Invites.GetByCodeAsync(request.code);
            if (invite == null)
            {
                throw new InvalidDataException("Invite is not found");
            }

            if(!invite.Status)
            {
                throw new InvalidDataException("Invite is not found");
            }

            if (invite.Uses >= invite.MaxUses)
            {
                throw new InvalidOperationException("Invite usage exceeded the maximum allowed.");
            }
            if(invite.ExpiryDate <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Invite code has expired");
            }
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            var server = await _unitOfWork.Servers.GetServerIncludeRolesMemberAsync(invite.ServerId);
            ServerMember existMember = server.ServerMembers.FirstOrDefault(m => m.UserId.Equals(userId));
            if (existMember != null)
            {
                if(existMember.Banned)
                {
                    throw new InvalidOperationException("You are BANNED from this Server.");
                }
                throw new InvalidOperationException("User is already a member of the server.");
            }
            
            var member = new ServerMember
            {
                UserId = user.Id,
                Banned = false,
                Deafened = false,
                JoinedAt = DateTime.UtcNow,
                Muted = false,
                Nickname = user.DisplayName,
                User = user,
                ServerId = server.Id
            };

            var createdMember = await _unitOfWork.ServerMembers.CreateAsync(member);

            Role serverRoleEveryone = server.Roles.FirstOrDefault(r => r.Name.Equals("@everyone"));
            var existedEveryone = await _unitOfWork.Roles.GetByIdAsync(serverRoleEveryone.Id);
            MemberRole memRole = new MemberRole
            {
                ServerMember = createdMember,
                Role = existedEveryone,
                RoleId = existedEveryone.Id,
                ServerMemberId = createdMember.Id
            };
            existedEveryone.MemberRoles.Add(memRole);
            await _unitOfWork.Roles.UpdateAsync(existedEveryone);
            server.ServerMembers.Add(createdMember);
            await _unitOfWork.Servers.UpdateAsync(server);

            invite.Uses++;
            var inviteUsage = new InviteUsage
            {
                ServerMemberId = createdMember.Id,
                InviteId = invite.Id,
                UsedAt = DateTime.UtcNow,
            };

            await _unitOfWork.InvitesUsages.CreateAsync(inviteUsage); 

            await _serverHub.Clients.Group(invite.ServerId.ToString()).AlertToServer(invite.ServerId, createdMember.Nickname);

            return $"{createdMember.Nickname} has joined server successfully";
        }


    }
}
