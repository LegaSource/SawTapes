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
        // HUD
        public static ConfigEntry<bool> isSubtitles;
        public static ConfigEntry<float> chronoPosX;
        public static ConfigEntry<float> chronoPosY;
        // SURVIVAL GAME
        public static ConfigEntry<int> survivalRarity;
        public static ConfigEntry<bool> penalizePlayerWhoCamp;
        public static ConfigEntry<int> campDuration;
        // HUNTING GAME
        public static ConfigEntry<int> huntingRarity;
        public static ConfigEntry<float> huntingGassedDistance;
        public static ConfigEntry<float> huntingCheatDistance;
        public static ConfigEntry<int> huntingDuration;
        public static ConfigEntry<int> huntingBillyValue;
        public static ConfigEntry<int> huntingReverseBearTrapValue;
        public static ConfigEntry<float> huntingAura;
        public static ConfigEntry<string> huntingExclusions;

        // Encapsulation des paramètres qui pourraient être modifiés
        public static int SurvivalRarity => survivalRarity.Value;
        public static int HuntingRarity => huntingRarity.Value;


        public static void Load()
        {
            // GLOBAL
            rarityIncrement = SawTapes.configFile.Bind(Constants.GLOBAL, "Rarity increment", 10, "By how much does the chance of a Saw game appearing increase with each round if it hasn't appeared?");
            isDebug = SawTapes.configFile.Bind(Constants.GLOBAL, "Enable debugging", false, "Is debugging enabled?");
            isInfoInGame = SawTapes.configFile.Bind(Constants.GLOBAL, "Enable Saw theme", true, "Display a tip when the player enters the mini-game to inform them to find the tape");
            isSawTheme = SawTapes.configFile.Bind(Constants.GLOBAL, "Enable Saw theme", true, "Is Saw theme enabled?");
            // HUD
            isSubtitles = SawTapes.configFile.Bind(Constants.HUD, "Enable subtitles", false, "Is subtitles enabled?");
            chronoPosX = SawTapes.configFile.Bind(Constants.HUD, "Chrono pos X", 106f, "X position of chrono on interface.");
            chronoPosY = SawTapes.configFile.Bind(Constants.HUD, "Chrono pos Y", -50f, "Y position of chrono on interface.");
            // SURVIVAL GAME
            survivalRarity = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Rarity", 20, $"Default probability of the {Constants.SURVIVAL_GAME} mini-game appearing");
            penalizePlayerWhoCamp = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Penalize campers", true, "Enable the possibility to penalize the player who is camping by spawning a Nutcracker");
            campDuration = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Camping duration", 5, "Total camping duration before the player is killed");
            // HUNTING GAME
            huntingRarity = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Rarity", 20, $"Default probability of the {Constants.HUNTING_GAME} mini-game appearing");
            huntingGassedDistance = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Gassed distance", 25f, "Maximum distance between the player and the tape before he is gassed");
            huntingCheatDistance = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Cheat distance", 15f, "Maximum distance the player can move before launching the tape. Beyond that, he'll be killed.");
            huntingDuration = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Duration", 150, $"Duration of the {Constants.HUNTING_GAME}");
            huntingBillyValue = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Billy value", 90, $"Billy value for the {Constants.HUNTING_GAME}");
            huntingReverseBearTrapValue = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Reverse bear trap value", 30, $"Reverse Bear Trap value for the {Constants.HUNTING_GAME}");
            huntingAura = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Aura duration", 10f, "Duration for which the enemy's aura is visible through walls");
            huntingExclusions = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Exclusion list", "Blob,Maneater,Lasso,Red pill", "List of creatures that will not be selected by the Hunting Game.\nYou can add enemies by separating them with a comma.");
        }
    }
}
