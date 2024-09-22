using Newtonsoft.Json;
using System.Collections.Generic;

namespace SawTapes.Files.Values
{
    internal class RoomMapping
    {
        [JsonProperty("room_name")]
        public string RoomName { get; internal set; }

        [JsonProperty("door_name")]
        public string DoorName { get; internal set; }

        [JsonProperty("weight")]
        public int Weight { get; internal set; }

        [JsonProperty("hordes")]
        public List<string> Hordes { get; internal set; }
    }
}
