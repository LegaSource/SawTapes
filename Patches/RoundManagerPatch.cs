using DunGen;
using HarmonyLib;
using SawTapes.Behaviours;
using SawTapes.Managers;
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
        public static SawTapeValue tapeToSpawn;

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPostfix]
        private static void LoadNewGame(ref RoundManager __instance)
        {
            SawTapes.eligibleTiles.Clear();
            tapeToSpawn = null;
            if (__instance.IsServer)
            {
                STUtilities.Shuffle(SawTapes.sawTapeValues);
                foreach (SawTapeValue sawTapeValue in SawTapes.sawTapeValues)
                {
                    if (tapeToSpawn == null && new System.Random().Next(1, 100) <= sawTapeValue.Rarity)
                    {
                        tapeToSpawn = sawTapeValue;
                        sawTapeValue.Rarity = sawTapeValue.DefaultRarity;
                    }
                    else if (sawTapeValue.Rarity > 0 && sawTapeValue.Rarity < 100)
                    {
                        sawTapeValue.Rarity = Mathf.Min(100, sawTapeValue.Rarity + ConfigManager.rarityIncrement.Value);
                    }
                }
                SawTapesNetworkManager.Instance.SetGenerateGameTileClientRpc(tapeToSpawn != null && tapeToSpawn.IsTileGame);
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPostfix]
        private static void SpawnItems(ref RoundManager __instance) => AddNewItems(ref __instance);

        public static void AddNewItems(ref RoundManager roundManager)
        {
            if (tapeToSpawn != null)
            {
                if (tapeToSpawn.IsTileGame)
                {
                    STUtilities.Shuffle(SawTapes.eligibleTiles);
                    foreach (Tile tile in SawTapes.eligibleTiles.ToList())
                    {
                        List<DoorLock> doorLocks = new List<DoorLock>();
                        bool noDoor = false;
                        foreach (Doorway doorway in tile.UsedDoorways)
                        {
                            Vector3 doorPosition = doorway.transform.position;
                            DoorLock doorLock = UnityEngine.Object.FindObjectsOfType<DoorLock>()
                                .Where(d => Vector3.Distance(d.transform.position, doorway.transform.position) < 5f)
                                .OrderBy(d => Vector3.Distance(d.transform.position, doorway.transform.position))
                                .FirstOrDefault();

                            if (doorLock == null)
                            {
                                noDoor = true;
                                break;
                            }
                            else
                            {
                                doorLocks.Add(doorLock);
                            }
                        }
                        if (noDoor)
                        {
                            SawTapes.eligibleTiles.Remove(tile);
                            continue;
                        }

                        List<RandomScrapSpawn> listRandomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => !s.spawnUsed && tile.Bounds.Contains(s.transform.position)).ToList();
                        if (listRandomScrapSpawn.Count <= 0)
                        {
                            SawTapes.eligibleTiles.Remove(tile);
                            continue;
                        }
                        Vector3 position = GetRandomSpawnItemPosition(listRandomScrapSpawn, roundManager, tapeToSpawn.Item);
                        GrabbableObject grabbableObject = SpawnItem(ref tapeToSpawn.Item.spawnPrefab, position);

                        // Ajout des infos pour les portes et entrées/sorties liées dans le Tile - permet d'éviter le freeze lors de l'accès à une salle de jeu
                        Vector3[] doorsPos = doorLocks.Select(d => d.transform.position).ToArray();
                        List<NetworkObjectReference> entranceTeleports = new List<NetworkObjectReference>();
                        foreach (EntranceTeleport entranceTeleport in UnityEngine.Object.FindObjectsOfType<EntranceTeleport>().Where(e => tile.Bounds.Contains(e.transform.position)))
                        {
                            NetworkObject entranceObject = entranceTeleport.GetComponent<NetworkObject>();
                            if (entranceObject != null)
                            {
                                entranceTeleports.Add(entranceObject);
                                foreach (EntranceTeleport exitTeleport in UnityEngine.Object.FindObjectsOfType<EntranceTeleport>().Where(e => e.isEntranceToBuilding != entranceTeleport.isEntranceToBuilding && e.entranceId == entranceTeleport.entranceId))
                                {
                                    NetworkObject exitObject = exitTeleport.GetComponent<NetworkObject>();
                                    if (exitObject != null)
                                    {
                                        entranceTeleports.Add(exitObject);
                                    }
                                }
                            }
                        }
                        SawTapesNetworkManager.Instance.AddTileInfosClientRpc(tile.transform.position, doorsPos, entranceTeleports.ToArray(), grabbableObject.GetComponent<NetworkObject>());
                        SawTapes.eligibleTiles.RemoveAll(t => t != tile);
                        break;
                    }
                }
                else
                {
                    Vector3 position = GetRandomSpawnItemPosition(UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => !s.spawnUsed).ToList(), roundManager, tapeToSpawn.Item);
                    GrabbableObject grabbableObject = SpawnItem(ref tapeToSpawn.Item.spawnPrefab, position);
                }
            }
        }

        public static Vector3 GetRandomSpawnItemPosition(List<RandomScrapSpawn> listRandomScrapSpawn, RoundManager roundManager, Item item)
        {
            System.Random random = new System.Random();
            int indexRandomScrapSpawn = random.Next(0, listRandomScrapSpawn.Count);
            RandomScrapSpawn randomScrapSpawn = listRandomScrapSpawn[indexRandomScrapSpawn];
            if (randomScrapSpawn.spawnedItemsCopyPosition)
            {
                randomScrapSpawn.spawnUsed = true;
                listRandomScrapSpawn.RemoveAt(indexRandomScrapSpawn);
            }
            else
            {
                randomScrapSpawn.transform.position = roundManager.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, roundManager.navHit, roundManager.AnomalyRandom) + Vector3.up * item.verticalOffset;
            }
            return randomScrapSpawn.transform.position + Vector3.up * 0.5f;
        }

        public static GrabbableObject SpawnItem(ref GameObject spawnPrefab, Vector3 position) => SpawnItem(ref spawnPrefab, position, Quaternion.identity);

        public static GrabbableObject SpawnItem(ref GameObject spawnPrefab, Vector3 position, Quaternion rotation)
        {
            if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost)
            {
                try
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(spawnPrefab, position, rotation, StartOfRound.Instance.propsContainer);
                    GrabbableObject grabbableObject = gameObject.GetComponent<GrabbableObject>();
                    grabbableObject.fallTime = 0f;
                    gameObject.GetComponent<NetworkObject>().Spawn();

                    if (grabbableObject is SawTape)
                    {
                        SawTapesNetworkManager.Instance.SpawnTapeParticleClientRpc(grabbableObject.GetComponent<NetworkObject>());
                    }

                    return grabbableObject;
                }
                catch (Exception arg)
                {
                    SawTapes.mls.LogError($"Error in SpawnItem: {arg}");
                }
            }
            return null;
        }
    }
}
