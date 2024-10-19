using DunGen;
using SawTapes.Behaviours;
using SawTapes.Patches;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    internal class TileSTManager
    {
        public static void UnlockDoors(int playerId)
        {
            TileSTBehaviour tileSTBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>().tileGame.GetComponent<TileSTBehaviour>();
            if (tileSTBehaviour != null)
            {
                foreach (DoorLock doorLock in tileSTBehaviour.doorLocks)
                {
                    if (!doorLock.isLocked)
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
    }
}
