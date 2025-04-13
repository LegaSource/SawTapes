using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using SawTapes.Behaviours;
using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.Tapes;
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
    private const string modGUID = "Lega.SawTapes";
    private const string modName = "Saw Tapes";
    private const string modVersion = "2.0.1";

    private readonly Harmony harmony = new Harmony(modGUID);
    private static readonly AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "sawtapes"));
    internal static ManualLogSource mls;
    public static ConfigFile configFile;

    public static GameObject managerPrefab = NetworkPrefabs.CreateNetworkPrefab("SawTapesNetworkManager");

    // Items
    public static List<SawTapeValue> sawTapeValues = [];
    public static GameObject billyPuppetObj;
    public static GameObject reverseBearTrapObj;
    public static Item sawKey;
    public static Item pursuerEye;
    public static Item sawItem;
    public static GameObject chainObj;
    public static GameObject sawBombObj;

    // Hazards
    public static GameObject sawBoxObj;

    // Audios sources
    public static GameObject sawTheme;
    public static GameObject sawRecordingSurvival;
    public static GameObject sawRecordingHunting;
    public static GameObject sawRecordingEscape;
    public static GameObject sawRecordingExplosive;
    public static GameObject billyRecordingSurvival;
    public static GameObject steamAudio;

    // Particles
    public static GameObject spawnParticle;
    public static GameObject teleportParticle;
    public static GameObject despawnParticle;
    public static GameObject steamParticle;
    public static GameObject pathParticle;

    // Enemies
    public static EnemyType billyEnemy;

    // Shaders
    public static Material wallhackShader;

    public static HashSet<EnemyType> allEnemies = [];

    public void Awake()
    {
        mls = BepInEx.Logging.Logger.CreateLogSource("SawTapes");
        configFile = Config;
        ConfigManager.Load();

        LoadManager();
        NetcodePatcher();
        LoadItems();
        LoadSawBox();
        LoadEnemies();
        LoadParticles();
        LoadAudios();
        LoadShaders();

        harmony.PatchAll(typeof(HUDManagerPatch));
        harmony.PatchAll(typeof(PlayerControllerBPatch));
        harmony.PatchAll(typeof(StartOfRoundPatch));
        harmony.PatchAll(typeof(ManualCameraRendererPatch));
        harmony.PatchAll(typeof(ShipTeleporterPatch));
        harmony.PatchAll(typeof(RoundManagerPatch));
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

        foreach (SawTapeValue sawTapeValue in sawTapeValues)
        {
            _ = RegisterItem(sawTapeValue.Type, sawTapeValue.Item);
        }
        billyPuppetObj = RegisterItem(typeof(BillyPuppet), bundle.LoadAsset<Item>("Assets/BillyPuppet/BillyPuppetItem.asset")).spawnPrefab;
        reverseBearTrapObj = RegisterItem(typeof(ReverseBearTrap), bundle.LoadAsset<Item>("Assets/ReverseBearTrap/ReverseBearTrapItem.asset")).spawnPrefab;
        sawKey = RegisterItem(typeof(SawKey), bundle.LoadAsset<Item>("Assets/SawKey/SawKeyItem.asset"));
        pursuerEye = RegisterItem(typeof(PursuerEye), bundle.LoadAsset<Item>("Assets/PursuerEye/PursuerEyeItem.asset"));
        sawItem = RegisterItem(typeof(Saw), bundle.LoadAsset<Item>("Assets/Saw/SawItem.asset"));
        chainObj = RegisterItem(typeof(Chain), bundle.LoadAsset<Item>("Assets/Chain/ChainItem.asset")).spawnPrefab;
        sawBombObj = RegisterItem(typeof(SawBomb), bundle.LoadAsset<Item>("Assets/SawBomb/SawBombItem.asset")).spawnPrefab;
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

    public void LoadHazards()
        => LoadSawBox();

    public void LoadSawBox()
    {
        sawBoxObj = bundle.LoadAsset<GameObject>("Assets/SawBox/SawBox.prefab");
        NetworkPrefabs.RegisterNetworkPrefab(sawBoxObj);
        Utilities.FixMixerGroups(sawBoxObj);
    }

    public void LoadEnemies()
    {
        billyEnemy = bundle.LoadAsset<EnemyType>("Assets/BillyPuppet/BillyEnemy.asset");
        _ = billyEnemy.enemyPrefab.AddComponent<Billy>();
        NetworkPrefabs.RegisterNetworkPrefab(billyEnemy.enemyPrefab);
    }

    public void LoadParticles()
    {
        HashSet<GameObject> gameObjects =
        [
            (spawnParticle = bundle.LoadAsset<GameObject>("Assets/Particles/SpawnParticle.prefab")),
            (teleportParticle = bundle.LoadAsset<GameObject>("Assets/Particles/TeleportParticle.prefab")),
            (despawnParticle = bundle.LoadAsset<GameObject>("Assets/Particles/DespawnParticle.prefab")),
            (steamParticle = bundle.LoadAsset<GameObject>("Assets/Particles/SteamParticle.prefab")),
            (pathParticle = bundle.LoadAsset<GameObject>("Assets/Particles/PathParticle.prefab"))
        ];

        foreach (GameObject gameObject in gameObjects)
        {
            NetworkPrefabs.RegisterNetworkPrefab(gameObject);
            Utilities.FixMixerGroups(gameObject);
        }
    }

    public void LoadAudios()
    {
        HashSet<GameObject> gameObjects =
        [
            (sawTheme = bundle.LoadAsset<GameObject>("Assets/Audios/Assets/SawTheme.prefab")),
            (sawRecordingSurvival = bundle.LoadAsset<GameObject>("Assets/Audios/Assets/SawRecording_Survival.prefab")),
            (sawRecordingHunting = bundle.LoadAsset<GameObject>("Assets/Audios/Assets/SawRecording_Hunting.prefab")),
            (sawRecordingEscape = bundle.LoadAsset<GameObject>("Assets/Audios/Assets/SawRecording_Escape.prefab")),
            (sawRecordingExplosive = bundle.LoadAsset<GameObject>("Assets/Audios/Assets/SawRecording_Explosive.prefab")),
            (billyRecordingSurvival = bundle.LoadAsset<GameObject>("Assets/Audios/Assets/BillyRecording_Survival.prefab")),
            (steamAudio = bundle.LoadAsset<GameObject>("Assets/Audios/Assets/SteamAudio.prefab"))
        ];

        foreach (GameObject gameObject in gameObjects)
        {
            NetworkPrefabs.RegisterNetworkPrefab(gameObject);
            Utilities.FixMixerGroups(gameObject);
        }
    }

    public static void LoadShaders()
        => wallhackShader = bundle.LoadAsset<Material>("Assets/Shaders/WallhackMaterial.mat");
}
