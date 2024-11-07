using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;

namespace SawTapes.Patches
{
    internal class DoorLockPatch
    {
        public static Dictionary<DoorLock, Tile> blockedDoors = new Dictionary<DoorLock, Tile>();

        [HarmonyPatch(typeof(DoorLock), nameof(DoorLock.OnTriggerStay))]
        [HarmonyPrefix]
        private static bool OpenDoorAsEnemy(ref DoorLock __instance) => !IsDoorBlocked(ref __instance);

        [HarmonyPatch(typeof(DoorLock), nameof(DoorLock.TryDoorHaunt))]
        [HarmonyPrefix]
        private static bool OpenDoorAsHaunt(ref DoorLock __instance) => !IsDoorBlocked(ref __instance);

        [HarmonyPatch(typeof(DoorLock), nameof(DoorLock.OpenOrCloseDoor))]
        [HarmonyPrefix]
        private static bool OpenDoorAsPlayer(ref DoorLock __instance, ref PlayerControllerB playerWhoTriggered)
        {
            if (IsDoorBlocked(ref __instance))
            {
                if (GameNetworkManager.Instance.localPlayerController == playerWhoTriggered)
                {
                    HUDManager.Instance.DisplayTip("Impossible Action", "You can't open the door until the end of the game!");
                }
                return false;
            }
            return true;
        }

        public static bool IsDoorBlocked(ref DoorLock doorLock)
        {
            if (blockedDoors.ContainsKey(doorLock))
            {
                return true;
            }
            return false;
        }
    }
}
