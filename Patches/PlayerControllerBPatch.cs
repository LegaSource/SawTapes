using GameNetcodeStuff;
using HarmonyLib;
using SawTapes.Behaviours;
using SawTapes.Behaviours.Tapes;
using SawTapes.Managers;

namespace SawTapes.Patches
{
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        [HarmonyPostfix]
        private static void StartPlayerControllerB(ref PlayerControllerB __instance)
            => PlayerSTManager.AddPlayerBehaviour(__instance);

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.TeleportPlayer))]
        [HarmonyPrefix]
        private static bool TeleportPlayer(ref PlayerControllerB __instance)
            => !PlayerSTManager.PreventTeleportPlayer(__instance);

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.spawnPlayerAnimTimer))]
        [HarmonyPrefix]
        private static void SpawnPlayerAnim(ref PlayerControllerB __instance)
        {
            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(__instance);
            if (playerBehaviour == null) return;
            
            __instance.StartCoroutine(PlayerSTManager.SetUntargetablePlayerCoroutine(playerBehaviour, 4f));
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ItemSecondaryUse_performed))]
        [HarmonyPrefix]
        private static void SecondaryUsePerformed(ref PlayerControllerB __instance)
        {
            if (__instance != GameNetworkManager.Instance.localPlayerController) return;

            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(__instance);
            if (playerBehaviour == null) return;

            (playerBehaviour.sawTape as EscapeTape)?.TeleportSawToPlayerServerRpc((int)__instance.playerClientId);
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayerClientRpc))]
        [HarmonyPostfix]
        private static void KillPlayerForClient(ref PlayerControllerB __instance, int playerId)
        {
            PlayerControllerB player = __instance.playersManager.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            PlayerSTManager.ResetPlayerGame(player);
        }
    }
}
