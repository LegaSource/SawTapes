using BepInEx.Configuration;

namespace SawTapes
{
    internal class ConfigManager
    {
        // GLOBAL
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<bool> isInfoInGame;
        public static ConfigEntry<bool> isSawTheme;
        public static ConfigEntry<bool> isSubtitles;
        // SURVIVAL GAME
        public static ConfigEntry<int> survivalRarity;
        public static ConfigEntry<bool> killPlayerWhoCamp;
        public static ConfigEntry<int> campDuration;

        internal static void Load()
        {
            isDebug = SawTapes.configFile.Bind<bool>("_Global_", "Enable debugging", false, "Is debugging enabled?");
            isInfoInGame = SawTapes.configFile.Bind<bool>("_Global_", "Enable Saw theme", true, "Display a tip when the player enters the mini-game to inform them to find the tape");
            isSawTheme = SawTapes.configFile.Bind<bool>("_Global_", "Enable Saw theme", true, "Is Saw theme enabled?");
            isSubtitles = SawTapes.configFile.Bind<bool>("_Global_", "Enable subtitles", false, "Is subtitles enabled?");
            // SURVIVAL GAME
            survivalRarity = SawTapes.configFile.Bind<int>("Survival Game", "Rarity", 20, "Probability of the 'Survival Game' mini-game appearing");
            killPlayerWhoCamp = SawTapes.configFile.Bind<bool>("Survival Game", "Kill campers", true, "Enable the possibility to kill the player who is camping");
            campDuration = SawTapes.configFile.Bind<int>("Survival Game", "Camping duration", 10, "Total camping duration before the player is killed");
        }
    }
}
