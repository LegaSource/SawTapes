using DunGen;
using HarmonyLib;
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
        public static List<Item> tileGameTapes = new List<Item>();

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPostfix]
        private static void LoadNewGame(ref RoundManager __instance)
        {
            SawTapes.eligibleTiles.Clear();
            tileGameTapes.Clear();
            if (__instance.IsServer)
            {
                foreach (CustomItem customItem in SawTapes.customItems.Where(i => i.IsTileGame && new System.Random().Next(1, 100) <= i.Rarity))
                {
                    tileGameTapes.Add(customItem.Item);
                }
                SawTapesNetworkManager.Instance.SetGenerateGameTileClientRpc(tileGameTapes.Count() > 0);
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPostfix]
        private static void SpawnItems(ref RoundManager __instance)
        {
            AddNewItems(ref __instance);
        }

        private static void AddNewItems(ref RoundManager roundManager)
        {
            List<Tile> usedTiles = new List<Tile>();
            foreach (Item item in tileGameTapes)
            {
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
                    Vector3 position = randomScrapSpawn.transform.position + Vector3.up * 0.5f;
                    SpawnItem(ref item.spawnPrefab, ref position);

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
                    SawTapesNetworkManager.Instance.AddTileInfosClientRpc(tile.transform.position, doorsPos, entranceTeleports.ToArray());

                    usedTiles.Add(tile);
                    break;
                }
            }
            SawTapes.eligibleTiles.RemoveWhere(t => !usedTiles.Contains(t));
        }

        public static GrabbableObject SpawnItem(ref GameObject spawnPrefab, ref Vector3 position)
        {
            if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost)
            {
                try
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(spawnPrefab, position, Quaternion.identity, StartOfRound.Instance.propsContainer);
                    GrabbableObject scrap = gameObject.GetComponent<GrabbableObject>();
                    scrap.fallTime = 0f;
                    gameObject.GetComponent<NetworkObject>().Spawn();
                    return scrap;
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
