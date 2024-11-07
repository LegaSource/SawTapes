using GameNetcodeStuff;
using HarmonyLib;
using SawTapes.Behaviours;

namespace SawTapes.Patches
{
    internal class EnemyAIPatch
    {
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.HitEnemy))]
        [HarmonyPostfix]
        private static void HitEnemy(ref EnemyAI __instance, int force = 1, PlayerControllerB playerWhoHit = null)
        {
            if ((GameNetworkManager.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.localPlayerController.IsServer)
                && !__instance.isEnemyDead
                && __instance.enemyHP - force <= 0f)
            {
                EnemySTBehaviour enemyBehaviour = __instance.enemyType?.enemyPrefab?.GetComponent<EnemySTBehaviour>();
                if (enemyBehaviour != null && enemyBehaviour.isAssignedEnemy)
                {
                    RoundManagerPatch.SpawnItem(ref SawTapes.sawKeyObj, __instance.transform.position);
                    return;
                }

                if (playerWhoHit?.GetComponent<PlayerSTBehaviour>().assignedEnemy != null)
                {
                    RoundManagerPatch.SpawnItem(ref SawTapes.pursuerEyeObj, __instance.transform.position);
                }
            }
        }
    }
}
