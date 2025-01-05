using DunGen;
using HarmonyLib;
using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.Tapes;
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

        [HarmonyPatch(typeof(RuntimeDungeon), nameof(RuntimeDungeon.Generate))]
        [HarmonyPrefix]
        private static void LoadNewGame()
        {
            SawTapes.eligibleTiles.Clear();
            tapeToSpawn = null;

            if (!RoundManager.Instance.IsServer) return;

            STUtilities.Shuffle(SawTapes.sawTapeValues);
            foreach (SawTapeValue sawTapeValue in SawTapes.sawTapeValues.Where(s => string.IsNullOrEmpty(s.InteriorsExclusion) || !s.InteriorsExclusion.Contains(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name)))
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

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPostfix]
        private static void SpawnItems() => AddNewItems();

        public static void AddNewItems()
        {
            if (tapeToSpawn == null) return;

            List<RandomScrapSpawn> listRandomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => !s.spawnUsed).ToList();
            if (tapeToSpawn.IsTileGame)
            {
                STUtilities.Shuffle(SawTapes.eligibleTiles);
                foreach (Tile tile in SawTapes.eligibleTiles.ToList())
                {
                    SurvivalRoom room = SawTapes.rooms.FirstOrDefault(r => tile.name.Equals(r.RoomName));
                    List<DoorLock> doorLocks = new List<DoorLock>();

                    bool IsDoorwayValid(Doorway doorway)
                    {
                        if (doorway.doorComponent == null) return false;
                        if (!doorway.ConnectedDoorway.ConnectorPrefabWeights.Any()) return false;
                        if (!room.DoorsNames.Contains(doorway.doorComponent.name.Replace("(Clone)", "").Trim())) return false;

                        DoorLock doorLock = UnityEngine.Object.FindObjectsOfType<DoorLock>()
                            .Where(d => Vector3.Distance(d.transform.position, doorway.doorComponent.transform.position) < 2f)
                            .OrderBy(d => Vector3.Distance(d.transform.position, doorway.doorComponent.transform.position))
                            .FirstOrDefault();

                        if (doorLock != null)
                            doorLocks.Add(doorLock);
                        return true;
                    }

                    if (!tile.UsedDoorways.All(IsDoorwayValid))
                    {
                        SawTapes.eligibleTiles.RemoveAll(t => t == tile);
                        continue;
                    }

                    // On ne prend que les spawns dans la room
                    List<RandomScrapSpawn> listRandomScrapTile = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => tile.Bounds.Contains(s.transform.position)).ToList();
                    if (listRandomScrapTile.Count <= 0)
                    {
                        // Au cas où tous les spawn de la room ont été utilisés
                        listRandomScrapTile = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => tile.Bounds.Contains(s.transform.position)).ToList();
                        if (listRandomScrapTile.Count <= 0)
                        {
                            SawTapes.eligibleTiles.RemoveAll(t => t == tile);
                            continue;
                        }
                    }
                    // Spawn de la tape à la position la plus proche du centre de la room
                    GrabbableObject grabbableObject = SpawnTape(listRandomScrapTile.OrderBy(s => Vector3.Distance(s.transform.position, tile.Bounds.center)).FirstOrDefault());

                    // Ajout des infos pour les portes et entrées/sorties liées dans le Tile - permet d'éviter le freeze lors de l'accès à une salle de jeu
                    List<NetworkObjectReference> entranceTeleports = entranceTeleports = CollectEntranceAndExitTeleports(tile);
                    Vector3[] doorsPos = doorLocks.Select(d => d.transform.position).ToArray();
                    SawTapesNetworkManager.Instance.AddTileInfosClientRpc(tile.transform.position, doorsPos, entranceTeleports.ToArray(), grabbableObject.GetComponent<NetworkObject>());

                    // Suppression des autres rooms pour ne garder que celle-ci
                    SawTapes.eligibleTiles.RemoveAll(t => t != tile);
                    break;
                }
            }
            else
            {
                int indexRandomScrapSpawn = new System.Random().Next(0, listRandomScrapSpawn.Count);
                SpawnTape(listRandomScrapSpawn[indexRandomScrapSpawn]);
            }
        }

        public static List<NetworkObjectReference> CollectEntranceAndExitTeleports(Tile tile)
        {
            List<NetworkObjectReference> entranceTeleports = new List<NetworkObjectReference>();
            foreach (EntranceTeleport entranceTeleport in UnityEngine.Object.FindObjectsOfType<EntranceTeleport>()
                .Where(e => tile.Bounds.Contains(e.transform.position)))
            {
                NetworkObject entranceObject = entranceTeleport.GetComponent<NetworkObject>();
                if (entranceObject != null)
                {
                    entranceTeleports.Add(entranceObject);
                    foreach (EntranceTeleport exitTeleport in UnityEngine.Object.FindObjectsOfType<EntranceTeleport>()
                        .Where(e => e.isEntranceToBuilding != entranceTeleport.isEntranceToBuilding && e.entranceId == entranceTeleport.entranceId))
                    {
                        NetworkObject exitObject = exitTeleport.GetComponent<NetworkObject>();
                        if (exitObject != null)
                            entranceTeleports.Add(exitObject);
                    }
                }
            }
            return entranceTeleports;
        }

        public static GrabbableObject SpawnTape(RandomScrapSpawn randomScrapSpawn)
        {
            if (randomScrapSpawn == null) return null;

            if (randomScrapSpawn.spawnedItemsCopyPosition)
                randomScrapSpawn.spawnUsed = true;
            else
                randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + Vector3.up * tapeToSpawn.Item.verticalOffset;

            return SpawnItem(tapeToSpawn.Item.spawnPrefab, randomScrapSpawn.transform.position + Vector3.up * 0.5f);
        }

        public static GrabbableObject SpawnItem(GameObject spawnPrefab, Vector3 position) => SpawnItem(spawnPrefab, position, Quaternion.identity);

        public static GrabbableObject SpawnItem(GameObject spawnPrefab, Vector3 position, Quaternion rotation)
        {
            if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost)
            {
                try
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(spawnPrefab, position, rotation, StartOfRound.Instance.propsContainer);
                    GrabbableObject grabbableObject = gameObject.GetComponent<GrabbableObject>();
                    grabbableObject.fallTime = 0f;
                    gameObject.GetComponent<NetworkObject>().Spawn();

                    if (grabbableObject is SawTape || grabbableObject is Saw)
                        SawTapesNetworkManager.Instance.SpawnBlackParticleClientRpc(grabbableObject.GetComponent<NetworkObject>());

                    return grabbableObject;
                }
                catch (Exception arg)
                {
                    SawTapes.mls.LogError($"Error in SpawnItem: {arg}");
                }
            }
            return null;
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DetectElevatorIsRunning))]
        [HarmonyPostfix]
        private static void EndGame()
        {
            foreach (SawTape sawTape in UnityEngine.Object.FindObjectsOfType<SawTape>())
                SawTapesNetworkManager.Instance.DestroyObjectServerRpc(sawTape.GetComponent<NetworkObject>());
        }
    }
}
