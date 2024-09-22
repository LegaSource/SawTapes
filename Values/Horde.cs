using System.Collections.Generic;

namespace SawTapes.Values
{
    public class Horde
    {
        public string HordeName { get; internal set; }
        public int GameDuration { get; internal set; }
        public int BillyValue { get; internal set; }
        public int MinHour { get; internal set; }
        public int MaxHour { get; internal set; }
        public Dictionary<int, EnemyType> EnemiesSpawn { get; internal set; }

        public Horde(string hordeName, int gameDuration, int billyValue, int minHour, int maxHour, Dictionary<int, EnemyType> enemiesSpawn)
        {
            this.HordeName = hordeName;
            this.GameDuration = gameDuration;
            this.BillyValue = billyValue;
            this.MinHour = minHour;
            this.MaxHour = maxHour;
            this.EnemiesSpawn = enemiesSpawn;
        }
    }
}
