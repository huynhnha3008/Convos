using BusinessObjects.DTOs;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using Services.Interfaces;
namespace Services.impl
{
    public class ServerMemberRepository : GenericRepository<ServerMember>, IServerMemberRepository
    {
        private readonly ConvosDbContext _context;
        
        public ServerMemberRepository(ConvosDbContext convosDbContext) : base(convosDbContext) 
        {
            _context = convosDbContext;
        }

        public async Task<ServerMember> BanMemberAsync(ServerMember member)
        {

                member.Banned = true;
                await _context.SaveChangesAsync();
            return member;  
            }

        public async Task<ServerMember> FindByUserIdAndServerIdAsync(ServerMemberCreateRequest serverMemberCreateRequest)
        {
            return await _context.ServerMembers
                                 .Include(sm => sm.Invites)
                                 .Include(sm =>sm.InvitesUsages)
                                 .Include(sm => sm.MemberRoles)
                                     .ThenInclude(mr => mr.Role)
                                     .ThenInclude(r => r.RolePermissions)
                                     .ThenInclude(rm => rm.Permission)
                                 .Include(sm => sm.MemberRoles)
                                     .ThenInclude(mr => mr.Role)
                                     .ThenInclude(r => r.Server)
                                     .ThenInclude(crm => crm.Channels)
                                     .ThenInclude(r => r.ChannelRolePermissions)
                                     .ThenInclude(crm => crm.Permission)                                     
                                 .SingleOrDefaultAsync(sm => sm.ServerId.Equals(serverMemberCreateRequest.ServerId) &&
                                                             sm.UserId.Equals(serverMemberCreateRequest.UserId));
        }

        public async Task<List<ServerMember>> GetAllAsync(Guid serverId)
        {
            return await _context.ServerMembers
                .Where(sm => sm.ServerId.Equals(serverId))
                .ToListAsync();
        }


        public async Task<List<ServerMember>> GetAllByRoleIdAsync(Guid roleId)
        {
            List<MemberRole> memberRoles = await _context.MemberRoles
                .Include(mr => mr.ServerMember)
                    .ThenInclude(sm => sm.Invites)
                .Where(mr => mr.RoleId == roleId) 
                .ToListAsync();

            return memberRoles.Select(mr => mr.ServerMember).ToList();
        }

        public async Task<List<Guid>> GetAllServerIdByUserIdAsync(Guid userId)
        {
            var serverIds= await _context.ServerMembers
                .Where(sm => sm.UserId.Equals(userId))
                .Select(sm => sm.ServerId)
                .ToListAsync();

            return serverIds;
        }

        public async Task<ServerMember> GetByUserIdAndServerIdIncludeRoles(Guid userId, Guid serverId)
        {
            return await _context.ServerMembers
                .Include(sm => sm.MemberRoles)
                    .ThenInclude(mr => mr.Role)
                .SingleOrDefaultAsync(sm => sm.ServerId.Equals(serverId) &&
                                                             sm.UserId.Equals(userId));
        }

        public async Task<ServerMember> GetByUserIdAndServerIdIncludeRolesPermissionsAsync(ServerMemberCreateRequest serverMemberCreateRequest)
        {
            return await _context.ServerMembers
                                .Include(sm => sm.MemberRoles)
                                    .ThenInclude(mr => mr.Role)
                                    .ThenInclude(r => r.RolePermissions)
                                    .ThenInclude(rm => rm.Permission)
                                .Include(sm => sm.MemberRoles)
                                    .ThenInclude(mr => mr.Role)
                                    .ThenInclude(r => r.ChannelRolePermissions)
                                .FirstOrDefaultAsync(sm => sm.ServerId.Equals(serverMemberCreateRequest.ServerId) &&
                                                            sm.UserId.Equals(serverMemberCreateRequest.UserId));
        }

        public async Task<ServerMember> GetMemberIncludeUserAsync(Guid memberId)
        {
            return await _context.ServerMembers
                .Include(sm => sm.User)
                .FirstOrDefaultAsync(sm => sm.Id.Equals(memberId));
        }

        public async Task<ServerMember> GetSimpleByUserIdServerIdAsync(Guid serverId, Guid userId)
        {
            return await _context.ServerMembers
                .FirstOrDefaultAsync(sm => sm.ServerId.Equals(serverId) &&
                                                             sm.UserId.Equals(userId));
        }
    }
}
