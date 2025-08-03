using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Utilities;
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

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
    [HarmonyPrefix]
    private static bool PreKillPlayer(ref PlayerControllerB __instance)
    {
        if (StartOfRound.Instance.shipIsLeaving || __instance != GameNetworkManager.Instance.localPlayerController || SawTapes.bathroom != null) return true;

        JigsawJudgement jigsawJudgement = LFCUtilities.GetAddonComponent<JigsawJudgement>(__instance);
        if (jigsawJudgement != null && !jigsawJudgement.onCooldown)
        {
            __instance.DropAllHeldItemsAndSync();
            jigsawJudgement.ActivateAddonAbility();
            return false;
        }
        return true;
    }
}
