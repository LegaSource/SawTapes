using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Addons;
using SawTapes.Managers;
using Unity.Netcode;

namespace SawTapes.Behaviours.Items.Addons;

public class FinalDetonation : AddonComponent
{
    public EnemyAI aimedEnemy;
    public PlayerControllerB aimedPlayer;

    public void Update()
    {
        if (onCooldown || !grabbableObject.isHeld || grabbableObject.isPocketed || grabbableObject.playerHeldBy == null || grabbableObject.playerHeldBy != GameNetworkManager.Instance.localPlayerController)
        {
            if (aimedEnemy != null) STUtilities.RemoveAuraFromEnemy($"{SawTapes.modName}FD", ref aimedEnemy);
            if (aimedPlayer != null) STUtilities.RemoveAuraFromPlayer($"{SawTapes.modName}FD", ref aimedPlayer);
            return;
        }
        STUtilities.ShowAuraAimedEnemy(grabbableObject.playerHeldBy, $"{SawTapes.modName}FD", 10f, ref aimedEnemy);
        if (aimedEnemy == null) STUtilities.ShowAuraAimedPlayer(grabbableObject.playerHeldBy, $"{SawTapes.modName}FD", 10f, ref aimedPlayer);
    }

    public override void ActivateAddonAbility()
    {
        if (onCooldown || (aimedEnemy == null && aimedPlayer == null)) return;

        PlayerControllerB player = GetComponentInParent<GrabbableObject>()?.playerHeldBy;
        if (player == null) return;

        NetworkObject entityObj = aimedEnemy != null ? aimedEnemy.thisNetworkObject : aimedPlayer.GetComponent<NetworkObject>();
        StartCooldown(ConfigManager.finalDetonationCooldown.Value);
        SawTapesNetworkManager.Instance.SpawnFinalDetonationServerRpc(entityObj, (int)player.playerClientId);
    }
}
