using DunGen;
using GameNetcodeStuff;
using SawTapes.Behaviours;
using SawTapes.Patches;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace SawTapes.Managers
{
    public class TileSTManager
    {
        public static void UnlockDoors(ref TileSTBehaviour tileSTBehaviour)
        {
            if (tileSTBehaviour != null)
            {
                foreach (DoorLock doorLock in tileSTBehaviour.doorLocks)
                {
                    if (doorLock != null && doorLock.gameObject != null && !doorLock.isLocked)
                    {
                        if (doorLock.gameObject.TryGetComponent<AnimatedObjectTrigger>(out var triggerAnimation))
                        {
                            triggerAnimation.TriggerAnimationNonPlayer(playSecondaryAudios: false, overrideBool: true);
                            doorLock.OpenDoorAsEnemyServerRpc();
                        }
                    }
                    DoorLockPatch.blockedDoors.Remove(doorLock);
                }
                foreach (EntranceTeleport entranceTeleport in tileSTBehaviour.entranceTeleports)
                {
                    HUDManagerPatch.blockedEntrances.Remove(entranceTeleport);
                }
            }
        }

        public static void AddTileInfos(Vector3 tilePos, Vector3[] doorsPos, NetworkObjectReference[] entrancesObj, NetworkObjectReference obj)
        {
            Tile tile = Object.FindObjectsOfType<Tile>().FirstOrDefault(t => t.transform.position == tilePos);
            if (tile != null)
            {
                TileSTBehaviour tileBehaviour = tile.GetComponent<TileSTBehaviour>() ?? tile.gameObject.AddComponent<TileSTBehaviour>();
                foreach (Vector3 doorPos in doorsPos)
                {
                    DoorLock doorLock;
                    // Spécifique StarlancerWarehouse
                    if (RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name.Equals("SciFiWarehouseDungeonFlow"))
                    {
                        doorLock = Object.FindObjectsOfType<DoorLock>().FirstOrDefault(d => d.transform.position == doorPos && d.name.Equals("SciFiWarehouseDoor"));
                    }
                    // Autres
                    else
                    {
                        doorLock = Object.FindObjectsOfType<DoorLock>().FirstOrDefault(t => t.transform.position == doorPos);
                    }

                    if (doorLock != null)
                    {
                        tileBehaviour.doorLocks.Add(doorLock);
                    }
                    else
                    {
                        SawTapes.mls.LogWarning("DoorLock not found during the creation of the game room.");
                    }
                }
                foreach (NetworkObjectReference entranceObj in entrancesObj)
                {
                    if (entranceObj.TryGet(out var networkObjectEntrance))
                    {
                        EntranceTeleport entranceTeleport = networkObjectEntrance.GetComponent<EntranceTeleport>();
                        if (entranceTeleport != null)
                        {
                            tileBehaviour.entranceTeleports.Add(entranceTeleport);
                        }
                        else
                        {
                            SawTapes.mls.LogWarning("EntranceTeleport not found during the creation of the game room.");
                        }
                    }
                }
                if (obj.TryGet(out var networkObject))
                {
                    GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                    if (grabbableObject is SawTape sawTape)
                    {
                        tileBehaviour.sawTape = sawTape;
                    }
                }
            }
            else
            {
                SawTapes.mls.LogWarning("Tile not found during the creation of the game room");
            }
        }

        public static void LogTileDebugInfo(ref Tile tile, ref Collider collider)
        {
            if (ConfigManager.isDebug.Value)
            {
                PlayerControllerB player = collider.GetComponent<PlayerControllerB>();
                if (player != null)
                {
                    SawTapes.mls.LogDebug($"Enter in the {tile.name} tile");
                    SawTapes.mls.LogDebug("Names of the different possible connections for each doorway:");
                    foreach (Doorway doorway in tile.UsedDoorways)
                    {
                        SawTapes.mls.LogDebug("---");
                        if (doorway.ConnectedDoorway.ConnectorPrefabWeights.Any())
                        {
                            foreach (GameObjectWeight connectorPrefab in doorway.ConnectedDoorway.ConnectorPrefabWeights)
                            {
                                SawTapes.mls.LogDebug("- " + connectorPrefab.GameObject.name);
                            }
                        }
                        else
                        {
                            SawTapes.mls.LogDebug("- None");
                        }
                    }
                }
            }
        }

        public static void HandleTileBehaviour(ref Tile tile, ref Collider collider)
        {
            if (SawTapes.eligibleTiles.Contains(tile))
            {
                PlayerSTBehaviour playerBehaviour = collider.GetComponentInChildren<PlayerSTBehaviour>();
                if (playerBehaviour != null)
                {
                    TileSTBehaviour tileBehaviour = tile.GetComponent<TileSTBehaviour>();
                    if (tileBehaviour != null && !IsPlayerInGame(tile))
                    {
                        if (ConfigManager.isInfoInGame.Value && GameNetworkManager.Instance.localPlayerController == playerBehaviour.playerProperties)
                        {
                            HUDManager.Instance.DisplayTip(Constants.INFORMATION, Constants.MESSAGE_INFO_LOCKED);
                        }

                        playerBehaviour.isInGame = true;
                        playerBehaviour.tileGame = tile;
                        SawTapesNetworkManager.Instance.TapeSearchServerRpc((int)playerBehaviour.playerProperties.playerClientId);
                        MapCameraSTManager.UpdateMapCamera(ref StartOfRound.Instance.mapScreen);

                        foreach (DoorLock doorLock in tileBehaviour.doorLocks)
                        {
                            if (doorLock.isDoorOpened && doorLock.gameObject.TryGetComponent<AnimatedObjectTrigger>(out var triggerAnimation))
                            {
                                triggerAnimation.TriggerAnimationNonPlayer(playSecondaryAudios: true, overrideBool: true);
                                if (!triggerAnimation.boolValue)
                                {
                                    doorLock.CloseDoorNonPlayerServerRpc();
                                }
                            }
                            if (!DoorLockPatch.blockedDoors.ContainsKey(doorLock))
                            {
                                DoorLockPatch.blockedDoors.Add(doorLock, tile);
                            }
                        }
                        foreach (EntranceTeleport entranceTeleport in tileBehaviour.entranceTeleports)
                        {
                            if (!HUDManagerPatch.blockedEntrances.ContainsKey(entranceTeleport))
                            {
                                HUDManagerPatch.blockedEntrances.Add(entranceTeleport, tile);
                            }
                        }
                    }
                }
            }
        }

        public static bool IsPlayerInGame(Tile tile)
        {
            PlayerSTBehaviour playerSTBehaviour;
            return StartOfRound.Instance.allPlayerScripts.Any(p => (playerSTBehaviour = p.GetComponent<PlayerSTBehaviour>()) != null && playerSTBehaviour.isInGame && playerSTBehaviour.tileGame == tile);
        }

        public static Vector3 GetRandomNavMeshPositionInTile(ref PlayerSTBehaviour playerBehaviour)
        {
            float padding = 3.0f;
            float heightTolerance = 1.0f;
            float randomX = Random.Range(playerBehaviour.tileGame.Bounds.min.x + padding, playerBehaviour.tileGame.Bounds.max.x - padding);
            float randomY = Random.Range(playerBehaviour.playerProperties.transform.position.y + heightTolerance, playerBehaviour.playerProperties.transform.position.y - heightTolerance);
            float randomZ = Random.Range(playerBehaviour.tileGame.Bounds.min.z + padding, playerBehaviour.tileGame.Bounds.max.z - padding);

            Vector3 randomPosition = new Vector3(randomX, randomY, randomZ);

            // Vérifier si la position est valide sur le NavMesh
            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit navHit, Mathf.Max(playerBehaviour.tileGame.Bounds.size.x, playerBehaviour.tileGame.Bounds.size.z), 1))
            {
                return navHit.position;
            }
            return randomPosition;
        }
    }
}
