using BepInEx.Configuration;

namespace SawTapes
{
    public class ConfigManager
    {
        // GLOBAL
        public static ConfigEntry<int> rarityIncrement;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<bool> isInfoInGame;
        public static ConfigEntry<bool> isSawTheme;
        public static ConfigEntry<bool> isSubtitles;
        // SURVIVAL GAME
        public static ConfigEntry<int> survivalRarity;
        public static ConfigEntry<bool> penalizePlayerWhoCamp;
        public static ConfigEntry<int> campDuration;
        // HUNTING GAME
        public static ConfigEntry<int> huntingRarity;
        public static ConfigEntry<float> huntingDistance;
        public static ConfigEntry<int> huntingDuration;
        public static ConfigEntry<int> huntingBillyValue;
        public static ConfigEntry<int> huntingReverseBearTrapValue;
        public static ConfigEntry<float> huntingAura;
        public static ConfigEntry<string> huntingExclusions;

        public static void Load()
        {
            rarityIncrement = SawTapes.configFile.Bind("_Global_", "Rarity increment", 10, "By how much does the chance of a Saw game appearing increase with each round if it hasn't appeared?");
            isDebug = SawTapes.configFile.Bind("_Global_", "Enable debugging", false, "Is debugging enabled?");
            isInfoInGame = SawTapes.configFile.Bind("_Global_", "Enable Saw theme", true, "Display a tip when the player enters the mini-game to inform them to find the tape");
            isSawTheme = SawTapes.configFile.Bind("_Global_", "Enable Saw theme", true, "Is Saw theme enabled?");
            isSubtitles = SawTapes.configFile.Bind("_Global_", "Enable subtitles", false, "Is subtitles enabled?");
            // SURVIVAL GAME
            survivalRarity = SawTapes.configFile.Bind("Survival Game", "Rarity", 20, "Default probability of the Survival Game mini-game appearing");
            penalizePlayerWhoCamp = SawTapes.configFile.Bind("Survival Game", "Penalize campers", true, "Enable the possibility to penalize the player who is camping by spawning a Nutcracker");
            campDuration = SawTapes.configFile.Bind("Survival Game", "Camping duration", 5, "Total camping duration before the player is killed");
            // HUNTING GAME
            huntingRarity = SawTapes.configFile.Bind("Hunting Game", "Rarity", 20, "Default probability of the Hunting Game mini-game appearing");
            huntingDistance = SawTapes.configFile.Bind("Hunting Game", "Distance", 50f, "Maximum distance between the player and the tape before he is gassed");
            huntingDuration = SawTapes.configFile.Bind("Hunting Game", "Duration", 150, "Duration of the Hunting Game");
            huntingBillyValue = SawTapes.configFile.Bind("Hunting Game", "Billy value", 90, "Billy value for the Hunting Game");
            huntingReverseBearTrapValue = SawTapes.configFile.Bind("Hunting Game", "Reverse bear trap value", 30, "Reverse Bear Trap value for the Hunting Game");
            huntingAura = SawTapes.configFile.Bind("Hunting Game", "Aura duration", 10f, "Duration for which the enemy's aura is visible through walls");
            huntingExclusions = SawTapes.configFile.Bind("Hunting Game", "Exclusion list", "Blob,Maneater,Lasso,Red pill", "List of creatures that will not be selected by the Hunting Game.\nYou can add enemies by separating them with a comma.");
        }
    }
}
