using Newtonsoft.Json;
using System.Collections.Generic;

namespace SawTapes.Files.Values
{
    public class SurvivalRoomMapping
    {
        [JsonProperty("room_name")]
        public string RoomName { get; internal set; }

        [JsonProperty("doors_names")]
        public List<string> DoorsNames { get; internal set; }

        [JsonProperty("weight")]
        public int Weight { get; internal set; }

        [JsonProperty("hordes")]
        public List<string> Hordes { get; internal set; }
    }
}
