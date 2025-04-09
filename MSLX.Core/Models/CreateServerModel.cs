using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSLX.Core.Models
{
    public class CreateServerModel
    {
        public required string Name { get; set; }
        public string? JavaPath { get; set; }
        public string? CorePath { get; set; }
        public int? MinMemory { get; set; }
        public int? MaxMemory { get; set; }
        public string? ServerArgs { get; set; }
        public string? ServerPath { get; set; }

    }
}
