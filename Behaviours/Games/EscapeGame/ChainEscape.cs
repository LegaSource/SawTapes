using GameNetcodeStuff;
using LegaFusionCore.Registries;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.EscapeGame;

public class ChainEscape : NetworkBehaviour
{
    public Transform attach1;
    public Transform attach2;
    public PlayerControllerB player1;
    public PlayerControllerB player2;

    public Coroutine slowDownCoroutine;

    [ClientRpc]
    public void SetUpChainClientRpc(int playerId1, int playerId2)
    {
        player1 = StartOfRound.Instance.allPlayerObjects[playerId1].GetComponent<PlayerControllerB>();
        player2 = StartOfRound.Instance.allPlayerObjects[playerId2].GetComponent<PlayerControllerB>();

        ConfigureAttach(attach1, player1);
        ConfigureAttach(attach2, player2);
        SetupCollisionIgnore();
    }

    public void SetupCollisionIgnore()
    {
        foreach (Collider chainCollider in GetComponentsInChildren<Collider>())
        {
            chainCollider.excludeLayers = -2621449;
            foreach (Collider playerCollider in GameNetworkManager.Instance.localPlayerController.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(chainCollider, playerCollider, true);
        }
    }

    public void ConfigureAttach(Transform attach, PlayerControllerB player)
    {
        attach.position = player.transform.position + (Vector3.up * 1.2f);
        attach.SetParent(player.playerGlobalHead);

        FixedJoint joint = attach.GetComponent<FixedJoint>();
        joint.connectedBody = player.playerRigidbody;
    }

    public void Update()
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
        LFCStatRegistry.AddModifier("Speed", $"{SawTapes.modName}ChainEscape", -0.5f);
        yield return new WaitForSeconds(1f);
        LFCStatRegistry.RemoveModifier("Speed", $"{SawTapes.modName}ChainEscape");
        slowDownCoroutine = null;
    }

    public void LateUpdate()
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
        {
            StopCoroutine(slowDownCoroutine);
            slowDownCoroutine = null;
        }

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

        base.OnDestroy();
    }
}