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
        private static bool TeleportPlayer(ref PlayerControllerB __instance) => !PlayerSTManager.PreventTeleportPlayer(__instance);

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.spawnPlayerAnimTimer))]
        [HarmonyPrefix]
        private static void SpawnPlayerAnim(ref PlayerControllerB __instance)
        {
            PlayerSTBehaviour playerBehaviour = __instance.GetComponent<PlayerSTBehaviour>();
            if (playerBehaviour != null)
                __instance.StartCoroutine(PlayerSTManager.SetUntargetablePlayerCoroutine(playerBehaviour, 4f));
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ItemSecondaryUse_performed))]
        [HarmonyPrefix]
        private static void SecondaryUsePerformed(ref PlayerControllerB __instance)
        {
            if (__instance != GameNetworkManager.Instance.localPlayerController) return;

            PlayerSTBehaviour playerBehaviour = __instance.GetComponent<PlayerSTBehaviour>();
            if (playerBehaviour != null)
                PlayerSTManager.SecondaryUsePerformed(playerBehaviour);
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayerClientRpc))]
        [HarmonyPostfix]
        private static void PlayerDeath(ref PlayerControllerB __instance, int playerId)
        {
            PlayerSTBehaviour playerBehaviour = __instance.playersManager.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>();
            if (playerBehaviour == null) return;

            TileSTManager.OpenTileDoors(playerBehaviour);
            PlayerSTManager.ResetPlayerGame(playerBehaviour);
        }
    }
}
