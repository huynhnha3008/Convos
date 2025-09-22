using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.DatabaseSettings
{
    public class DatabaseSettings
    {
        public string MongoDb { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
}