using BepInEx.Configuration;

namespace SawTapes.Managers;

public class ConfigManager
{
    // GLOBAL
    public static ConfigEntry<int> rarityIncrement;
    public static ConfigEntry<bool> isSawTheme;
    public static ConfigEntry<float> gassingDistance;
    public static ConfigEntry<string> excludedPlayers;
    // HUD
    public static ConfigEntry<bool> isSubtitles;
    public static ConfigEntry<float> chronoPosX;
    public static ConfigEntry<float> chronoPosY;
    // ITEMS
    public static ConfigEntry<float> eyeAuraDurationHunting;
    public static ConfigEntry<int> eyeCooldownHunting;
    public static ConfigEntry<float> eyeDistanceSurvival;
    public static ConfigEntry<int> eyeCooldownSurvival;
    public static ConfigEntry<int> reverseBearTrapValue;
    public static ConfigEntry<int> sawValue;
    public static ConfigEntry<int> sawMaxUse;
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
    public static ConfigEntry<string> huntingEnemies;
    public static ConfigEntry<string> huntingInteriorExclusions;
    // ESCAPE GAME
    public static ConfigEntry<int> escapeRarity;
    public static ConfigEntry<int> escapeMinPlayers;
    public static ConfigEntry<int> escapeMaxPlayers;
    public static ConfigEntry<int> escapeDuration;
    public static ConfigEntry<int> escapeBillyValue;
    public static ConfigEntry<string> escapeHazards;
    public static ConfigEntry<string> escapeInteriorExclusions;
    // EXPLOSIVE GAME
    public static ConfigEntry<int> explosiveRarity;
    public static ConfigEntry<int> explosiveMinPlayers;
    public static ConfigEntry<int> explosiveMaxPlayers;
    public static ConfigEntry<int> explosiveDuration;
    public static ConfigEntry<int> explosiveExtraDuration;
    public static ConfigEntry<int> explosiveBillyValue;
    public static ConfigEntry<float> explosiveAura;
    public static ConfigEntry<string> explosiveInteriorExclusions;
    // JIGSAW'S JUDGEMENT
    public static ConfigEntry<float> bathroomPosX;
    public static ConfigEntry<float> bathroomPosY;
    public static ConfigEntry<int> bathroomDuration;
    public static ConfigEntry<int> bathroomCooldown;

    // Encapsulation des paramètres qui pourraient être modifiés
    public static int SurvivalRarity => survivalRarity.Value;
    public static int HuntingRarity => huntingRarity.Value;
    public static int EscapeRarity => escapeRarity.Value;
    public static int ExplosiveRarity => explosiveRarity.Value;

    public static void Load()
    {
        // GLOBAL
        rarityIncrement = SawTapes.configFile.Bind(Constants.GLOBAL, "Rarity increment", 15, "By how much does the chance of a Saw game appearing increase with each round if it hasn't appeared?");
        isSawTheme = SawTapes.configFile.Bind(Constants.GLOBAL, "Enable Saw theme", true, "Is Saw theme enabled?");
        gassingDistance = SawTapes.configFile.Bind(Constants.GLOBAL, "Gassing distance", 30f, "Maximum distance between the player and the tape before he is gassed");
        excludedPlayers = SawTapes.configFile.Bind(Constants.GLOBAL, "Excluded players", "", "List of players who cannot be selected for mini-games");
        // HUD
        isSubtitles = SawTapes.configFile.Bind(Constants.HUD, "Enable subtitles", false, "Is subtitles enabled?");
        chronoPosX = SawTapes.configFile.Bind(Constants.HUD, "Chrono pos X", 106f, "X position of chrono on interface.");
        chronoPosY = SawTapes.configFile.Bind(Constants.HUD, "Chrono pos Y", -50f, "Y position of chrono on interface.");
        // ITEMS
        eyeAuraDurationHunting = SawTapes.configFile.Bind(Constants.MONSTER_EYE, $"Aura duration - {Constants.HUNTING_GAME}", 5f, $"Duration for which the enemy's aura is visible through walls for the {Constants.HUNTING_GAME}");
        eyeCooldownHunting = SawTapes.configFile.Bind(Constants.MONSTER_EYE, $"Cooldown - {Constants.HUNTING_GAME}", 20, $"Cooldown for the {Constants.HUNTING_GAME}");
        eyeDistanceSurvival = SawTapes.configFile.Bind(Constants.MONSTER_EYE, $"Activation range - {Constants.SURVIVAL_GAME}", 10f, $"Maximum distance between player and enemy for activation for the {Constants.SURVIVAL_GAME}");
        eyeCooldownSurvival = SawTapes.configFile.Bind(Constants.MONSTER_EYE, $"Cooldown - {Constants.SURVIVAL_GAME}", 20, $"Cooldown for the {Constants.SURVIVAL_GAME}");
        reverseBearTrapValue = SawTapes.configFile.Bind(Constants.REVERSE_BEAR_TRAP, $"{Constants.REVERSE_BEAR_TRAP} value", 30, $"{Constants.REVERSE_BEAR_TRAP} value");
        sawValue = SawTapes.configFile.Bind(Constants.SAW, $"{Constants.SAW} value", 60, $"{Constants.SAW} value");
        sawMaxUse = SawTapes.configFile.Bind(Constants.SAW, $"{Constants.SAW} max use", 3, $"Maximum number of uses before the {Constants.SAW} breaks");
        // SURVIVAL GAME
        survivalRarity = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Rarity", 20, $"Default probability of the {Constants.SURVIVAL_GAME} mini-game appearing");
        survivalMinPlayers = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Min players", 2, $"Minimum number of players for {Constants.SURVIVAL_GAME} - Set to -1 to limit it to the number of connected players who can participate");
        survivalMaxPlayers = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Max players", -1, $"Maximum number of players for {Constants.SURVIVAL_GAME} - Set to -1 to limit it to the number of connected players who can participate");
        survivalDuration = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Duration", 120, $"Duration of the {Constants.SURVIVAL_GAME}");
        survivalBillyValue = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Billy value", 120, $"Billy value for the {Constants.SURVIVAL_GAME}");
        survivalEnemies = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Enemies list", "Blob,Crawler,Bunker Spider,Flowerman,Puffer,Hoarding bug,Spring,Clay Surgeon,Masked,Nutcracker,Butler", $"List of creatures that will be selected by the {Constants.SURVIVAL_GAME}.\nYou can add enemies by separating them with a comma.");
        survivalInteriorExclusions = SawTapes.configFile.Bind(Constants.SURVIVAL_GAME, "Interiors exclusion list", "", $"List of interiors in which the {Constants.SURVIVAL_GAME} will not appear.");
        // HUNTING GAME
        huntingRarity = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Rarity", 20, $"Default probability of the {Constants.HUNTING_GAME} mini-game appearing");
        huntingMinPlayers = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Min players", 2, $"Minimum number of players for {Constants.HUNTING_GAME} - Set to -1 to limit it to the number of connected players who can participate");
        huntingMaxPlayers = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Max players", -1, $"Maximum number of players for {Constants.HUNTING_GAME} - Set to -1 to limit it to the number of connected players who can participate");
        huntingDuration = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Duration", 150, $"Duration of the {Constants.HUNTING_GAME}");
        huntingBillyValue = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Billy value", 90, $"Billy value for the {Constants.HUNTING_GAME}");
        huntingEnemies = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Enemies list", "Crawler,Bunker Spider,Flowerman,Hoarding bug,Masked,Nutcracker,Butler", $"List of creatures that will be selected by the {Constants.HUNTING_GAME}.\nYou can add enemies by separating them with a comma.");
        huntingInteriorExclusions = SawTapes.configFile.Bind(Constants.HUNTING_GAME, "Interiors exclusion list", "", $"List of interiors in which the {Constants.HUNTING_GAME} will not appear.");
        // ESCAPE GAME
        escapeRarity = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Rarity", 20, $"Default probability of the {Constants.ESCAPE_GAME} mini-game appearing");
        escapeMinPlayers = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Min players", 2, $"Minimum number of players for {Constants.ESCAPE_GAME} - Set to -1 to limit it to the number of connected players who can participate");
        escapeMaxPlayers = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Max players", -1, $"Maximum number of players for {Constants.ESCAPE_GAME} - Set to -1 to limit it to the number of connected players who can participate");
        escapeDuration = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Duration", 150, $"Duration of the {Constants.ESCAPE_GAME}");
        escapeBillyValue = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Billy value", 90, $"Billy value for the {Constants.ESCAPE_GAME}");
        escapeHazards = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Hazards list", "Landmine:2:false:false:false:false,SpikeRoofTrapHazard:3:false:true:true:true,TurretContainer:1:true:false:false:false,LaserTurret:2:false:false:false:false,FanTrapAnimated:2:false:false:false:false,FunctionalMicrowave:1:false:false:false:false", $"List of spawnable hazards for the {Constants.ESCAPE_GAME}.\nThe format is 'HazardName:Weight:SpawnFacingAwayFromWall:SpawnFacingWall:SpawnWithBackToWall:SpawnWithBackFlushAgainstWall'.");
        escapeInteriorExclusions = SawTapes.configFile.Bind(Constants.ESCAPE_GAME, "Interiors exclusion list", "GrandArmoryFlow,Level3Flow,StorehouseFlow", $"List of interiors in which the {Constants.ESCAPE_GAME} will not appear.");
        // EXPLOSIVE GAME
        explosiveRarity = SawTapes.configFile.Bind(Constants.EXPLOSIVE_GAME, "Rarity", 20, $"Default probability of the {Constants.EXPLOSIVE_GAME} mini-game appearing");
        explosiveMinPlayers = SawTapes.configFile.Bind(Constants.EXPLOSIVE_GAME, "Min players", 2, $"Minimum number of players for {Constants.EXPLOSIVE_GAME} - Set to -1 to limit it to the number of connected players who can participate");
        explosiveMaxPlayers = SawTapes.configFile.Bind(Constants.EXPLOSIVE_GAME, "Max players", -1, $"Maximum number of players for {Constants.EXPLOSIVE_GAME} - Set to -1 to limit it to the number of connected players who can participate");
        explosiveDuration = SawTapes.configFile.Bind(Constants.EXPLOSIVE_GAME, "Duration", 150, $"Duration of the {Constants.EXPLOSIVE_GAME}");
        explosiveExtraDuration = SawTapes.configFile.Bind(Constants.EXPLOSIVE_GAME, "Extra duration", 90, $"Extra duration added to the basic one for the second part of the {Constants.EXPLOSIVE_GAME}");
        explosiveBillyValue = SawTapes.configFile.Bind(Constants.EXPLOSIVE_GAME, "Billy value", 60, $"Billy value for the {Constants.EXPLOSIVE_GAME}");
        explosiveAura = SawTapes.configFile.Bind(Constants.EXPLOSIVE_GAME, "Aura duration", 30f, "Duration for which the Hoarding bug's aura is visible through walls");
        explosiveInteriorExclusions = SawTapes.configFile.Bind(Constants.EXPLOSIVE_GAME, "Interiors exclusion list", "", $"List of interiors in which the {Constants.EXPLOSIVE_GAME} will not appear.");
        // JIGSAW'S JUDGEMENT
        bathroomPosX = SawTapes.configFile.Bind(Constants.JIGSAW_JUDGEMENT, "Position X", -700f, $"Position X of the Bathroom");
        bathroomPosY = SawTapes.configFile.Bind(Constants.JIGSAW_JUDGEMENT, "Position Y", -200f, $"Position Y of the Bathroom");
        bathroomDuration = SawTapes.configFile.Bind(Constants.JIGSAW_JUDGEMENT, "Duration", 60, $"Duration of the {Constants.JIGSAW_JUDGEMENT}");
        bathroomCooldown = SawTapes.configFile.Bind(Constants.JIGSAW_JUDGEMENT, "Cooldown", 600, $"Duration of the {Constants.JIGSAW_JUDGEMENT}");
    }
}
