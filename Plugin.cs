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

namespace SawTapes
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class SawTapes : BaseUnityPlugin
    {
        private const string modGUID = "Lega.SawTapes";
        private const string modName = "Saw Tapes";
        private const string modVersion = "1.1.3";

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

        // Audios sources
        public static GameObject sawTheme;
        public static GameObject sawRecordingSurvival;
        public static GameObject sawRecordingHunting;
        public static GameObject billyRecordingSurvival;
        public static GameObject steamAudio;

        // Particles
        public static GameObject tapeParticle;
        public static GameObject spawnParticle;
        public static GameObject despawnParticle;
        public static GameObject steamParticle;

        // Enemies
        public static EnemyType billyEnemy;

        // Shaders
        public static Material wallhackShader;

        public static HashSet<EnemyType> allEnemies = new HashSet<EnemyType>();
        public static List<Tile> eligibleTiles = new List<Tile>();
        public static HashSet<Room> rooms = new HashSet<Room>();
        public static HashSet<Horde> hordes = new HashSet<Horde>();

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
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

        public void LoadItems()
        {
            sawTapeValues = new List<SawTapeValue>
            {
                new SawTapeValue(typeof(SurvivalTape), bundle.LoadAsset<Item>("Assets/SawTape/SurvivalTapeItem.asset"), ConfigManager.survivalRarity.Value, true),
                new SawTapeValue(typeof(HuntingTape), bundle.LoadAsset<Item>("Assets/SawTape/HuntingTapeItem.asset"), ConfigManager.huntingRarity.Value, false)
            };

            foreach (SawTapeValue sawTapeValue in sawTapeValues)
            {
                RegisterItem(sawTapeValue.Type, sawTapeValue.Item);
            }
            billyPuppetObj = RegisterItem(typeof(BillyPuppet), bundle.LoadAsset<Item>("Assets/BillyPuppet/BillyPuppetItem.asset"));
            reverseBearTrapObj = RegisterItem(typeof(ReverseBearTrap), bundle.LoadAsset<Item>("Assets/ReverseBearTrap/ReverseBearTrapItem.asset"));
            sawKeyObj = RegisterItem(typeof(SawKey), bundle.LoadAsset<Item>("Assets/SawKey/SawKeyItem.asset"));
            pursuerEyeObj = RegisterItem(typeof(PursuerEye), bundle.LoadAsset<Item>("Assets/PursuerEye/PursuerEyeItem.asset"));
        }

        public GameObject RegisterItem(Type type, Item item)
        {
            PhysicsProp script = item.spawnPrefab.AddComponent(type) as PhysicsProp;
            script.grabbable = true;
            script.grabbableToEnemies = true;
            script.itemProperties = item;

            NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
            Utilities.FixMixerGroups(item.spawnPrefab);
            Items.RegisterItem(item);

            return item.spawnPrefab;
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
                (steamParticle = bundle.LoadAsset<GameObject>("Assets/Particles/SteamParticle.prefab"))
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
        {
            wallhackShader = bundle.LoadAsset<Material>("Assets/Shaders/WallhackMaterial.mat");
        }
    }
}
