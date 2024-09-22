using Newtonsoft.Json;
using System.Collections.Generic;

namespace SawTapes.Files.Values
{
    internal class HordeMapping
    {
        [JsonProperty("horde_name")]
        public string HordeName { get; internal set; }

        [JsonProperty("game_duration")]
        public int GameDuration { get; internal set; }

        [JsonProperty("min_hour")]
        public int MinHour { get; internal set; }

        [JsonProperty("max_hour")]
        public int MaxHour { get; internal set; }

        [JsonProperty("billy_value")]
        public int BillyValue { get; internal set; }

        [JsonProperty("enemies_spawn")]
        public List<EnemySpawnMapping> EnemiesSpawn { get; internal set; }
    }
}
