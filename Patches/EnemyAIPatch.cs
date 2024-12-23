﻿using GameNetcodeStuff;
using HarmonyLib;
using SawTapes.Behaviours;
using SawTapes.Managers;

namespace SawTapes.Patches
{
    internal class EnemyAIPatch
    {
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.HitEnemy))]
        [HarmonyPostfix]
        private static void HitEnemy(ref EnemyAI __instance, int force = 1, PlayerControllerB playerWhoHit = null)
        {
            EnemyAI assignedEnemy = playerWhoHit?.GetComponent<PlayerSTBehaviour>().huntingTape?.assignedEnemy;
            if (!__instance.isEnemyDead
                && __instance.enemyHP - force <= 0f
                && GameNetworkManager.Instance.localPlayerController == playerWhoHit
                && assignedEnemy != null
                && assignedEnemy != __instance)
            {
                SawTapesNetworkManager.Instance.SpawnPursuerEyeServerRpc(__instance.transform.position);
            }
        }

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.KillEnemy))]
        [HarmonyPostfix]
        private static void KillEnemy(ref EnemyAI __instance)
        {
            if (GameNetworkManager.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.localPlayerController.IsServer)
            {
                EnemyAI enemy = UnityEngine.Object.FindFirstObjectByType<HuntingTape>()?.assignedEnemy;
                if (enemy != null && __instance == enemy)
                {
                    SawTapesNetworkManager.Instance.SpawnSawKeyServerRpc(__instance.transform.position);
                }
            }
        }
    }
}
