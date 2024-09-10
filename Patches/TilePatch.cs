using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using SawTapes.Behaviours;
using System.Linq;
using UnityEngine;

namespace SawTapes.Patches
{
    internal class TilePatch
    {
        [HarmonyPatch(typeof(Tile), nameof(Tile.OnTriggerEnter))]
        [HarmonyPrefix]
        private static void EnterTile(ref Tile __instance, Collider other)
        {
            if (ConfigManager.isDebug.Value)
            {
                PlayerControllerB player = other.GetComponent<PlayerControllerB>();
                if (player != null)
                {
                    SawTapes.mls.LogDebug($"Enter in the {__instance.name} tile");
                    SawTapes.mls.LogDebug("Names of the different possible connections for each doorway:");
                    foreach (Doorway doorway in __instance.UsedDoorways)
                    {
                        SawTapes.mls.LogDebug("---");
                        foreach (GameObjectWeight connectorPrefab in doorway.ConnectedDoorway.ConnectorPrefabWeights)
                        {
                            SawTapes.mls.LogDebug("- " + connectorPrefab.GameObject.name);
                        }
                    }
                }
            }

            if (SawTapes.eligibleTiles.Contains(__instance))
            {
                PlayerSTBehaviour playerBehaviour = other.GetComponentInChildren<PlayerSTBehaviour>();
                if (playerBehaviour != null)
                {
                    TileSTBehaviour tileBehaviour = __instance.GetComponent<TileSTBehaviour>();
                    if (tileBehaviour != null)
                    {
                        playerBehaviour.isInGame = true;
                        playerBehaviour.tileGame = __instance;

                        /*foreach (Doorway doorway in __instance.UsedDoorways)
                        {
                            DoorLock doorLock = Object.FindObjectsOfType<DoorLock>()
                                .Where(d => Vector3.Distance(d.transform.position, doorway.transform.position) < 5f)
                                .OrderBy(d => Vector3.Distance(d.transform.position, doorway.transform.position))
                                .FirstOrDefault();

                            if (doorLock != null && doorLock.isDoorOpened)
                            {
                                if (doorLock.gameObject.TryGetComponent<AnimatedObjectTrigger>(out var triggerAnimation))
                                {
                                    triggerAnimation.TriggerAnimationNonPlayer(playSecondaryAudios: true, overrideBool: true);
                                    if (!triggerAnimation.boolValue)
                                    {
                                        doorLock.CloseDoorNonPlayerServerRpc();
                                    }
                                }
                            }
                            if (!DoorLockPatch.blockedDoors.ContainsKey(doorLock))
                            {
                                DoorLockPatch.blockedDoors.Add(doorLock, __instance);
                            }
                        }*/
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
                                DoorLockPatch.blockedDoors.Add(doorLock, __instance);
                            }
                        }
                        foreach (EntranceTeleport entranceTeleport in tileBehaviour.entranceTeleports)
                        {
                            if (!HUDManagerPatch.blockedEntrances.ContainsKey(entranceTeleport))
                            {
                                HUDManagerPatch.blockedEntrances.Add(entranceTeleport, __instance);
                            }
                        }
                    }
                }
            }
        }
    }
}
