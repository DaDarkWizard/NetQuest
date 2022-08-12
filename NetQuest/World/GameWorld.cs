using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetQuest.World
{
    public class GameWorld
    {
        public string Name { get; private set; }

        public Dictionary<UInt64, Room> Rooms { get; private set; } = new();

        public Dictionary<UInt64, Entity> Entities { get; private set; } = new();

        public Dictionary<UInt64, Blueprint> Blueprints { get; private set; } = new();

        public UInt64 CurrentRoom { get; set; }

        public List<UInt64> Inventory { get; set; } = new();

        public UInt64 RoomCounter = 2;
        public UInt64 EntityCounter = 1;
        public UInt64 BlueprintCounter = 1;

        public GameWorld(string name)
        {
            Name = name;
        }

        public void AddToInventory(Entity entity)
        {
            Entity? inventory = Inventory.Select(x => Entities[x]).FirstOrDefault(x => x.Blueprint == entity.Blueprint);
            if(inventory is not null)
            {
                inventory.Quantity += entity.Quantity;
                if(Entities.ContainsKey(entity.Id))
                {
                    Entities.Remove(entity.Id);
                }
            }
            else
            {
                if(Rooms[CurrentRoom]?.Entities.Where(x => Entities[x.Value] == entity).Any() ?? false)
                {
                    foreach(var ent in Rooms[CurrentRoom].Entities.Where(x => Entities[x.Value] == entity))
                    {
                        Rooms[CurrentRoom].Entities.Remove(ent.Key);
                    }
                }
                if(!Entities.ContainsKey(entity.Id))
                {
                    entity.Id = EntityCounter++;
                    Entities[entity.Id] = entity;
                }
                Inventory.Add(entity.Id);
            }
        }

        public void AddRangeToInventory(ICollection<Entity> entities)
        {
            foreach(var entity in entities)
            {
                AddToInventory(entity);
            }
        }

        public bool HasResouresToHarvestQuantity(Blueprint blueprint, int quantity)
        {
            foreach(var item in blueprint.HarvestRequires)
            {
                var ent = Inventory.Select(x => Entities[x]).FirstOrDefault(x => x.Blueprint == item.Key);
                if(ent is null)
                {
                    return false;
                }
                if(item.Value.consumed && ent.Quantity < quantity * item.Value.quantity)
                {
                    return false;
                }
                if(!item.Value.consumed && ent.Quantity < item.Value.quantity)
                {
                    return false;
                }
            }
            return true;
        }

        public void RemoveFromInventory(Blueprint blueprint, int quantity)
        {
            var ent = Inventory.Select(x => Entities[x]).FirstOrDefault(x => x.Blueprint == blueprint.Id);
            if(ent is null)
            {
                return;
            }
            ent.Quantity -= quantity;
            if(ent.Quantity <= 0)
            {
                Inventory.Remove(ent.Id);
                Entities.Remove(ent.Id);
            }
        }

        public int InventoryContains(string name)
        {
            return Entities.Where(x => Inventory.Contains(x.Key) && Blueprints[x.Value.Blueprint].Name == name).FirstOrDefault().Value?.Quantity ?? 0;
        }
    }
}
