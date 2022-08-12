using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetQuest.World
{
    public class Entity
    {
        public UInt64 Id { get; set; }

        public UInt64 Blueprint { get; set; }

        public int Quantity { get; set; }

        public DateTimeOffset LastHarvest { get; set; }
    }
}
