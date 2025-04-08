using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSLX.Core.Models.FrpService
{
    public class MSLFrpModel
    {
        public class Tunnel
        {
            public required string Name { get; set; }
            public required string Status { get; set; }
            public required string LocalPort { get; set; }
            public required string RemotePort { get; set; }
            public required string Node { get; set; }
        }
    }
}
