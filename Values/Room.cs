using System.Collections.Generic;

namespace SawTapes.Values
{
    public class Room
    {
        public string RoomName { get; internal set; }
        public List<string> DoorsNames { get; internal set; }
        public int Weight { get; internal set; }
        public List<Horde> Hordes { get; internal set; }

        public Room(string roomName, List<string> doorsNames, int weight, List<Horde> hordes)
        {
            this.RoomName = roomName;
            this.DoorsNames = doorsNames;
            this.Weight = weight;
            this.Hordes = hordes;
        }
    }
}
