using GameNetcodeStuff;
using SawTapes.Behaviours;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers;

public class ObjectSTManager
{
    public static void ForceGrabObject(GrabbableObject grabbableObject, PlayerControllerB player)
    {
        player.currentlyGrabbingObject = grabbableObject;
        player.grabInvalidated = false;

        player.currentlyGrabbingObject.InteractItem();

        if (player.currentlyGrabbingObject.grabbable && player.FirstEmptyItemSlot() != -1)
        {
            player.playerBodyAnimator.SetBool("GrabInvalidated", value: false);
            player.playerBodyAnimator.SetBool("GrabValidated", value: false);
            player.playerBodyAnimator.SetBool("cancelHolding", value: false);
            player.playerBodyAnimator.ResetTrigger("Throw");
            player.SetSpecialGrabAnimationBool(setTrue: true);
            player.isGrabbingObjectAnimation = true;
            player.cursorIcon.enabled = false;
            player.cursorTip.text = "";
            player.twoHanded = player.currentlyGrabbingObject.itemProperties.twoHanded;
            player.carryWeight = Mathf.Clamp(player.carryWeight + (player.currentlyGrabbingObject.itemProperties.weight - 1f), 1f, 10f);
            player.grabObjectAnimationTime = player.currentlyGrabbingObject.itemProperties.grabAnimationTime > 0f
                ? player.currentlyGrabbingObject.itemProperties.grabAnimationTime
                : 0.4f;

            if (!player.isTestingPlayer) player.GrabObjectServerRpc(player.currentlyGrabbingObject.NetworkObject);
            if (player.grabObjectCoroutine != null) player.StopCoroutine(player.grabObjectCoroutine);
            player.grabObjectCoroutine = player.StartCoroutine(player.GrabObject());
        }
    }

    public static void ChangeObjectPosition(GrabbableObject grabbableObject, Vector3 position)
    {
        if (grabbableObject.isHeld) return;

        grabbableObject.EnableItemMeshes(false);
        grabbableObject.transform.localPosition = position;
        grabbableObject.transform.position = position;
        grabbableObject.startFallingPosition = position;
        grabbableObject.FallToGround();
        grabbableObject.hasHitGround = false;
        grabbableObject.EnableItemMeshes(true);
    }

    public static void DestroyObjectOfTypeForServer<T>(T grabbableObject) where T : GrabbableObject
    {
        if (grabbableObject == null) return;

        NetworkObject networkObject = grabbableObject.GetComponent<NetworkObject>();
        if (networkObject == null || !networkObject.IsSpawned) return;

        SawTapesNetworkManager.Instance.DestroyObjectClientRpc(grabbableObject.GetComponent<NetworkObject>());
    }

    public static void DestroyObjectsOfTypeAllForServer<T>() where T : GrabbableObject
    {
        foreach (T grabbableObject in Resources.FindObjectsOfTypeAll<T>())
            DestroyObjectOfTypeForServer<T>(grabbableObject);
    }

    public static void DestroyReverseBearTrapForServer(PlayerControllerB player)
    {
        if (!GameNetworkManager.Instance.localPlayerController.IsHost && !GameNetworkManager.Instance.localPlayerController.IsServer) return;

        PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(player);
        if (playerBehaviour == null || playerBehaviour.reverseBearTrap == null) return;

        NetworkObject networkObject = playerBehaviour.reverseBearTrap.GetComponent<NetworkObject>();
        if (networkObject == null || !networkObject.IsSpawned) return;

        SawTapesNetworkManager.Instance.DestroyObjectClientRpc(networkObject);
    }
}
