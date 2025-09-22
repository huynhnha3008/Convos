using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ServerDto
{
    public class InviteUsageServerDetailResponse
    {
        public DateTime JoinAt { get; set; }
        public string JoinCode { get; set; }
    }
}
