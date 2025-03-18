using GameNetcodeStuff;
using SawTapes.Behaviours;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    public class ObjectSTManager
    {
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

        public static void DestroyObjectsOfTypeAllForServer<T>() where T : GrabbableObject
        {
            foreach (T grabbableObject in Resources.FindObjectsOfTypeAll<T>())
            {
                if (grabbableObject == null) continue;

                NetworkObject networkObject = grabbableObject.GetComponent<NetworkObject>();
                if (networkObject == null || !networkObject.IsSpawned) continue;

                SawTapesNetworkManager.Instance.DestroyObjectClientRpc(grabbableObject.GetComponent<NetworkObject>());
            }
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
}
