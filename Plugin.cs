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

namespace SawTapes
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class SawTapes : BaseUnityPlugin
    {
        private const string modGUID = "Lega.SawTapes";
        private const string modName = "Saw Tapes";
        private const string modVersion = "1.0.3";

        private readonly Harmony harmony = new Harmony(modGUID);
        private readonly static AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "sawtapes"));
        internal static ManualLogSource mls;
        public static ConfigFile configFile;

        public static GameObject managerPrefab = NetworkPrefabs.CreateNetworkPrefab("SawTapesNetworkManager");
        public static List<CustomItem> customItems = new List<CustomItem>();

        // Audios sources
        public static GameObject sawTheme;
        public static GameObject sawRecordingSurvival;

        // Particles
        public static GameObject spawnParticle;
        public static GameObject despawnParticle;

        public static List<EnemyAI> allEnemies = new List<EnemyAI>();
        public static List<Tile> eligibleTiles = new List<Tile>();
        public static List<Room> rooms = new List<Room>();
        public static List<Horde> hordes = new List<Horde>();

        public void Awake()
        {
            mls = BepInEx.Logging.Logger.CreateLogSource("SawTapes");
            configFile = Config;
            ConfigManager.Load();

            LoadManager();
            NetcodePatcher();
            LoadItems();
            LoadParticles();
            LoadAudios();

            harmony.PatchAll(typeof(GameNetworkManagerPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(DungeonPatch));
            harmony.PatchAll(typeof(TilePatch));
            harmony.PatchAll(typeof(DoorLockPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));
        }

        public static void LoadManager()
        {
            Utilities.FixMixerGroups(managerPrefab);
            managerPrefab.AddComponent<SawTapesNetworkManager>();
        }

        public static void LoadItems()
        {
            customItems = new List<CustomItem>
            {
                new CustomItem(typeof(SurvivalTape), bundle.LoadAsset<Item>("Assets/SawTape/SawTapeItem.asset"), ConfigManager.survivalRarity.Value, null, true)
            };

            foreach (CustomItem customItem in customItems)
            {
                var script = customItem.Item.spawnPrefab.AddComponent(customItem.Type) as PhysicsProp;
                script.grabbable = true;
                script.grabbableToEnemies = true;
                script.itemProperties = customItem.Item;

                NetworkPrefabs.RegisterNetworkPrefab(customItem.Item.spawnPrefab);
                Utilities.FixMixerGroups(customItem.Item.spawnPrefab);
                Items.RegisterItem(customItem.Item);
            }
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

        public static void LoadParticles()
        {
            spawnParticle = bundle.LoadAsset<GameObject>("Assets/ParticleSystem/SpawnParticle.prefab");
            NetworkPrefabs.RegisterNetworkPrefab(spawnParticle);
            Utilities.FixMixerGroups(spawnParticle);

            despawnParticle = bundle.LoadAsset<GameObject>("Assets/ParticleSystem/DespawnParticle.prefab");
            NetworkPrefabs.RegisterNetworkPrefab(despawnParticle);
            Utilities.FixMixerGroups(despawnParticle);
        }

        public static void LoadAudios()
        {
            sawTheme = bundle.LoadAsset<GameObject>("Assets/Audios/SawTheme.prefab");
            NetworkPrefabs.RegisterNetworkPrefab(sawTheme);
            Utilities.FixMixerGroups(sawTheme);

            sawRecordingSurvival = bundle.LoadAsset<GameObject>("Assets/Audios/SawRecording_Survival.prefab");
            NetworkPrefabs.RegisterNetworkPrefab(sawRecordingSurvival);
            Utilities.FixMixerGroups(sawRecordingSurvival);
        }
    }
}
