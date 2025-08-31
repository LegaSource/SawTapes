using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using SawTapes.Behaviours.Bathroom;
using SawTapes.Behaviours.Bathroom.Items;
using SawTapes.Behaviours.Games;
using SawTapes.Behaviours.Games.EscapeGame;
using SawTapes.Behaviours.Games.ExplosiveGame;
using SawTapes.Behaviours.Games.HuntingGame;
using SawTapes.Behaviours.Games.SurvivalGame;
using SawTapes.Behaviours.Items;
using SawTapes.Managers;
using SawTapes.Patches;
using SawTapes.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SawTapes;

[BepInPlugin(modGUID, modName, modVersion)]
public class SawTapes : BaseUnityPlugin
{
    internal const string modGUID = "Lega.SawTapes";
    internal const string modName = "Saw Tapes";
    internal const string modVersion = "3.0.0";

    private readonly Harmony harmony = new Harmony(modGUID);
    private static readonly AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "sawtapes"));
    internal static ManualLogSource mls;
    public static ConfigFile configFile;

    public static GameObject managerPrefab = NetworkPrefabs.CreateNetworkPrefab("SawTapesNetworkManager");

    // Global objects
    public static SawTape sawTape;
    public static Bathroom bathroom;
    public static HashSet<EnemyType> allEnemies = [];

    // Items
    public static List<SawTapeValue> sawTapeValues = [];
    public static Item billyPuppet;
    public static Item billyPuppetJJ;
    public static Item billyHead;
    public static Item billyBody;
    public static Item sawEscape;
    public static Item sawBombExplosive;
    public static Item billyPuppetFD;
    public static Item rBTrapHunting;
    public static Item sawKeyHunting;
    public static Item billyPuppetHunting;
    public static Item billyPuppetHM;
    public static Item billyPuppetSurvival;
    public static Item billyPuppetSB;
    public static Item sawKeyBathroom;
    public static Item sawBathroom;
    public static Item sawBC;

    // Enemies
    public static EnemyType billyAnnouncementEnemy;
    public static EnemyType billyBathroomEnemy;
    public static EnemyType billyFDEnemy;
    public static EnemyType billyHMEnemy;

    // Prefabs
    public static GameObject puzzleBoardInterface;
    public static GameObject puzzleBoardPrefab;
    public static GameObject puzzlePiecePrefab;
    public static GameObject chainEscapeObj;
    public static GameObject sawBoxExplosiveObj;
    public static GameObject bathroomObj;
    public static GameObject bleedingChainsObj;
    public static GameObject redExplosionParticle;
    public static GameObject bleedingChainsAudio;

    public void Awake()
    {
        mls = BepInEx.Logging.Logger.CreateLogSource("SawTapes");
        configFile = Config;
        ConfigManager.Load();

        LoadManager();
        NetcodePatcher();
        LoadItems();
        LoadEnemies();
        LoadPrefabs();

        harmony.PatchAll(typeof(HUDManagerPatch));
        harmony.PatchAll(typeof(StartOfRoundPatch));
        harmony.PatchAll(typeof(PlayerControllerBPatch));
        harmony.PatchAll(typeof(RoundManagerPatch));
        harmony.PatchAll(typeof(ShovelPatch));
        harmony.PatchAll(typeof(EnemyAIPatch));
    }

    public static void LoadManager()
    {
        Utilities.FixMixerGroups(managerPrefab);
        _ = managerPrefab.AddComponent<SawTapesNetworkManager>();
    }

    private static void NetcodePatcher()
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (Type type in types)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                object[] attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length == 0) continue;
                _ = method.Invoke(null, null);
            }
        }
    }

    public void LoadItems()
    {
        sawTapeValues =
        [
            new SawTapeValue(typeof(SurvivalTape), bundle.LoadAsset<Item>("Assets/SawTape/SurvivalTapeItem.asset"), ConfigManager.SurvivalRarity, ConfigManager.survivalMinPlayers.Value, ConfigManager.survivalMaxPlayers.Value, ConfigManager.survivalInteriorExclusions.Value),
            new SawTapeValue(typeof(HuntingTape), bundle.LoadAsset<Item>("Assets/SawTape/HuntingTapeItem.asset"), ConfigManager.HuntingRarity, ConfigManager.huntingMinPlayers.Value, ConfigManager.huntingMaxPlayers.Value, ConfigManager.huntingInteriorExclusions.Value),
            new SawTapeValue(typeof(EscapeTape), bundle.LoadAsset<Item>("Assets/SawTape/EscapeTapeItem.asset"), ConfigManager.EscapeRarity, 2, 2, ConfigManager.escapeInteriorExclusions.Value),
            new SawTapeValue(typeof(ExplosiveTape), bundle.LoadAsset<Item>("Assets/SawTape/ExplosiveTapeItem.asset"), ConfigManager.ExplosiveRarity, ConfigManager.explosiveMinPlayers.Value, ConfigManager.explosiveMaxPlayers.Value, ConfigManager.explosiveInteriorExclusions.Value)
        ];
        foreach (SawTapeValue sawTapeValue in sawTapeValues) _ = RegisterItem(sawTapeValue.Type, sawTapeValue.Item);

        billyPuppet = RegisterItem(typeof(BillyPuppet), bundle.LoadAsset<Item>("Assets/Billy/Puppet/BillyPuppetItem.asset"));
        billyPuppetJJ = RegisterItem(typeof(BillyPuppetJJ), bundle.LoadAsset<Item>("Assets/Billy/Puppet/BillyPuppetJJItem.asset"));
        billyHead = RegisterItem(typeof(BillyHead), bundle.LoadAsset<Item>("Assets/Billy/Head/BillyHeadItem.asset"));
        billyBody = RegisterItem(typeof(BillyBody), bundle.LoadAsset<Item>("Assets/Billy/Body/BillyBodyItem.asset"));
        sawEscape = RegisterItem(typeof(SawEscape), bundle.LoadAsset<Item>("Assets/Saw/SawEscapeItem.asset"));
        sawBombExplosive = RegisterItem(typeof(SawBombExplosive), bundle.LoadAsset<Item>("Assets/SawBomb/SawBombExplosiveItem.asset"));
        billyPuppetFD = RegisterItem(typeof(BillyPuppetFD), bundle.LoadAsset<Item>("Assets/Billy/Puppet/BillyPuppetFDItem.asset"));
        rBTrapHunting = RegisterItem(typeof(RBTrapHunting), bundle.LoadAsset<Item>("Assets/ReverseBearTrap/RBTrapHuntingItem.asset"));
        sawKeyHunting = RegisterItem(typeof(SawKeyHunting), bundle.LoadAsset<Item>("Assets/SawKey/SawKeyHuntingItem.asset"));
        billyPuppetHunting = RegisterItem(typeof(BillyPuppetHunting), bundle.LoadAsset<Item>("Assets/Billy/Puppet/BillyPuppetHuntingItem.asset"));
        billyPuppetHM = RegisterItem(typeof(BillyPuppetHM), bundle.LoadAsset<Item>("Assets/Billy/Puppet/BillyPuppetHMItem.asset"));
        billyPuppetSurvival = RegisterItem(typeof(BillyPuppetSurvival), bundle.LoadAsset<Item>("Assets/Billy/Puppet/BillyPuppetSurvivalItem.asset"));
        billyPuppetSB = RegisterItem(typeof(BillyPuppetSB), bundle.LoadAsset<Item>("Assets/Billy/Puppet/BillyPuppetSBItem.asset"));
        sawKeyBathroom = RegisterItem(typeof(SawKeyBathroom), bundle.LoadAsset<Item>("Assets/SawKey/SawKeyBathroomItem.asset"));
        sawBathroom = RegisterItem(typeof(SawBathroom), bundle.LoadAsset<Item>("Assets/Saw/SawBathroomItem.asset"));
        sawBC = RegisterItem(typeof(SawBC), bundle.LoadAsset<Item>("Assets/Saw/SawBC.asset"));
    }

    public Item RegisterItem(Type type, Item item)
    {
        if (item.spawnPrefab.GetComponent(type) == null)
        {
            PhysicsProp script = item.spawnPrefab.AddComponent(type) as PhysicsProp;
            script.grabbable = true;
            script.grabbableToEnemies = true;
            script.itemProperties = item;
        }

        NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
        Utilities.FixMixerGroups(item.spawnPrefab);
        Items.RegisterItem(item);

        return item;
    }

    public void LoadEnemies()
    {
        List<EnemyType> enemyTypes =
        [
            (billyAnnouncementEnemy = bundle.LoadAsset<EnemyType>("Assets/Billy/Enemy/BillyAnnouncementEnemy.asset")),
            (billyBathroomEnemy = bundle.LoadAsset<EnemyType>("Assets/Billy/Enemy/BillyBathroomEnemy.asset")),
            (billyFDEnemy = bundle.LoadAsset<EnemyType>("Assets/Billy/Enemy/BillyFDEnemy.asset")),
            (billyHMEnemy = bundle.LoadAsset<EnemyType>("Assets/Billy/Enemy/BillyHMEnemy.asset"))
        ];
        enemyTypes.ForEach(e => NetworkPrefabs.RegisterNetworkPrefab(e.enemyPrefab));
    }

    public static void LoadPrefabs()
    {
        puzzleBoardPrefab = bundle.LoadAsset<GameObject>("Assets/SlidingPuzzle/PuzzleBoard.prefab");
        puzzlePiecePrefab = bundle.LoadAsset<GameObject>("Assets/SlidingPuzzle/PuzzlePiece.prefab");

        List<GameObject> gameObjects =
        [
            (sawBoxExplosiveObj = bundle.LoadAsset<GameObject>("Assets/SawBox/SawBoxExplosive.prefab")),
            (chainEscapeObj = bundle.LoadAsset<GameObject>("Assets/Chain/ChainEscape.prefab")),
            (bathroomObj = bundle.LoadAsset<GameObject>("Assets/Addons/JigsawJudgement/Bathroom.prefab")),
            (bleedingChainsObj = bundle.LoadAsset<GameObject>("Assets/Addons/BleedingChains/BleedingChains.prefab")),
            (redExplosionParticle = bundle.LoadAsset<GameObject>("Assets/Particles/RedExplosionParticle.prefab")),
            (bleedingChainsAudio = bundle.LoadAsset<GameObject>("Assets/Audios/Prefabs/BleedingChainsAudio.prefab"))
        ];

        gameObjects.ForEach(o =>
        {
            NetworkPrefabs.RegisterNetworkPrefab(o);
            Utilities.FixMixerGroups(o);
        });
    }
}
