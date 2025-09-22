using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.UserDTO
{
    public class FriendRequestModel
    {
        public string RequesterUsername { get; set; }
        public string AddresseeUsername { get; set; }
    }
}
