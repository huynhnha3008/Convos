using BusinessObjects.DTOs;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;


namespace Services.impl
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ConvosDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ConvosDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            if (typeof(T) == typeof(Server))
            {
                return await _dbSet
                    .Include("ServerMembers")
                    .Include("Roles.MemberRoles")
                    .Include("Channels")
                    .Include(s => ((Server)(object)s).Categories)
                        .ThenInclude(c => c.Channels)
                    .ToListAsync();
            }
            else if (typeof(T) == typeof(Channel))
            {
                return await _dbSet
                    .Include("ChannelRolePermissions")
                    .ToListAsync();
            }
            else if (typeof(T) == typeof(Role))
            {
                var roles = await _dbSet
                    .Include("Servers")
                    .ToListAsync();

                return MapToResponseList(roles.Cast<Role>().ToList()) as IEnumerable<T>;
            }
            else if (typeof(T) == typeof(User))
            {
                return await _context.Users
                    .Include(u => u.ServerMembers)
                        .ThenInclude(sm => sm.Server)
                    .ToListAsync() as IEnumerable<T>;
            }
            else
            {
                return await _dbSet.ToListAsync();
            }
        }


        public async Task<T> GetByIdAsync(Guid id)
        {
            if (typeof(T) == typeof(Category))
            {
                return await _dbSet
                    .Include("Channels")
                    .SingleOrDefaultAsync(c => ((Category)(object)c).Id == id) as T;
            }
            else if (typeof(T) == typeof(Channel))
            {
                return await _dbSet
                    .Include("ChannelRolePermissions.Permission")
                    .SingleOrDefaultAsync(ch => ((Channel)(object)ch).Id == id) as T;
            }
            else if (typeof(T) == typeof(Invite))
            {
                return await _dbSet
                    .Include("ServerMember")
                    .SingleOrDefaultAsync(ch => ((Invite)(object)ch).Id == id) as T;
            }
            else if (typeof(T) == typeof(Server))
            {
                return await _dbSet
                    .Include("ServerMembers.User")
                    .Include("Roles.MemberRoles.ServerMember")
                    .Include("Categories.Channels")
                    .Include("Channels")
                    .Include("Emojis")
                    .SingleOrDefaultAsync(s => ((Server)(object)s).Id == id) as T;
            }
            else if (typeof(T) == typeof(Role))
            {
                var role = await _dbSet
                    .Include("MemberRoles.ServerMember")
                    .Include("RolePermissions.Permission")
                    .Include("ChannelRolePermissions.Permission")
                    .Include("Server")
                    .SingleOrDefaultAsync(r => ((Role)(object)r).Id.Equals(id));

                if (role != null)
                {
                    return role as T;
                }

                return null;
            }

            else if (typeof(T) == typeof(Emoji))
            {
                return await _dbSet
                    .Include("Server.ServerMembers")
                    .SingleOrDefaultAsync(e => ((Emoji)(object)e).Id.Equals(id)) as T;
            }
            else if (typeof(T) == typeof(SoundBoard))
            {
                return await _dbSet
                    .Include("Server.ServerMembers")
                    .SingleOrDefaultAsync(e => ((SoundBoard)(object)e).Id.Equals(id)) as T;
            }
            else if (typeof(T) == typeof(ServerMember))
            {
                return await _dbSet
                    .Include("User")
                    .Include("Server")
                    .Include("MemberRoles.Role.ChannelRolePermissions")
                    .SingleOrDefaultAsync(ch => ((ServerMember)(object)ch).Id.Equals(id)) as T;
            }
            else if (typeof(T) == typeof(User))
            {
                return await _dbSet
                    .Include(u => ((User)(object)u).RequestedFriendships)
                    .Include(u => ((User)(object)u).ReceivedFriendships)
                    .Include(u => ((User)(object)u).ServerMembers)
                        .ThenInclude(sm => sm.MemberRoles)
                    .Include(u => ((User)(object)u).ServerMembers)
                        .ThenInclude(sm => sm.Invites)
                    .Include(u => ((User)(object)u).ServerMembers)
                        .ThenInclude(sm => sm.InvitesUsages)
                    .FirstOrDefaultAsync(u => ((User)(object)u).Id == id) as T;

            }
            else
            {
                return await _dbSet.FindAsync(id) as T;
            }
        }



        public async Task<T> CreateAsync(T entity)
        {
            _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return (entity);
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return (entity);
        }


        public async Task<T> DeleteAsync(T entity)
        {

            if (typeof(T) == typeof(Role))
            {
                var role = entity as Role;
                if (role != null)
                {

                    var relatedPermissions = await _context.ChannelRolePermissions
                        .Where(crp => crp.RoleId == role.Id)
                        .ToListAsync();
                    _context.ChannelRolePermissions.RemoveRange(relatedPermissions);

                    var relatedRolePermissions = await _context.RolePermissions
                      .Where(crp => crp.RoleId == role.Id)
                      .ToListAsync();
                    _context.RolePermissions.RemoveRange(relatedRolePermissions);

                    var relatedMemberRoles = await _context.MemberRoles
                      .Where(crp => crp.RoleId == role.Id)
                      .ToListAsync();
                    _context.MemberRoles.RemoveRange(relatedMemberRoles);

                    var rolesInServer = await _context.Roles
                        .Where(r => r.ServerId == role.ServerId && r.Position > role.Position)
                        .ToListAsync();

                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();

                    foreach (var r in rolesInServer)
                    {
                        r.Position -= 1;
                    }
                    await _context.SaveChangesAsync();
                }
            }

            else if (typeof(T) == typeof(Server))
            {
                var server = entity as Server;
                if (server != null)
                {

                    var relatedRoles = await _context.Roles
                       .Where(r => r.ServerId.Equals(server.Id))
                       .ToListAsync();
                    foreach (var r in relatedRoles)
                    {
                        var relatedMemberRoles = await _context.MemberRoles
                            .Where(mr => mr.RoleId.Equals(r.Id))
                            .ToListAsync();
                        _context.MemberRoles.RemoveRange(relatedMemberRoles);

                        var relatedPermissions = await _context.ChannelRolePermissions
                        .Where(crp => crp.RoleId == r.Id)
                        .ToListAsync();
                        _context.ChannelRolePermissions.RemoveRange(relatedPermissions);

                        var relatedRolePermissions = await _context.RolePermissions
                          .Where(crp => crp.RoleId == r.Id)
                          .ToListAsync();
                        _context.RolePermissions.RemoveRange(relatedRolePermissions);
                    }

                    _context.Roles.RemoveRange(relatedRoles);


                    var relatedMember = await _context.ServerMembers
                        .Where(sm => sm.ServerId.Equals(server.Id))
                        .ToListAsync();

                    foreach(var member in relatedMember)
                    {
                        var relatedInviteU = await _context.InviteUsages
                            .Where(iu => iu.ServerMemberId.Equals(member.Id))
                            .ToListAsync();
                        _context.InviteUsages.RemoveRange(relatedInviteU);

                        var relatedInvite = await _context.Invites
                            .Where(i => i.CreatorId.Equals(member.Id))
                            .ToListAsync();
                        _context.Invites.RemoveRange(relatedInvite);
                    }
                    _context.RemoveRange(relatedMember);
                    var relatedChannel = await _context.Channels
                        .Where(c => c.ServerId.Equals(server.Id))
                        .ToListAsync();
                    _context.RemoveRange(relatedChannel);

                    // emoji
                    var relatedEmoji = await _context.Emojis
                        .Where(c => c.ServerId.Equals(server.Id))
                        .ToListAsync();
                    _context.RemoveRange(relatedEmoji);

                    // soundboard
                    var relatedSoundBoard = await _context.SoundBoards
                     .Where(c => c.ServerId.Equals(server.Id))
                     .ToListAsync();
                    _context.RemoveRange(relatedSoundBoard);


                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();


                }
            }

            else if (typeof(T) == typeof(User))
            {
                var user = entity as User;
                if (user != null)
                {
                    var relatedFship = await _context.Friendships
                        .Where(fs => fs.AddresseeId.Equals(user.Id) || fs.RequesterId.Equals(user.Id))
                        .ToListAsync();
                    _context.RemoveRange(relatedFship);

                    var relatedMembers = await _context.ServerMembers
                        .Where(sm => sm.UserId.Equals(user.Id))
                        .ToListAsync();

                    foreach( var member in relatedMembers)
                    {
                        var relatedCreatedServers = await _context.Servers
                            .Where(s => s.OwnerId.Equals(user.Id))
                            .ToListAsync();

                        foreach(var server in relatedCreatedServers)
                        {
                            var relatedRoles = await _context.Roles
                      .Where(r => r.ServerId.Equals(server.Id))
                      .ToListAsync();
                            foreach (var r in relatedRoles)
                            {
                                var relatedMemberRoles = await _context.MemberRoles
                                    .Where(mr => mr.RoleId.Equals(r.Id))
                                    .ToListAsync();
                                _context.MemberRoles.RemoveRange(relatedMemberRoles);

                                var relatedPermissions = await _context.ChannelRolePermissions
                                .Where(crp => crp.RoleId == r.Id)
                                .ToListAsync();
                                _context.ChannelRolePermissions.RemoveRange(relatedPermissions);

                                var relatedRolePermissions = await _context.RolePermissions
                                  .Where(crp => crp.RoleId == r.Id)
                                  .ToListAsync();
                                _context.RolePermissions.RemoveRange(relatedRolePermissions);
                            }

                            _context.Roles.RemoveRange(relatedRoles);


                            var relatedMember = await _context.ServerMembers
                                .Where(sm => sm.ServerId.Equals(server.Id))
                                .ToListAsync();

                            foreach (var m in relatedMember)
                            {
                                var relatedInviteU = await _context.InviteUsages
                                    .Where(iu => iu.ServerMemberId.Equals(m.Id))
                                    .ToListAsync();
                                _context.InviteUsages.RemoveRange(relatedInviteU);

                                var relatedInvite = await _context.Invites
                                    .Where(i => i.CreatorId.Equals(m.Id))
                                    .ToListAsync();
                                _context.Invites.RemoveRange(relatedInvite);
                            }
                            _context.RemoveRange(relatedMember);
                            var relatedChannel = await _context.Channels
                                .Where(c => c.ServerId.Equals(server.Id))
                                .ToListAsync();
                            _context.RemoveRange(relatedChannel);

                            // emoji
                            var relatedEmoji = await _context.Emojis
                                .Where(c => c.ServerId.Equals(server.Id))
                                .ToListAsync();
                            _context.RemoveRange(relatedEmoji);

                            // soundboard
                            var relatedSoundBoard = await _context.SoundBoards
                             .Where(c => c.ServerId.Equals(server.Id))
                             .ToListAsync();
                            _context.RemoveRange(relatedSoundBoard);

                            _context.RemoveRange(server);
                        }

                        var relatedMemRoles = await _context.MemberRoles
                            .Where(mr => mr.ServerMemberId.Equals(member.Id))
                            .ToListAsync();

                        _context.MemberRoles.RemoveRange(relatedMemRoles);

                        var relatedInviteUs = await _context.InviteUsages
                            .Where(iu => iu.ServerMemberId.Equals(member.Id))
                            .ToListAsync();
                        _context.InviteUsages.RemoveRange(relatedInviteUs);

                    }


                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }

            else if (typeof(T) == typeof(Category))
            {
                var cate = entity as Category;
                if (cate != null)
                {

                    var relatedChannels = await _context.Channels
                        .Where(c => c.CategoryId == cate.Id)
                        .ToListAsync();
                    _context.Channels.RemoveRange(relatedChannels);

                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }



            else if (typeof(T) == typeof(Channel))
            {
                var channel = entity as Channel;
                if (channel != null)
                {

                    var relatedPermissions = await _context.ChannelRolePermissions
                        .Where(crp => crp.ChannelId == channel.Id)
                        .ToListAsync();
                    _context.ChannelRolePermissions.RemoveRange(relatedPermissions);

                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }

               else if (typeof(T) == typeof(Invite))
            {
                var invite = entity as Invite;
                if (invite != null)
                {

                   var relatedInviteU = await _context.InviteUsages
                    .Where( iu => iu.InviteId.Equals(invite.Id))
                    .ToListAsync();

                    _context.InviteUsages.RemoveRange(relatedInviteU);

                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }


            else if (typeof(T) == typeof(Permission))
            {
                var permission = entity as Permission;
                if (permission != null)
                {

                    var relatedChannelPermissions = await _context.ChannelRolePermissions
                        .Where(crp => crp.PermissionId == permission.Id)
                        .ToListAsync();
                    _context.ChannelRolePermissions.RemoveRange(relatedChannelPermissions);


                    var relatedRolePermissions = await _context.RolePermissions
                        .Where(rp => rp.PermissionId == permission.Id)
                        .ToListAsync();
                    _context.RolePermissions.RemoveRange(relatedRolePermissions);

                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
            else if (typeof(T) == typeof(ServerMember))
            {
                var member = entity as ServerMember;
                if (member != null)
                {
                    //inviteUsage
                    var relatedInviteU = await _context.InviteUsages
                     .Where(iu => iu.ServerMemberId.Equals(member.Id))
                     .ToListAsync();
                    _context.InviteUsages.RemoveRange(relatedInviteU);


                    //invite
                    var relatedInvite =  await  _context.Invites
                        .Where(i => i.CreatorId.Equals(member.Id))
                        .ToListAsync();
                    _context.Invites.RemoveRange(relatedInvite);

                    //memberrole
                    var relatedMemberRole = await _context.MemberRoles
                        .Where(mr => mr.ServerMemberId.Equals(member.Id))
                        .ToListAsync();
                    _context.MemberRoles.RemoveRange(relatedMemberRole);

                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }


            else
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }

            return entity;
        }




        public async Task<IEnumerable<T>> SearchAsync(string name)
        {
            if (typeof(T) == typeof(Role))
            {
                var roles = _dbSet.OfType<Role>();

                if (!string.IsNullOrEmpty(name))
                {
                    return await roles.Where(s => s.Name.Contains(name)).ToListAsync() as IEnumerable<T>;
                }

                return await roles.ToListAsync() as IEnumerable<T>;
            }
            if (typeof(T) == typeof(Category))
            {
                var cates = _dbSet.OfType<Category>();

                if (!string.IsNullOrEmpty(name))
                {
                    return await cates.Include(s => s.Channels)
                        .Where(s => s.Name.Contains(name)).ToListAsync() as IEnumerable<T>;
                }

                return await cates.ToListAsync() as IEnumerable<T>;
            }

            if (typeof(T) == typeof(Channel))
            {
                var cates = _dbSet.OfType<Channel>();

                if (!string.IsNullOrEmpty(name))
                {
                    return await cates.Include(s => s.ChannelRolePermissions)
                        .Where(s => s.Name.Contains(name)).ToListAsync() as IEnumerable<T>;
                }

                return await cates.ToListAsync() as IEnumerable<T>;
            }

            if (typeof(T) == typeof(Emoji))
            {
                var emo = _dbSet.OfType<Emoji>();

                if (!string.IsNullOrEmpty(name))
                {
                    return await emo.Include(e => e.Server)
                        .Where(s => s.Name.Contains(name)).ToListAsync() as IEnumerable<T>;
                }

                return await emo.ToListAsync() as IEnumerable<T>;
            }
            if (typeof(T) == typeof(SoundBoard))
            {
                var sound = _dbSet.OfType<SoundBoard>();

                if (!string.IsNullOrEmpty(name))
                {
                    return await sound.Include(e => e.Server)
                        .Where(s => s.Name.Contains(name)).ToListAsync() as IEnumerable<T>;
                }

                return await sound.ToListAsync() as IEnumerable<T>;
            }
            if (typeof(T) == typeof(MemberRole))
            {
                var memRole = _dbSet.OfType<MemberRole>();

                if (!string.IsNullOrEmpty(name))
                {
                    return await memRole.Where(s => s.ServerMember.Nickname.Contains(name)).ToListAsync() as IEnumerable<T>;
                }

                return await memRole.ToListAsync() as IEnumerable<T>;
            }
            if (typeof(T) == typeof(Permission))
            {
                var permissions = _dbSet.OfType<Permission>();

                if (!string.IsNullOrEmpty(name))
                {
                    return await permissions.Where(s => s.Name.Contains(name) ||
                                                        s.Description.Contains(name) ||
                                                        s.Code.Contains(name))
                                            .ToListAsync() as IEnumerable<T>;
                }

                return await permissions.ToListAsync() as IEnumerable<T>;
            }
            if (typeof(T) == typeof(ServerMember))
            {
                var member = _dbSet.OfType<ServerMember>();

                if (!string.IsNullOrEmpty(name))
                {
                    return await member.Where(s => s.Nickname.Contains(name)).ToListAsync() as IEnumerable<T>;
                }

                return await member.ToListAsync() as IEnumerable<T>;
            }

            if (typeof(T) == typeof(Server))
            {
                var serverQuery = _dbSet.OfType<Server>();

                if (!string.IsNullOrEmpty(name))
                {
                    serverQuery = serverQuery.Where(s => s.Name.Contains(name));
                }

                return await serverQuery
                    .Include("ServerMembers")
                    
                    .Include("Channels")
                    .Include(s => ((Server)(object)s).Categories)
                        .ThenInclude(c => c.Channels)
                    .ToListAsync() as IEnumerable<T>;
            }
            if (typeof(T) == typeof(Invite))
            {
                var invites = _dbSet.OfType<Invite>();

                if (!string.IsNullOrEmpty(name))
                {
                    return await invites.Where(i => i.Code.Contains(name)).ToListAsync() as IEnumerable<T>;
                }

                return await invites.ToListAsync() as IEnumerable<T>;
            }

            if (typeof(T) == typeof(Event))
            {
                var events = _dbSet.OfType<Event>();

                if (!string.IsNullOrEmpty(name))
                {
                    return await events.Where(s => s.Title.Contains(name) ||
                                                        s.Description.Contains(name) ).ToListAsync() as IEnumerable<T>;
                }

                return await events.ToListAsync() as IEnumerable<T>;
            }



            return Enumerable.Empty<T>();
        }



        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
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

        private List<RoleCreateResponse> MapToResponseList(List<Role> roles)
        {
            var responseList = new List<RoleCreateResponse>();
            foreach (var role in roles)
            {
                responseList.Add(MapToResponse(role));
            }
            return responseList;
        }


    }
}
