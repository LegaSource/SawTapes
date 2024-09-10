using System.Collections.Generic;

namespace SawTapes.Values
{
    public class Room
    {
        public string RoomName { get; internal set; }
        public string DoorName { get; internal set; }
        public List<Horde> Hordes { get; internal set; }

        public Room(string roomName, string doorName, List<Horde> hordes)
        {
            this.RoomName = roomName;
            this.DoorName = doorName;
            this.Hordes = hordes;
        }
    }
}
