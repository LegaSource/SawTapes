using HarmonyLib;
using SawTapes.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Patches
{
    internal class RoundManagerPatch
    {
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPostfix]
        private static void SpawnItems()
            => AddNewItems();

        public static void AddNewItems()
        {
            List<RandomScrapSpawn> listRandomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => !s.spawnUsed).ToList();
            SpawnTape(listRandomScrapSpawn[new System.Random().Next(0, listRandomScrapSpawn.Count)]);
        }

        public static void SpawnTape(RandomScrapSpawn randomScrapSpawn)
        {
            if (randomScrapSpawn == null) return;

            SawTapeValue sawTapeValue = GetSawTapeValue();
            if (sawTapeValue == null) return;

            if (randomScrapSpawn.spawnedItemsCopyPosition) randomScrapSpawn.spawnUsed = true;
            else randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + Vector3.up * sawTapeValue.Item.verticalOffset;

            SpawnItem(sawTapeValue.Item.spawnPrefab, randomScrapSpawn.transform.position + Vector3.up * 0.5f);
        }

        public static SawTapeValue GetSawTapeValue()
        {
            SawTapeValue tapeToSpawn = null;

            STUtilities.Shuffle(SawTapes.sawTapeValues);
            foreach (SawTapeValue sawTapeValue in SawTapes.sawTapeValues)
            {
                if (!string.IsNullOrEmpty(sawTapeValue.InteriorsExclusion) && sawTapeValue.InteriorsExclusion.Contains(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name)) continue;
                if (sawTapeValue.MinPlayers > StartOfRound.Instance.connectedPlayersAmount + 1) continue;

                if (tapeToSpawn == null && new System.Random().Next(1, 100) <= sawTapeValue.Rarity)
                {
                    tapeToSpawn = sawTapeValue;
                    sawTapeValue.Rarity = sawTapeValue.DefaultRarity;
                    continue;
                }
                sawTapeValue.Rarity = Mathf.Min(100, sawTapeValue.Rarity + ConfigManager.rarityIncrement.Value);
            }

            return tapeToSpawn;
        }

        public static GrabbableObject SpawnItem(GameObject spawnPrefab, Vector3 position) => SpawnItem(spawnPrefab, position, Quaternion.identity);

        public static GrabbableObject SpawnItem(GameObject spawnPrefab, Vector3 position, Quaternion rotation)
        {
            if (!GameNetworkManager.Instance.localPlayerController.IsServer && !GameNetworkManager.Instance.localPlayerController.IsHost) return null;

            GrabbableObject grabbableObject = null;
            try
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(spawnPrefab, position, rotation, StartOfRound.Instance.propsContainer);
                grabbableObject = gameObject.GetComponent<GrabbableObject>();
                grabbableObject.fallTime = 0f;
                gameObject.GetComponent<NetworkObject>().Spawn();
            }
            catch (Exception arg)
            {
                SawTapes.mls.LogError($"Error in SpawnItem: {arg}");
            }
            return grabbableObject;
        }
    }
}
