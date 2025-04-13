using SawTapes.Managers;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class SawKey : PhysicsProp
{
    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown && playerHeldBy == null) return;

        PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(playerHeldBy);
        if (playerBehaviour == null) return;

        if (playerBehaviour.reverseBearTrap == null)
        {
            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_IMPAC_KEY);
            return;
        }

        Vector3 position = playerHeldBy.gameplayCamera.transform.position + playerHeldBy.gameplayCamera.transform.forward;
        ReleaseFromReverseBearTrapServerRpc(playerBehaviour.reverseBearTrap.GetComponent<NetworkObject>(), position);
        SawTapesNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReleaseFromReverseBearTrapServerRpc(NetworkObjectReference obj, Vector3 position)
        => ReleaseFromReverseBearTrapClientRpc(obj, position);

    [ClientRpc]
    public void ReleaseFromReverseBearTrapClientRpc(NetworkObjectReference obj, Vector3 position)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        ReverseBearTrap reverseBearTrap = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as ReverseBearTrap;
        reverseBearTrap.parentObject = null;
        reverseBearTrap.EnablePhysics(enable: true);
        reverseBearTrap.SetCarriedState(false);
        reverseBearTrap.transform.localPosition = position;
        reverseBearTrap.transform.position = position;
        reverseBearTrap.startFallingPosition = position;
        reverseBearTrap.FallToGround();
        reverseBearTrap.isReleased = true;
        reverseBearTrap.grabbable = true;
        reverseBearTrap.grabbableToEnemies = true;
    }
}
