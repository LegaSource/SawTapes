using System.Collections.Generic;

namespace SawTapes.Values
{
    public class Room
    {
        public string RoomName { get; internal set; }
        public string DoorName { get; internal set; }
        public int Weight { get; internal set; }
        public List<Horde> Hordes { get; internal set; }

        public Room(string roomName, string doorName, int weight, List<Horde> hordes)
        {
            this.RoomName = roomName;
            this.DoorName = doorName;
            this.Weight = weight;
            this.Hordes = hordes;
        }
    }
}
