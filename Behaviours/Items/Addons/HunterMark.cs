using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Addons;
using SawTapes.Managers;

namespace SawTapes.Behaviours.Items.Addons;

public class HunterMark : AddonComponent
{
    public override void ActivateAddonAbility()
    {
        if (onCooldown) return;

        PlayerControllerB player = GetComponentInParent<GrabbableObject>()?.playerHeldBy;
        if (player == null) return;

        StartCooldown(ConfigManager.hunterMarkCooldown.Value);
        SawTapesNetworkManager.Instance.SpawnHunterMarkServerRpc((int)player.playerClientId);
    }
}
