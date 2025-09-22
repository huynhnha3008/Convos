using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class FriendshipReminderNameRequest
    {
        public Guid targetId { get; set; }

        [Required]
        public string reminderName { get; set; }
    }
}
