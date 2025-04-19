using GameNetcodeStuff;
using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.Tapes;
using SawTapes.Managers;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.MapObjects;

public class SawBox : NetworkBehaviour
{
    public InteractTrigger boxTrigger;

    public void Update()
    {
        PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(GameNetworkManager.Instance.localPlayerController);
        if (playerBehaviour == null || !playerBehaviour.isInGame || playerBehaviour.sawTape is not ExplosiveTape) return;

        if (Vector3.Distance(playerBehaviour.playerProperties.transform.position, transform.position) > 20f)
        {
            CustomPassManager.SetupAuraForObjects([gameObject], SawTapes.yellowWallhackShader);
            return;
        }
        CustomPassManager.RemoveAuraFromObjects([gameObject]);
    }

    public void BoxInteraction()
        => BoxInteractionByTape();

    public void BoxInteractionByTape()
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

        SawTape sawTape = SawGameSTManager.GetSawTapeFromPlayer(player);
        if (sawTape == null) return;

        switch (sawTape)
        {
            case ExplosiveTape:
                SawBomb sawBomb = null;
                for (int i = 0; i < player.ItemSlots.Length; i++)
                {
                    GrabbableObject grabbableObject = player.ItemSlots[i];
                    if (grabbableObject == null || grabbableObject is not SawBomb) continue;

                    sawBomb = grabbableObject as SawBomb;
                }
                if (sawBomb != null) BombContainmentServerRpc(sawBomb.GetComponent<NetworkObject>());
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BombContainmentServerRpc(NetworkObjectReference obj)
        => BombContainmentClientRpc(obj);

    [ClientRpc]
    public void BombContainmentClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        SawBomb sawBomb = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as SawBomb;
        if (sawBomb == null) return;

        if (sawBomb.tickingCoroutine != null)
        {
            sawBomb.StopCoroutine(sawBomb.tickingCoroutine);
            sawBomb.tickingCoroutine = null;
        }

        sawBomb.bombAudio.Stop();
        sawBomb.hasExploded = true;
        sawBomb.DestroyObjectInHand(sawBomb.playerHeldBy);
    }
}
