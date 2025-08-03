using GameNetcodeStuff;
using UnityEngine;

namespace SawTapes.Behaviours.Bathroom;

public class ChainBathroom : MonoBehaviour
{
    public Transform attach1;
    public Transform attach2;
    public PlayerControllerB player;

    public void SetupCollisionIgnore()
    {
        foreach (Collider chainCollider in GetComponentsInChildren<Collider>())
        {
            chainCollider.excludeLayers = -2621449;
            foreach (Collider playerCollider in GameNetworkManager.Instance.localPlayerController.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(chainCollider, playerCollider, true);
        }
    }

    public void ConfigureAttach()
    {
        attach2.position = player.transform.position + (Vector3.up * 0.2f);
        attach2.SetParent(player.playerGlobalHead);

        FixedJoint joint = attach2.GetComponent<FixedJoint>();
        joint.connectedBody = player.playerRigidbody;
    }

    public void LateUpdate()
    {
        if (player == null) return;
        if (attach1 == null || attach2 == null) return;

        // Ajustement de la rotation globale de la chaîne
        Vector3 direction = attach2.position - attach1.position;
        transform.forward = direction.normalized;

        // Ajuster la rotation des attaches pour pointer vers l'autre joueur
        attach1.LookAt(attach2.position);
        attach2.LookAt(attach1.position);
    }

    public void OnDestroy()
    {
        // Supprimer les maillons aux extrémités
        if (attach1 != null)
        {
            attach1.SetParent(null);
            Destroy(attach1.gameObject);
            attach1 = null;
        }
        if (attach2 != null)
        {
            attach2.SetParent(null);
            Destroy(attach2.gameObject);
            attach2 = null;
        }
    }
}

