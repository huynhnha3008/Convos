using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class ServerCreateRequest
    {
        public string Name { get; set; }
        public int Type { get; set; }
    }
}
