using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class Chain : PhysicsProp
{
    public Transform attach1;
    public Transform attach2;
    public PlayerControllerB player1;
    public PlayerControllerB player2;

    public float movementSpeed;
    public Coroutine slowDownCoroutine;

    public override void Start()
    {
        base.Start();
        movementSpeed = GameNetworkManager.Instance.localPlayerController.movementSpeed;
    }

    [ClientRpc]
    public void SetUpChainClientRpc(int playerId1, int playerId2)
    {
        player1 = StartOfRound.Instance.allPlayerObjects[playerId1].GetComponent<PlayerControllerB>();
        player2 = StartOfRound.Instance.allPlayerObjects[playerId2].GetComponent<PlayerControllerB>();

        SetupChainPhysics();
        if (!InitializeAttachPoints()) return;
        ConfigureAttach(attach1, player1);
        ConfigureAttach(attach2, player2);
        SetupCollisionIgnore();
    }

    public void SetupCollisionIgnore()
    {
        foreach (Collider chainCollider in GetComponentsInChildren<Collider>())
        {
            foreach (Collider playerCollider in GameNetworkManager.Instance.localPlayerController.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(chainCollider, playerCollider, true);
        }
    }

    public void SetupChainPhysics()
    {
        isHeld = true;
        grabbable = false;
        grabbableToEnemies = false;
        hasHitGround = false;
        EnablePhysics(enable: false);

        Vector3 direction = player2.transform.position - player1.transform.position;
        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    public bool InitializeAttachPoints()
    {
        attach1 = transform.Find("ChainAttach1");
        attach2 = transform.Find("ChainAttach2");

        if (attach1 == null || attach2 == null)
        {
            SawTapes.mls.LogError("Impossible to find the attachment points in the prefab!");
            return false;
        }
        return true;
    }

    public void ConfigureAttach(Transform attach, PlayerControllerB player)
    {
        attach.position = player.transform.position + (Vector3.up * 1.2f);
        attach.SetParent(player.playerGlobalHead);

        FixedJoint joint = attach.GetComponent<FixedJoint>();
        joint.connectedBody = player.playerRigidbody;
    }

    public override void Update()
    {
        if (player1 == null || player2 == null) return;

        if (player1.isPlayerDead)
        {
            attach1?.SetParent(player2?.playerGlobalHead);
            player1 = null;
            return;
        }
        if (player2.isPlayerDead)
        {
            attach2?.SetParent(player1?.playerGlobalHead);
            player2 = null;
            return;
        }

        Vector3 direction = player2.transform.position - player1.transform.position;
        float distance = direction.magnitude;
        if (distance >= 8f)
        {
            ApplyForceToPlayer(player1, player2, -direction);
            ApplyForceToPlayer(player2, player1, direction);
        }
    }

    public void ApplyForceToPlayer(PlayerControllerB targetPlayer, PlayerControllerB otherPlayer, Vector3 direction)
    {
        if (targetPlayer != GameNetworkManager.Instance.localPlayerController) return;

        // Applique une force de déplacement
        float maxForce = targetPlayer.isSprinting ? 5f : 2.5f;
        Vector3 force = direction.normalized * Mathf.Min(targetPlayer.thisController.velocity.magnitude, maxForce) * 0.1f;
        PullPlayerServerRpc((int)otherPlayer.playerClientId, force);

        // Ralentir le joueur
        slowDownCoroutine ??= StartCoroutine(SlowDownPlayerCoroutine());
    }

    [ServerRpc(RequireOwnership = false)]
    public void PullPlayerServerRpc(int playerId, Vector3 force)
        => PullPlayerClientRpc(playerId, force);

    [ClientRpc]
    public void PullPlayerClientRpc(int playerId, Vector3 force)
        => StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>().thisController.Move(force);

    public IEnumerator SlowDownPlayerCoroutine()
    {
        movementSpeed = GameNetworkManager.Instance.localPlayerController.movementSpeed;
        GameNetworkManager.Instance.localPlayerController.movementSpeed /= 2f;

        yield return new WaitForSeconds(1f);

        GameNetworkManager.Instance.localPlayerController.movementSpeed = movementSpeed;
        slowDownCoroutine = null;
    }

    public override void LateUpdate()
    {
        if (player1 == null || player2 == null) return;
        if (attach1 == null || attach2 == null) return;

        // Ajustement de la rotation globale de la chaîne
        Vector3 direction = attach2.position - attach1.position;
        transform.forward = direction.normalized;

        // Ajuster la rotation des attaches pour pointer vers l'autre joueur
        attach1.LookAt(attach2.position);
        attach2.LookAt(attach1.position);
    }

    public override void OnDestroy()
    {
        // Réinitialiser les états des joueurs
        if (slowDownCoroutine != null)
            GameNetworkManager.Instance.localPlayerController.movementSpeed = movementSpeed;

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
            attach1 = null;
        }

        base.OnDestroy();
    }
}
