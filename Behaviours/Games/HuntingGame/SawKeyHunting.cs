using LegaFusionCore.Managers.NetworkManagers;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.HuntingGame;

public class SawKeyHunting : PhysicsProp
{
    public override void GrabItem()
    {
        if (playerHeldBy == null || playerHeldBy != GameNetworkManager.Instance.localPlayerController || SawTapes.sawTape == null) return;
        if (SawTapes.sawTape.players.Contains(playerHeldBy)) return;

        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_IMPAC_KEY);
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown && playerHeldBy == null) return;
        if (SawTapes.sawTape == null || SawTapes.sawTape is not HuntingTape huntingTape) return;
        if (!huntingTape.playerRBTs.TryGetValue(playerHeldBy, out RBTrapHunting rbt) || rbt.isReleased) return;
        if (!SawTapes.sawTape.players.Contains(playerHeldBy))
        {
            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_IMPAC_KEY);
            return;
        }

        Vector3 position = playerHeldBy.gameplayCamera.transform.position + playerHeldBy.gameplayCamera.transform.forward;
        ReleaseFromReverseBearTrapServerRpc(rbt.GetComponent<NetworkObject>(), position);
        LFCNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReleaseFromReverseBearTrapServerRpc(NetworkObjectReference obj, Vector3 position)
        => ReleaseFromReverseBearTrapClientRpc(obj, position);

    [ClientRpc]
    public void ReleaseFromReverseBearTrapClientRpc(NetworkObjectReference obj, Vector3 position)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        RBTrapHunting rbt = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as RBTrapHunting;
        rbt.parentObject = null;
        rbt.EnablePhysics(enable: true);
        rbt.SetCarriedState(false);
        rbt.parentObject = null;
        rbt.transform.SetParent(StartOfRound.Instance.propsContainer, worldPositionStays: true);
        rbt.EnablePhysics(enable: true);
        rbt.fallTime = 0f;
        rbt.startFallingPosition = position;
        rbt.transform.position = position;
        rbt.startFallingPosition = rbt.transform.parent.InverseTransformPoint(position);
        rbt.targetFloorPosition = rbt.transform.parent.InverseTransformPoint(rbt.GetItemFloorPosition());
        rbt.isReleased = true;
        rbt.grabbable = true;
        rbt.grabbableToEnemies = true;
    }
}
