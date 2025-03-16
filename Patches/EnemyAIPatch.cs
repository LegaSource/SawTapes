using GameNetcodeStuff;
using HarmonyLib;
using SawTapes.Behaviours;
using SawTapes.Behaviours.Tapes;
using SawTapes.Managers;
using System.Linq;

namespace SawTapes.Patches
{
    internal class EnemyAIPatch
    {
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.HitEnemy))]
        [HarmonyPostfix]
        private static void HitEnemy(ref EnemyAI __instance, int force = 1, PlayerControllerB playerWhoHit = null)
        {
            if (GameNetworkManager.Instance.localPlayerController != playerWhoHit) return;
            if (__instance.isEnemyDead || __instance.enemyHP - force > 0f) return;

            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(playerWhoHit);
            if (playerBehaviour == null) return;

            HuntingTape huntingTape = playerBehaviour.sawTape as HuntingTape;
            if (huntingTape == null) return;
            if (huntingTape.spawnedEnemies.Contains(__instance.thisNetworkObject)) return;

            SawTapesNetworkManager.Instance.SpawnPursuerEyeServerRpc(__instance.transform.position);
        }

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.KillEnemy))]
        [HarmonyPostfix]
        private static void KillEnemy(ref EnemyAI __instance)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (!player.IsHost && !player.IsServer) return;
            SawTapes.mls.LogError("KillEnemy server");

            HuntingTape huntingTape = StartOfRound.Instance.allPlayerScripts
                .Select(p => PlayerSTManager.GetPlayerBehaviour(p)?.sawTape as HuntingTape)
                .FirstOrDefault(h => h != null);
            if (huntingTape == null) return;
            SawTapes.mls.LogError("huntingTape trouvée");
            if (!huntingTape.spawnedEnemies.Contains(__instance.thisNetworkObject)) return;
            SawTapes.mls.LogError("spawnedEnemies contains thisNetworkObject");

            RoundManagerPatch.SpawnItem(SawTapes.sawKeyObj, __instance.transform.position);
        }

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.PlayerIsTargetable))]
        [HarmonyPrefix]
        private static bool IsPlayerTargetable(ref bool __result, PlayerControllerB playerScript)
        {
            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(playerScript);
            if (playerBehaviour == null || playerBehaviour.isTargetable) return true;

            __result = false;
            return false;
        }
    }
}
