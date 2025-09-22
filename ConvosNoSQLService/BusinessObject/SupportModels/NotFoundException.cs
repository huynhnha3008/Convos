using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessObject.SupportModels
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}