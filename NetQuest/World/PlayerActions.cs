using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetQuest.World
{
    public class PlayerActions
    {
        private readonly GameWorld _world;
        private readonly Player _player;

        public PlayerActions(GameWorld world, Player player)
        {
            _world = world;
            _player = player;
        }

        public string Look()
        {
            var room = _world.Rooms[_player.Room];
            var itemsInRoom = room.Entities.Select(x => new { location = x.Key, entity = _world.Entities[x.Value] });
            StringBuilder output = new StringBuilder();

            output.Append($"You are in {room.Name}\n\n");
            if (room.Rooms.Count == 1)
            {
                output.Append($"There is a room {room.Rooms.First().Key}\n\n");
            }
            else if(room.Rooms.Count > 1)
            {
                output.Append("There are rooms to the ");
                var rooms = room.Rooms.Select(x => x.Key).ToList();
                for (int i = 0; i < rooms.Count - 1; i++)
                {
                    output.Append(rooms[i] + ", ");
                }
                output.Append(rooms.Last() + "\n\n");
            }
            if (room.Entities.Any())
            {
                foreach (var pair in itemsInRoom)
                {
                    output.Append($"On the {pair.location} of the room there is {pair.entity.Quantity} {_world.Blueprints[pair.entity.Blueprint].Name}\n\n");
                }
            }
            return output.ToString();
        }
    }
}
