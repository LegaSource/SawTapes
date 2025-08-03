using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Managers;
using SawTapes.Behaviours.Games.HuntingGame;

namespace SawTapes.Patches;

internal class EnemyAIPatch
{
    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.KillEnemy))]
    [HarmonyPostfix]
    private static void KillEnemy(ref EnemyAI __instance)
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (!player.IsHost && !player.IsServer) return;
        if (SawTapes.sawTape == null || SawTapes.sawTape is not HuntingTape huntingTape) return;
        if (!huntingTape.spawnedEnemies.Contains(__instance.thisNetworkObject)) return;

        _ = LFCObjectsManager.SpawnObjectForServer(SawTapes.sawKeyHunting.spawnPrefab, __instance.transform.position);
    }

    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.PlayerIsTargetable))]
    [HarmonyPrefix]
    private static bool IsPlayerTargetable(ref bool __result)
    {
        if (PlayerControllerBPatch.isTargetable) return true;
        __result = false;
        return false;
    }
}
