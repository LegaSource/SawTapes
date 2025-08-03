using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Addons;
using SawTapes.Managers;

namespace SawTapes.Behaviours.Items.Addons;

public class JigsawJudgement : AddonComponent
{
    public override void ActivateAddonAbility()
    {
        PlayerControllerB player = GetComponentInParent<GrabbableObject>()?.playerHeldBy;
        if (player == null) return;

        StartCooldown(ConfigManager.bathroomCooldown.Value);
        SawTapesNetworkManager.Instance.SpawnBathroomServerRpc((int)player.playerClientId);
    }
}
