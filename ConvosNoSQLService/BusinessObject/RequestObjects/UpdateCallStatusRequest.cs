using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessObject.RequestObjects
{
    public class UpdateCallStatusRequest
    {
        public string Status { get; set; }
        public string Reason { get; set; }
    }
}