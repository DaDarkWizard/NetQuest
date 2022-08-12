using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetQuest.World
{
    public class Room
    {
        public UInt64 Id { get; set; }
        public string? Name { get; set; }

        public Dictionary<Cardinals, UInt64> Rooms { get; set; } = new();

        public Dictionary<RoomLocations, UInt64> Entities { get; set; } = new();

        public RoomLocations GetLocation(string value)
        {
            value = value.ToLower();
            if (value is "north" or "northwall" or "north wall")
            {
                return RoomLocations.NorthWall;
            }
            else if (value is "east" or "eastwall" or "east wall")
            {
                return RoomLocations.EastWall;
            }
            else if (value is "west" or "westwall" or "west wall")
            {
                return RoomLocations.WestWall;
            }
            else if (value is "south" or "southwall" or "south wall")
            {
                return RoomLocations.SouthWall;
            }
            else if (value is "northwest" or "northwestcorner" or "north west corner" or "northwest corner")
            {
                return RoomLocations.NorthWestCorner;
            }
            else if (value is "northeast" or "northeastcorner" or "north east corner" or "northeast corner")
            {
                return RoomLocations.NorthEastCorner;
            }
            else if (value is "southwest" or "southwestcorner" or "south west corner" or "southwest corner")
            {
                return RoomLocations.SouthWestCorner;
            }
            else if (value is "southeast" or "southeastcorner" or "south east corner" or "southeast corner")
            {
                return RoomLocations.SouthEastCorner;
            }
            else
            {
                return RoomLocations.Center;
            }
        }
    }

    public enum Cardinals
    { 
        Above,
        Below,
        North,
        South,
        East,
        West
    }

    public enum RoomLocations
    {
        NorthWall,
        EastWall,
        WestWall,
        SouthWall,
        NorthWestCorner,
        NorthEastCorner,
        SouthWestCorner,
        SouthEastCorner,
        Center
    }
}
