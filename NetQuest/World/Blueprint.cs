using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetQuest.World
{
    public class Blueprint
    {
        public UInt64 Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public bool Craftable { get; set; }

        public Dictionary<UInt64, (int quantity, bool consumed)> Ingredients { get; set; } = new();

        public bool Harvestable { get; set; }

        public int HarvestDelay { get; set; }

        public Dictionary<UInt64, int> HarvestedItems { get; set; } = new();

        public Dictionary<UInt64, (int quantity, bool consumed)> HarvestRequires { get; set; } = new();

        public bool ConsumedOnHarvest { get; set; }
    }
}
