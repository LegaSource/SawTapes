using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Addons;
using LegaFusionCore.Registries;
using SawTapes.Managers;
using System.Collections;
using UnityEngine;

namespace SawTapes.Behaviours.Items.Addons;

public class SprintBurst : AddonComponent
{
    public override void ActivateAddonAbility()
    {
        if (onCooldown) return;

        PlayerControllerB player = GetComponentInParent<GrabbableObject>()?.playerHeldBy;
        if (player == null) return;

        StartCooldown(ConfigManager.sprintBurstCooldown.Value);
        _ = StartCoroutine(SprintBurstCoroutine());
    }

    public IEnumerator SprintBurstCoroutine()
    {
        LFCStatRegistry.AddModifier(LegaFusionCore.Constants.STAT_SPEED, $"{SawTapes.modName}SprintBurst", 1f);
        yield return new WaitForSeconds(5f);
        LFCStatRegistry.RemoveModifier(LegaFusionCore.Constants.STAT_SPEED, $"{SawTapes.modName}SprintBurst");
    }
}
