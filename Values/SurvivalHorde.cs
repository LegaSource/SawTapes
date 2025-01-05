using System.Collections.Generic;

namespace SawTapes.Values
{
    public class SurvivalHorde(string hordeName, int gameDuration, int billyValue, int minHour, int maxHour, Dictionary<int, EnemyType> enemiesSpawn)
    {
        public string HordeName { get; internal set; } = hordeName;
        public int GameDuration { get; internal set; } = gameDuration;
        public int BillyValue { get; internal set; } = billyValue;
        public int MinHour { get; internal set; } = minHour;
        public int MaxHour { get; internal set; } = maxHour;
        public Dictionary<int, EnemyType> EnemiesSpawn { get; internal set; } = enemiesSpawn;
    }
}
