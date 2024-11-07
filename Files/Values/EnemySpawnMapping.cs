using Newtonsoft.Json;

namespace SawTapes.Files.Values
{
    public class EnemySpawnMapping
    {
        [JsonProperty("enemy")]
        public string Enemy { get; internal set; }

        [JsonProperty("time")]
        public int Time { get; internal set; }
    }
}
