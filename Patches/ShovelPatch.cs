using GameNetcodeStuff;
using HarmonyLib;
using SawTapes.Behaviours.Games.EscapeGame;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Patches;

internal class ShovelPatch
{
    [HarmonyPatch(typeof(Shovel), nameof(Shovel.HitShovel))]
    [HarmonyPrefix]
    private static void HitShovel(ref Shovel __instance)
    {
        if (SawTapes.sawTape == null || SawTapes.sawTape is not EscapeTape escapeTape) return;

        PlayerControllerB player = __instance.previousPlayerHeldBy;
        if (player == null) return;

        RaycastHit[] hits = Physics.SphereCastAll(
            player.gameplayCamera.transform.position + (player.gameplayCamera.transform.right * -0.35f),
            0.8f,
            player.gameplayCamera.transform.forward,
            1.5f,
            __instance.shovelMask,
            QueryTriggerInteraction.Collide
        );

        foreach (RaycastHit hit in hits)
        {
            GameObject hazard = hit.collider.transform.root.gameObject;
            if (!escapeTape.hazards.Contains(hazard)) continue;

            escapeTape.DespawnHazardServerRpc(hazard.GetComponent<NetworkObject>());
            break;
        }
    }
}
