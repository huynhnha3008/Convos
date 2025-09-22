using BusinessObjects.DTOs.EventDto;
using BusinessObjects.DTOs.ServerDto;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ServersDTO
{
    public class ServerChannelRoleResponse
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<CategoryCreateResponse> Categories { get; set; }
        public ICollection<ChannelServerDetailResponse> Channels { get; set; } // channels not been created in category
        public ICollection<EmojiServerDetailResponse> Emojis { get; set; }
        public ICollection<ServerMemberDTO> ServerMembers { get; set; } // display on members list of the server
        public ICollection<InviteDetailDTO> Invites { get; set; } // display server is invitelist include creator is name
        public ICollection<EventCreateResponse> Events { get; set; }

    }
}
