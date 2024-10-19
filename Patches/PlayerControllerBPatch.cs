using GameNetcodeStuff;
using HarmonyLib;
using SawTapes.Managers;

namespace SawTapes.Patches
{
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        [HarmonyPostfix]
        private static void StartPlayerControllerB(ref PlayerControllerB __instance)
        {
            PlayerSTManager.AddPlayerBehaviour(__instance);
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.TeleportPlayer))]
        [HarmonyPrefix]
        private static bool TeleportPlayer(ref PlayerControllerB __instance)
        {
            return !PlayerSTManager.PreventTeleportPlayer(ref __instance);
        }
    }
}
