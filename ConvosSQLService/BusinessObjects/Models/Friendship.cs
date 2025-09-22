using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Friendship
    {
        public Guid RequesterId { get; set; }

        public User Requester { get; set; }

        public Guid AddresseeId { get; set; }


        public User Addressee { get; set; }

        public FriendshipStatus Status { get; set; }
        public string? ReminderName { get; set; }
    }

    public enum FriendshipStatus
    {
        Pending, //0
        Accepted, // 1
        Blocked // 2

    }
}
