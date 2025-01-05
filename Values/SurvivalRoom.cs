using System.Collections.Generic;

namespace SawTapes.Values
{
    public class SurvivalRoom(string roomName, List<string> doorsNames, int weight, List<SurvivalHorde> hordes)
    {
        public string RoomName { get; internal set; } = roomName;
        public List<string> DoorsNames { get; internal set; } = doorsNames;
        public int Weight { get; internal set; } = weight;
        public List<SurvivalHorde> Hordes { get; internal set; } = hordes;
    }
}
