using System.Collections.Generic;

namespace SawTapes.Values
{
    public class Horde
    {
        public string HordeName { get; internal set; }
        public int GameDuration { get; internal set; }
        public Dictionary<int, EnemyAI> EnemiesSpawn { get; internal set; }

        public Horde(string hordeName, int gameDuration, Dictionary<int, EnemyAI> enemiesSpawn)
        {
            this.HordeName = hordeName;
            this.GameDuration = gameDuration;
            this.EnemiesSpawn = enemiesSpawn;
        }
    }
}
