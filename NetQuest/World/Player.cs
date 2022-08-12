using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetQuest.World
{
    public class Player
    {
        public UInt64 Id { get; set; }
        public UInt64 Room { get; set; }
        public List<UInt64> Inventory { get; set; } = new();
        public bool IsAdmin { get; set; }
    }
}
