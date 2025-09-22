using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessObject.RequestObjects
{
    public class InitiateCallRequest
{
    public string CallerId { get; set; }
    public string ReceiverId { get; set; }
    public string CallType { get; set; }
}
}