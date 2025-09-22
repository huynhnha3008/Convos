using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace BusinessObjects.Models
{
    public class ConvosDbContext : DbContext
    {

        public ConvosDbContext(DbContextOptions<ConvosDbContext> options)
            : base(options)
        { }


        // DbSet properties for all models
        public DbSet<User> Users { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<ServerMember> ServerMembers { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<MemberRole> MemberRoles { get; set; }
        public DbSet<InviteUsage> InviteUsages { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<ChannelRolePermission> ChannelRolePermissions { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Channel> Channels { get; set; }

        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<Emoji> Emojis { get; set; }
        public virtual DbSet<SoundBoard> SoundBoards { get; set; }
        public virtual DbSet<Event> Events { get; set; }

        public virtual DbSet<QuizMember> QuizMembers { get; set; }

        public ConvosDbContext()
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Permission>().HasData(
       //new Permission { Id = Guid.NewGuid(), Name = "View Channels", Code = "VIEW_CHANNELS", Description = "Ability to view channels by default (excluding private channels)", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Manage Channels", Code = "MANAGE_CHANNELS", Description = "Ability to create, delete, or edit channels", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Manage Roles", Code = "MANAGE_ROLES", Description = "Ability to create, edit, and assign roles within the server.", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Manage Emojis", Code = "MANAGE_EMOJIS", Description = "Ability to add or remove emojis available for the server.", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Manage Server", Code = "MANAGE_SERVER", Description = "Ability to change server settings (e.g., name, icon, region), view all invites.", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Create Invite", Code = "CREATE_INVITE", Description = "Ability to invite new people to server", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Change Nickname", Code = "CHANGE_NICKNAME", Description = "Ability to change own nickname, a custom name for server", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Manage Nicknames", Code = "MANAGE_NICKNAMES", Description = "Ability to change the nicknames of other members in server", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Kick Members", Code = "KICK_MEMBERS", Description = "Ability to kick members from server. Kicked members will be able to rejoin", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Ban Members", Code = "BAN_MEMBERS", Description = "Ability to permanently ban and delete the message history of other members", IsServer = true },
       //new Permission { Id = Guid.NewGuid(), Name = "Send Messages", Code = "SEND_MESSAGES", Description = "Ability to send messages in text channels", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Attach Files", Code = "ATTACH_FILES", Description = "Ability to upload files or media in text channels", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Manage Messages", Code = "MANAGE_MESSAGES", Description = "Ability to delete messages by other members or pin any message", IsServer = true },
       //new Permission { Id = Guid.NewGuid(), Name = "Send Voice Messages", Code = "SEND_VOICE_MESSAGES", Description = "Ability to send voice messages", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Connect", Code = "CONNECT", Description = "Ability to join voice channels and hear others", IsServer = true },
       //new Permission { Id = Guid.NewGuid(), Name = "Speak", Code = "SPEAK", Description = "Ability to talk in voice channels", IsServer = true },
       //new Permission { Id = Guid.NewGuid(), Name = "Video", Code = "VIDEO", Description = "Ability to share video, screen share or stream in server", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Mute Members", Code = "MUTE_MEMBERS", Description = "Ability to mute other members in voice channels for everyone", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Deafen Members", Code = "DEAFEN_MEMBERS", Description = "Ability to deafen other members in voice channels", IsServer = true },
       new Permission { Id = Guid.NewGuid(), Name = "Manage Categories", Code = "MANAGE_CATEGORIES", Description = "Ability to create, delete, or edit categories", IsServer = true },


       // Channel-specific permissions
       new Permission { Id = Guid.NewGuid(), Name = "View Channel", Code = "VIEW_CHANNEL", Description = "Ability to view the channel. Disable means channel is private", IsServer = false },
       new Permission { Id = Guid.NewGuid(), Name = "Manage Channel", Code = "MANAGE_CHANNEL", Description = "Ability to change channel is name and delete channel", IsServer = false },
       new Permission { Id = Guid.NewGuid(), Name = "Manage Permissions", Code = "MANAGE_PERMISSIONS", Description = "Ability to change channel is permission", IsServer = false },
       new Permission { Id = Guid.NewGuid(), Name = "Send Messages", Code = "SEND_MESSAGES_CHANNEL", Description = "Ability to send messages in channel", IsServer = false },
       new Permission { Id = Guid.NewGuid(), Name = "Attach Files", Code = "ATTACH_FILES_CHANNEL", Description = "Ability to upload files or media in channel", IsServer = false },
       new Permission { Id = Guid.NewGuid(), Name = "Mention @everyone, and All Roles", Code = "MENTION_ALL", Description = "Ability to use @everyone or @mention all roles in channel", IsServer = false },
       new Permission { Id = Guid.NewGuid(), Name = "Manage Messages", Code = "MANAGE_MESSAGES_CHANNEL", Description = "Ability to delete messages by other members or pin any message", IsServer = false },
       new Permission { Id = Guid.NewGuid(), Name = "Send Voice Messages", Code = "SEND_VOICE_MESSAGES_CHANNEL", Description = "Ability to send voice messages in channel", IsServer = false }
   );


            // Event Permissions
            modelBuilder.Entity<Permission>().HasData(
       // new Permission { Id = Guid.NewGuid(), Name = "Manage Event", Code = "MANAGE_EVENT", Description = "Ability to manage event settings and participants", IsServer = false },
       // new Permission { Id = Guid.NewGuid(), Name = "Create Event", Code = "CREATE_EVENT", Description = "Ability to create new Event in Stage channel", IsServer = false },

            // Quiz Permission - channel is permission
        new Permission { Id = Guid.NewGuid(), Name = "Manage Quiz", Code = "MANAGE_QUIZ", Description = "Ability to manage quizzes settings and participants", IsServer = false }

    );


            //Primarykey settings
            modelBuilder.Entity<MemberRole>()
                 .HasKey(mr => new { mr.ServerMemberId, mr.RoleId });
            modelBuilder.Entity<Friendship>()
                .HasIndex(f => new { f.RequesterId, f.AddresseeId })
                .IsUnique();
            modelBuilder.Entity<Friendship>()
                .HasKey(f => new { f.AddresseeId, f.RequesterId });

            //Between tables
            //ChannelRolePermission
            modelBuilder.Entity<Role>()
              .HasMany(r => r.ChannelRolePermissions)
              .WithOne(crm => crm.Role)
              .HasForeignKey(crm => crm.RoleId)
              .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Channel>()
              .HasMany(r => r.ChannelRolePermissions)
              .WithOne(crm => crm.Channel)
              .HasForeignKey(crm => crm.ChannelId)
              .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Permission>()
              .HasMany(r => r.ChannelRolePermissions)
              .WithOne(crm => crm.Permission)
              .HasForeignKey(crm => crm.PermissionId)
              .OnDelete(DeleteBehavior.ClientSetNull);


            //RolePermission
            modelBuilder.Entity<Role>()
              .HasMany(r => r.RolePermissions)
              .WithOne(crm => crm.Role)
              .HasForeignKey(crm => crm.RoleId)
              .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Permission>()
              .HasMany(r => r.RolePermissions)
              .WithOne(crm => crm.Permission)
              .HasForeignKey(crm => crm.PermissionId)
              .OnDelete(DeleteBehavior.ClientSetNull);


            //MemberRole
            modelBuilder.Entity<Role>()
              .HasMany(r => r.MemberRoles)
              .WithOne(crm => crm.Role)
              .HasForeignKey(crm => crm.RoleId)
              .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<ServerMember>()
              .HasMany(r => r.MemberRoles)
              .WithOne(crm => crm.ServerMember)
              .HasForeignKey(crm => crm.ServerMemberId)
              .OnDelete(DeleteBehavior.ClientSetNull);


            //ServerMember
            modelBuilder.Entity<User>()
              .HasMany(r => r.ServerMembers)
              .WithOne(crm => crm.User)
              .HasForeignKey(crm => crm.UserId)
              .OnDelete(DeleteBehavior.ClientSetNull);


            modelBuilder.Entity<Server>()
              .HasMany(r => r.ServerMembers)
              .WithOne(crm => crm.Server)
              .HasForeignKey(crm => crm.ServerId)
              .OnDelete(DeleteBehavior.ClientSetNull);




            //Invite
            modelBuilder.Entity<ServerMember>()
              .HasMany(r => r.Invites)
              .WithOne(crm => crm.ServerMember)
              .HasForeignKey(crm => crm.CreatorId)
              .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Server>()
              .HasMany(r => r.Invites)
              .WithOne(crm => crm.Server)
              .HasForeignKey(crm => crm.ServerId)
              .OnDelete(DeleteBehavior.ClientSetNull);




            //InviteUsage
            modelBuilder.Entity<ServerMember>()
              .HasMany(r => r.InvitesUsages)
              .WithOne(crm => crm.ServerMember)
              .HasForeignKey(crm => crm.ServerMemberId)
              .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Invite>()
              .HasMany(r => r.InviteUsages)
              .WithOne(crm => crm.Invite)
              .HasForeignKey(crm => crm.InviteId)
              .OnDelete(DeleteBehavior.ClientSetNull);



            //FriendShip
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Requester)
                .WithMany(u => u.RequestedFriendships)
                .HasForeignKey(f => f.RequesterId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Addressee)
                .WithMany(u => u.ReceivedFriendships)
                .HasForeignKey(f => f.AddresseeId)
                .OnDelete(DeleteBehavior.Restrict);

            //Channel
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Channels)
                .WithOne(ch => ch.Category)
                .HasForeignKey(ch => ch.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Server>()
              .HasMany(r => r.Channels)
              .WithOne(crm => crm.Server)
              .HasForeignKey(crm => crm.ServerId)
              .OnDelete(DeleteBehavior.ClientSetNull);
 //End Between tables






            //User
            modelBuilder.Entity<User>()
                .HasMany(u => u.RequestedFriendships)
                .WithOne(f => f.Requester)
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.ReceivedFriendships)
                .WithOne(f => f.Addressee)
                .HasForeignKey(f => f.AddresseeId)
                .OnDelete(DeleteBehavior.Restrict);


            //Server 1->N
            //Emoji
            modelBuilder.Entity<Emoji>()
             .HasOne(e => e.Server)
             .WithMany(s => s.Emojis)
             .HasForeignKey(e => e.ServerId)
             .OnDelete(DeleteBehavior.ClientSetNull);
            //SoundBoard
            modelBuilder.Entity<SoundBoard>()
             .HasOne(e => e.Server)
             .WithMany(s => s.SoundBoards)
             .HasForeignKey(e => e.ServerId)
             .OnDelete(DeleteBehavior.ClientSetNull);
            //Role
            modelBuilder.Entity<Role>()
             .HasOne(e => e.Server)
             .WithMany(s => s.Roles)
             .HasForeignKey(e => e.ServerId)
             .OnDelete(DeleteBehavior.ClientSetNull);
            //Category
            modelBuilder.Entity<Category>()
             .HasOne(e => e.Server)
             .WithMany(s => s.Categories)
             .HasForeignKey(e => e.ServerId)
             .OnDelete(DeleteBehavior.Cascade);

            //Event - servermember
            modelBuilder.Entity<Event>()
            .HasOne(e => e.Creator)
            .WithMany(s => s.Events)
            .HasForeignKey(e => e.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

            //event - server 
            modelBuilder.Entity<Event>()
            .HasOne(e => e.Server)
            .WithMany(s => s.Events)
            .HasForeignKey(e => e.ServerId)
            .OnDelete(DeleteBehavior.Restrict);


            //event - channel
            modelBuilder.Entity<Event>()
            .HasOne(e => e.Channel)
            .WithMany(s => s.Events)
            .HasForeignKey(e => e.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

            //Quiz - channel
            modelBuilder.Entity<Quiz>()
           .HasOne(q => q.Channel)
           .WithMany(c => c.quizs)
           .HasForeignKey(e => e.ChannelId)
           .OnDelete(DeleteBehavior.Restrict);

            //Quiz - servermember
            modelBuilder.Entity<Quiz>()
           .HasOne(q => q.Creator)
           .WithMany(s => s.CreatedQuizzes)
           .HasForeignKey(e => e.CreatorId)
           .OnDelete(DeleteBehavior.Restrict);

            //QuizMember - quiz
            modelBuilder.Entity<QuizMember>()
           .HasOne(qm => qm.Quiz)
           .WithMany(q => q.Participants)
           .HasForeignKey(e => e.QuizId)
           .OnDelete(DeleteBehavior.Restrict);

            //QuizMember - servermember
            modelBuilder.Entity<QuizMember>()
           .HasOne(qm => qm.Participant)
           .WithMany(sm => sm.ParticipatedQuizzes)
           .HasForeignKey(e => e.ParticipantId)
           .OnDelete(DeleteBehavior.Restrict);

            //Subcription - User
            modelBuilder.Entity<Subcription>()
           .HasOne(s => s.User)
           .WithMany(u => u.Subcriptions)
           .HasForeignKey(e => e.UserId)
           .OnDelete(DeleteBehavior.Restrict);

            //Feature - Subcription
            modelBuilder.Entity<Feature>()
           .HasOne(f => f.Subcription)
           .WithMany(s => s.Features)
           .HasForeignKey(f => f.SubcriptionId)
           .OnDelete(DeleteBehavior.Restrict);


        }
    }


}