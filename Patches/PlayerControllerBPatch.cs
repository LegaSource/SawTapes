using GameNetcodeStuff;
using HarmonyLib;
using SawTapes.Behaviours;
using SawTapes.Managers;

namespace SawTapes.Patches
{
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        [HarmonyPostfix]
        private static void StartPlayerControllerB(ref PlayerControllerB __instance) => PlayerSTManager.AddPlayerBehaviour(__instance);

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.TeleportPlayer))]
        [HarmonyPrefix]
        private static bool TeleportPlayer(ref PlayerControllerB __instance) => !PlayerSTManager.PreventTeleportPlayer(ref __instance);

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayerClientRpc))]
        [HarmonyPostfix]
        private static void PlayerDeath(ref PlayerControllerB __instance, int playerId)
        {
            PlayerSTBehaviour playerBehaviour = __instance.playersManager.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>();
            if (playerBehaviour != null)
            {
                PlayerSTManager.ResetPlayerGame(ref playerBehaviour);
            }
        }
    }
}
