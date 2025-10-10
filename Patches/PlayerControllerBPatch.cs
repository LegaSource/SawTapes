using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Utilities;
using SawTapes.Behaviours.Games.ExplosiveGame;
using SawTapes.Behaviours.Items.Addons;
using System.Collections;

namespace SawTapes.Patches;

internal class PlayerControllerBPatch
{
    public static bool isTargetable = true;

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.spawnPlayerAnimTimer))]
    [HarmonyPostfix]
    private static IEnumerator SpawnPlayerAnim(IEnumerator __result)
    {
        isTargetable = false;
        while (__result.MoveNext()) yield return __result.Current;
        isTargetable = true;
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Discard_performed))]
    [HarmonyPrefix]
    private static bool PreDiscardPerformed(ref PlayerControllerB __instance)
    {
        if (!__instance.IsOwner || !__instance.isPlayerControlled || (__instance.IsServer && !__instance.isHostPlayerObject) || __instance.currentlyHeldObjectServer == null) return true;

        if (__instance.currentlyHeldObjectServer is SawBombExplosive sawBomb)
        {
            sawBomb.RemoveAuraFromPlayer();
            if (!sawBomb.isTransferred && !sawBomb.hasBeenDestroyed && !sawBomb.deactivated) sawBomb.SpawnExplosionEveryoneRpc();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
    [HarmonyPrefix]
    private static bool PreKillPlayer(ref PlayerControllerB __instance)
    {
        if (StartOfRound.Instance.shipIsLeaving || HUDManagerPatch.isPlayingGame || __instance != GameNetworkManager.Instance.localPlayerController || SawTapes.bathroom != null) return true;

        JigsawJudgement jigsawJudgement = LFCUtilities.GetAddonComponent<JigsawJudgement>(__instance);
        if (jigsawJudgement != null && !jigsawJudgement.onCooldown)
        {
            jigsawJudgement.ActivateAddonAbility();
            return false;
        }
        return true;
    }
}
