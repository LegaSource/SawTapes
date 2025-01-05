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
        private static bool OpenDoorAsEnemy(ref DoorLock __instance) => !IsDoorBlocked(__instance);

        [HarmonyPatch(typeof(DoorLock), nameof(DoorLock.TryDoorHaunt))]
        [HarmonyPrefix]
        private static bool OpenDoorAsHaunt(ref DoorLock __instance) => !IsDoorBlocked(__instance);

        [HarmonyPatch(typeof(DoorLock), nameof(DoorLock.OpenOrCloseDoor))]
        [HarmonyPrefix]
        private static bool OpenDoorAsPlayer(ref DoorLock __instance, ref PlayerControllerB playerWhoTriggered)
        {
            if (!IsDoorBlocked(__instance)) return true;

            if (GameNetworkManager.Instance.localPlayerController == playerWhoTriggered)
                HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_IMPAC_LOCKED_DOOR);
            return false;
        }

        public static bool IsDoorBlocked(DoorLock doorLock)
            => blockedDoors.ContainsKey(doorLock);
    }
}
