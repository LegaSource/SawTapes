using BepInEx.Configuration;

namespace SawTapes
{
    public class ConfigManager
    {
        // GLOBAL
        public static ConfigEntry<int> rarityIncrement;
        public static ConfigEntry<bool> isSawTheme;
        // HUD
        public static ConfigEntry<bool> isSubtitles;
        public static ConfigEntry<float> chronoPosX;
        public static ConfigEntry<float> chronoPosY;
        // GASSING SYSTEM
        public static ConfigEntry<float> gassingDistance;
        //public static ConfigEntry<float> gassingCheatDistance;
        // SURVIVAL GAME
        public static ConfigEntry<int> survivalRarity;
        public static ConfigEntry<int> survivalMinPlayers;
        public static ConfigEntry<int> survivalMaxPlayers;
        public static ConfigEntry<int> survivalDuration;
        public static ConfigEntry<int> survivalBillyValue;
        public static ConfigEntry<string> survivalEnemies;
        public static ConfigEntry<string> survivalInteriorExclusions;
        // HUNTING GAME
        public static ConfigEntry<int> huntingRarity;
        public static ConfigEntry<int> huntingMinPlayers;
        public static ConfigEntry<int> huntingMaxPlayers;
        public static ConfigEntry<int> huntingDuration;
        public static ConfigEntry<int> huntingBillyValue;
        public static ConfigEntry<int> huntingReverseBearTrapValue;
        public static ConfigEntry<float> huntingAura;
        public static ConfigEntry<string> huntingExclusions;
        public static ConfigEntry<string> huntingInteriorExclusions;
        // ESCAPE GAME
        public static ConfigEntry<int> escapeRarity;
        public static ConfigEntry<int> escapeDuration;
        public static ConfigEntry<int> escapeBillyValue;
        public static ConfigEntry<int> escapeSawValue;
        public static ConfigEntry<int> escapeSawMaxUse;
        public static ConfigEntry<string> escapeHazards;
        public static ConfigEntry<string> escapeInteriorExclusions;

        // Encapsulation des paramètres qui pourraient être modifiés
        public static int SurvivalRarity => survivalRarity.Value;
        public static int HuntingRarity => huntingRarity.Value;
        public static int EscapeRarity => escapeRarity.Value;


        public static void Load()
        {
            // GLOBAL
            rarityIncrement = SawTapes.configFile.Bind(Constants.GLOBAL, "Rarity increment", 10, "By how much does the chance of a Saw game appearing increase with each round if it hasn't appeared?");
            isSawTheme = SawTapes.configFile.Bind(Constants.GLOBAL, "Enable Saw theme", true, "Is Saw theme enabled?");
            // HUD
            isSubtitles = SawTapes.configFile.Bind(Constants.HUD, "Enable subtitles", false, "Is subtitles enabled?");
            chronoPosX = SawTapes.configFile.Bind(Constants.HUD, "Chrono pos X", 106f, "X position of chrono on interface.");
            chronoPosY = SawTapes.configFile.Bind(Constants.HUD, "Chrono pos Y", -50f, "Y position of chrono on interface.");
            // GASSING SYSTEM
            gassingDistance = SawTapes.configFile.Bind(Constants.GASSING_SYSTEM, "Distance", 15f, "Maximum distance between the player and the tape before he is gassed");
            // SURVIVAL GAME
            survivalRarity = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Rarity", 20, $"Default probability of the {Constants.SURVIVAL_GAME} mini-game appearing");
            survivalMinPlayers = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Min players", 2, $"Minimum number of players for {Constants.SURVIVAL_GAME}");
            survivalMaxPlayers = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Max players", -1, $"Maximum number of players for {Constants.SURVIVAL_GAME}");
            survivalDuration = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Duration", 120, $"Duration of the {Constants.SURVIVAL_GAME}");
            survivalBillyValue = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Billy value", 120, $"Billy value for the {Constants.SURVIVAL_GAME}");
            survivalEnemies = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Enemies list", "Blob,Crawler,Bunker Spider,Flowerman,Puffer,Hoarding bug,Spring,Clay Surgeon,Masked,Nutcracker", $"List of creatures that will be selected by the {Constants.SURVIVAL_GAME}.\nYou can add enemies by separating them with a comma.");
            survivalInteriorExclusions = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Interiors exclusion list", "", $"List of interiors in which the {Constants.SURVIVAL_GAME} will not appear.");
            // HUNTING GAME
            huntingRarity = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Rarity", 20, $"Default probability of the {Constants.HUNTING_GAME} mini-game appearing");
            huntingMinPlayers = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Min players", 2, $"Minimum number of players for {Constants.HUNTING_GAME}");
            huntingMaxPlayers = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Max players", -1, $"Maximum number of players for {Constants.HUNTING_GAME}");
            huntingDuration = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Duration", 150, $"Duration of the {Constants.HUNTING_GAME}");
            huntingBillyValue = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Billy value", 90, $"Billy value for the {Constants.HUNTING_GAME}");
            huntingReverseBearTrapValue = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Reverse bear trap value", 30, $"Reverse Bear Trap value for the {Constants.HUNTING_GAME}");
            huntingAura = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Aura duration", 30f, "Duration for which the enemy's aura is visible through walls");
            huntingExclusions = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Enemies exclusion list", "Blob,Maneater,Lasso,Red pill", $"List of creatures that will not be selected by the {Constants.HUNTING_GAME}.\nYou can add enemies by separating them with a comma.");
            huntingInteriorExclusions = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Interiors exclusion list", "", $"List of interiors in which the {Constants.HUNTING_GAME} will not appear.");
            // ESCAPE GAME
            escapeRarity = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Rarity", 20, $"Default probability of the {Constants.ESCAPE_GAME} mini-game appearing");
            escapeDuration = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Duration", 150, $"Duration of the {Constants.ESCAPE_GAME}");
            escapeBillyValue = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Billy value", 90, $"Billy value for the {Constants.ESCAPE_GAME}");
            escapeSawValue = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Saw value", 60, $"{Constants.SAW} value");
            escapeSawMaxUse = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Saw max use", 3, $"Maximum number of uses before the {Constants.SAW} breaks");
            escapeHazards = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Hazards list", "Landmine:2:false:false:false:false,SpikeRoofTrapHazard:3:false:true:true:true,TurretContainer:1:true:false:false:false,LaserTurret:2:false:false:false:false,FanTrapAnimated:2:false:false:false:false,FunctionalMicrowave:1:false:false:false:false", $"List of spawnable hazards for the {Constants.ESCAPE_GAME}.\nThe format is 'HazardName:Weight:SpawnFacingAwayFromWall:SpawnFacingWall:SpawnWithBackToWall:SpawnWithBackFlushAgainstWall'.");
            escapeInteriorExclusions = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Interiors exclusion list", "GrandArmoryFlow,Level3Flow,StorehouseFlow", $"List of interiors in which the {Constants.ESCAPE_GAME} will not appear.");
        }
    }
}
