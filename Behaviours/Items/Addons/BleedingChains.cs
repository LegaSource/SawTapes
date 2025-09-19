using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Addons;
using SawTapes.Managers;
using Unity.Netcode;

namespace SawTapes.Behaviours.Items.Addons;

public class BleedingChains : AddonComponent
{
    public EnemyAI aimedEnemy;
    public PlayerControllerB aimedPlayer;

    public void Update()
    {
        if (onCooldown || !grabbableObject.isHeld || grabbableObject.isPocketed || grabbableObject.playerHeldBy == null || grabbableObject.playerHeldBy != GameNetworkManager.Instance.localPlayerController)
        {
            if (aimedEnemy != null) STUtilities.RemoveAuraFromEnemy($"{SawTapes.modName}BC", ref aimedEnemy);
            if (aimedPlayer != null) STUtilities.RemoveAuraFromPlayer($"{SawTapes.modName}BC", ref aimedPlayer);
            return;
        }
        STUtilities.ShowAuraAimedEnemy(grabbableObject.playerHeldBy, $"{SawTapes.modName}BC", 10f, ref aimedEnemy);
        if (aimedEnemy == null) STUtilities.ShowAuraAimedPlayer(grabbableObject.playerHeldBy, $"{SawTapes.modName}BC", 10f, ref aimedPlayer);
    }

    public override void ActivateAddonAbility()
    {
        if (onCooldown || (aimedEnemy == null && aimedPlayer == null)) return;

        PlayerControllerB player = GetComponentInParent<GrabbableObject>()?.playerHeldBy;
        if (player == null) return;

        NetworkObject entityObj = aimedEnemy != null ? aimedEnemy.thisNetworkObject : aimedPlayer.GetComponent<NetworkObject>();
        StartCooldown(ConfigManager.bleedingChainsCooldown.Value);
        SawTapesNetworkManager.Instance.SpawnBleedingChainsServerRpc(entityObj, (int)player.playerClientId);
    }
}
