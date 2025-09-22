using BusinessObjects.DTOs.EventDto;
using BusinessObjects.DTOs.ServersDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ServerDto
{
    public class ServerBackupResponse
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Name { get; set; }

        public string Icon { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<RoleServerDetailReponse> Roles { get; set; }
        public ICollection<CategoryCreateResponse> Categories { get; set; }
        public ICollection<ChannelServerDetailResponse> Channels { get; set; } // channels not been created in category
        public ICollection<ServerMemberDTO> ServerMembers { get; set; } // display on members list of the server

        public ICollection<EmojiServerDetailResponse> Emojis { get; set; }
        public ICollection<SoundBoardCreateResponse> SoundBoards { get; set; }

        public ICollection<EventCreateResponse> Events { get; set; }

    }
}
