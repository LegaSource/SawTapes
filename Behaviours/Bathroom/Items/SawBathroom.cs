using LegaFusionCore.Managers.NetworkManagers;
using Unity.Netcode;

namespace SawTapes.Behaviours.Bathroom.Items;

public class SawBathroom : PhysicsProp
{
    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown && playerHeldBy == null && playerHeldBy != SawTapes.bathroom.player) return;

        SawTapes.bathroom.hasUsedSaw = true;
        SawTapes.bathroom.EndOfGame();
        LFCNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
    }
}
