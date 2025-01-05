using BepInEx.Configuration;
using BepInEx;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using LethalLib.Modules;
using BepInEx.Logging;
using SawTapes.Patches;
using System.Collections.Generic;
using SawTapes.Managers;
using SawTapes.Behaviours;
using DunGen;
using SawTapes.Values;
using System;
using SawTapes.Behaviours.Tapes;
using SawTapes.Behaviours.Items;

namespace SawTapes
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class SawTapes : BaseUnityPlugin
    {
        private const string modGUID = "Lega.SawTapes";
        private const string modName = "Saw Tapes";
        private const string modVersion = "1.1.6";

        private readonly Harmony harmony = new Harmony(modGUID);
        private readonly static AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "sawtapes"));
        internal static ManualLogSource mls;
        public static ConfigFile configFile;

        public static GameObject managerPrefab = NetworkPrefabs.CreateNetworkPrefab("SawTapesNetworkManager");

        // Items
        public static List<SawTapeValue> sawTapeValues = new List<SawTapeValue>();
        public static GameObject billyPuppetObj;
        public static GameObject reverseBearTrapObj;
        public static GameObject sawKeyObj;
        public static GameObject pursuerEyeObj;
        public static Item sawItem;
        public static GameObject chainObj;

        // Audios sources
        public static GameObject sawTheme;
        public static GameObject sawRecordingSurvival;
        public static GameObject sawRecordingHunting;
        public static GameObject sawRecordingEscape;
        public static GameObject billyRecordingSurvival;
        public static GameObject steamAudio;

        // Particles
        public static GameObject tapeParticle;
        public static GameObject spawnParticle;
        public static GameObject despawnParticle;
        public static GameObject steamParticle;
        public static GameObject pathParticle;

        // Enemies
        public static EnemyType billyEnemy;

        // Shaders
        public static Material wallhackShader;

        public static HashSet<EnemyType> allEnemies = new HashSet<EnemyType>();
        public static List<Tile> eligibleTiles = new List<Tile>();
        public static HashSet<SurvivalRoom> rooms = new HashSet<SurvivalRoom>();
        public static HashSet<SurvivalHorde> hordes = new HashSet<SurvivalHorde>();

        public void Awake()
        {
            mls = BepInEx.Logging.Logger.CreateLogSource("SawTapes");
            configFile = Config;
            ConfigManager.Load();

            LoadManager();
            NetcodePatcher();
            LoadItems();
            LoadEnemies();
            LoadParticles();
            LoadAudios();
            LoadShaders();

            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(DungeonPatch));
            harmony.PatchAll(typeof(TilePatch));
            harmony.PatchAll(typeof(DoorLockPatch));
            harmony.PatchAll(typeof(ManualCameraRendererPatch));
            harmony.PatchAll(typeof(ShipTeleporterPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(EnemyAIPatch));
        }

        public static void LoadManager()
        {
            Utilities.FixMixerGroups(managerPrefab);
            managerPrefab.AddComponent<SawTapesNetworkManager>();
        }

        private static void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                        method.Invoke(null, null);
                }
            }
        }

        public void LoadItems()
        {
            sawTapeValues = new List<SawTapeValue>
            {
                new SawTapeValue(typeof(SurvivalTape), bundle.LoadAsset<Item>("Assets/SawTape/SurvivalTapeItem.asset"), ConfigManager.SurvivalRarity, true, ConfigManager.survivalInteriorExclusions.Value),
                new SawTapeValue(typeof(HuntingTape), bundle.LoadAsset<Item>("Assets/SawTape/HuntingTapeItem.asset"), ConfigManager.HuntingRarity, false, ConfigManager.huntingInteriorExclusions.Value),
                new SawTapeValue(typeof(EscapeTape), bundle.LoadAsset<Item>("Assets/SawTape/EscapeTapeItem.asset"), ConfigManager.EscapeRarity, false, ConfigManager.escapeInteriorExclusions.Value)
            };

            foreach (SawTapeValue sawTapeValue in sawTapeValues)
            {
                RegisterItem(sawTapeValue.Type, sawTapeValue.Item);
            }
            billyPuppetObj = RegisterItem(typeof(BillyPuppet), bundle.LoadAsset<Item>("Assets/BillyPuppet/BillyPuppetItem.asset")).spawnPrefab;
            reverseBearTrapObj = RegisterItem(typeof(ReverseBearTrap), bundle.LoadAsset<Item>("Assets/ReverseBearTrap/ReverseBearTrapItem.asset")).spawnPrefab;
            sawKeyObj = RegisterItem(typeof(SawKey), bundle.LoadAsset<Item>("Assets/SawKey/SawKeyItem.asset")).spawnPrefab;
            pursuerEyeObj = RegisterItem(typeof(PursuerEye), bundle.LoadAsset<Item>("Assets/PursuerEye/PursuerEyeItem.asset")).spawnPrefab;
            sawItem = RegisterItem(typeof(Saw), bundle.LoadAsset<Item>("Assets/Saw/SawItem.asset"));
            chainObj = RegisterItem(typeof(Chain), bundle.LoadAsset<Item>("Assets/Chain/ChainItem.asset")).spawnPrefab;
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
            billyEnemy = bundle.LoadAsset<EnemyType>("Assets/BillyPuppet/BillyEnemy.asset");
            billyEnemy.enemyPrefab.AddComponent<Billy>();
            NetworkPrefabs.RegisterNetworkPrefab(billyEnemy.enemyPrefab);
        }

        public void LoadParticles()
        {
            HashSet<GameObject> gameObjects = new HashSet<GameObject>
            {
                (tapeParticle = bundle.LoadAsset<GameObject>("Assets/Particles/TapeParticle.prefab")),
                (spawnParticle = bundle.LoadAsset<GameObject>("Assets/Particles/SpawnParticle.prefab")),
                (despawnParticle = bundle.LoadAsset<GameObject>("Assets/Particles/DespawnParticle.prefab")),
                (steamParticle = bundle.LoadAsset<GameObject>("Assets/Particles/SteamParticle.prefab")),
                (pathParticle = bundle.LoadAsset<GameObject>("Assets/Particles/PathParticle.prefab"))
            };

            foreach (GameObject gameObject in gameObjects)
            {
                NetworkPrefabs.RegisterNetworkPrefab(gameObject);
                Utilities.FixMixerGroups(gameObject);
            }
        }

        public void LoadAudios()
        {
            HashSet<GameObject> gameObjects = new HashSet<GameObject>
            {
                (sawTheme = bundle.LoadAsset<GameObject>("Assets/Audios/SawTheme.prefab")),
                (sawRecordingSurvival = bundle.LoadAsset<GameObject>("Assets/Audios/SawRecording_Survival.prefab")),
                (sawRecordingHunting = bundle.LoadAsset<GameObject>("Assets/Audios/SawRecording_Hunting.prefab")),
                (sawRecordingEscape = bundle.LoadAsset<GameObject>("Assets/Audios/SawRecording_Escape.prefab")),
                (billyRecordingSurvival = bundle.LoadAsset<GameObject>("Assets/Audios/BillyRecording_Survival.prefab")),
                (steamAudio = bundle.LoadAsset<GameObject>("Assets/Audios/SteamAudio.prefab"))
            };

            foreach (GameObject gameObject in gameObjects)
            {
                NetworkPrefabs.RegisterNetworkPrefab(gameObject);
                Utilities.FixMixerGroups(gameObject);
            }
        }

        public static void LoadShaders()
            => wallhackShader = bundle.LoadAsset<Material>("Assets/Shaders/WallhackMaterial.mat");
    }
}
