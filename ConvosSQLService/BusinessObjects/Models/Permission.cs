using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsServer { get; set; }
        public string Code { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; }

        public virtual ICollection<ChannelRolePermission> ChannelRolePermissions { get; set; }

    }

    public enum PermissionEnum
    {
        // Server-wide permissions
        //VIEW_CHANNELS,         // "Ability to view channels by default (excluding private channels)"
        MANAGE_CHANNELS,       // "Ability to create, delete, or edit channels"
        MANAGE_ROLES,          // "Ability to create, edit, and assign roles within the server"
        MANAGE_EMOJIS,         // "Ability to add or remove emojis available for the server"
        MANAGE_SERVER,         // "Ability to change server settings (e.g., name, icon, region), view all invites"
        CREATE_INVITE,         // "Ability to invite new people to server"
        CHANGE_NICKNAME,       // "Ability to change own nickname, a custom name for server"
        MANAGE_NICKNAMES,      // "Ability to change the nicknames of other members in server"
        KICK_MEMBERS,          // "Ability to kick members from server. Kicked members will be able to rejoin"
        BAN_MEMBERS,           // "Ability to permanently ban and delete the message history of other members"
        //SEND_MESSAGES,         // "Ability to send messages in text channels"
        ATTACH_FILES,          // "Ability to upload files or media in text channels"
        MANAGE_MESSAGES,       // "Ability to delete messages by other members or pin any message"
        //SEND_VOICE_MESSAGES,   // "Ability to send voice messages"
        CONNECT,               // "Ability to join voice channels and hear others"
       // SPEAK,                 // "Ability to talk in voice channels"
       // VIDEO,                 // "Ability to share video, screen share or stream in server"
        MUTE_MEMBERS,          // "Ability to mute other members in voice channels for everyone"
        DEAFEN_MEMBERS,        // "Ability to deafen other members in voice channels"
        MANAGE_CATEGORIES,     // "Ability to create, delete, or edit categories"


        // Channel-specific permissions
        VIEW_CHANNEL,          // "Ability to view the channel. Disable means channel is private"
        MANAGE_CHANNEL,        // "Ability to change channel is name and delete channel"
        MANAGE_PERMISSIONS,    // "Ability to change channel is permission"
        SEND_MESSAGES_CHANNEL, // "Ability to send messages in channel"
        ATTACH_FILES_CHANNEL,  // "Ability to upload files or media in channel"
        MENTION_ALL,           // "Ability to use @everyone or @mention all roles in channel"
        MANAGE_MESSAGES_CHANNEL,// "Ability to delete messages by other members or pin any message"
        SEND_VOICE_MESSAGES_CHANNEL, // "Ability to send voice messages in channel"
        //MANAGE_EVENT,           // Ability to manage event and participants
        //CREATE_EVENT,
        MANAGE_QUIZ
    }

}
