using System.Collections.Generic;

namespace SawTapes.Values
{
    public class Horde
    {
        public string HordeName { get; internal set; }
        public int GameDuration { get; internal set; }
        public int BillyValue { get; internal set; }
        public Dictionary<int, EnemyType> EnemiesSpawn { get; internal set; }

        public Horde(string hordeName, int gameDuration, int billyValue, Dictionary<int, EnemyType> enemiesSpawn)
        {
            this.HordeName = hordeName;
            this.GameDuration = gameDuration;
            this.BillyValue = billyValue;
            this.EnemiesSpawn = enemiesSpawn;
        }
    }
}
