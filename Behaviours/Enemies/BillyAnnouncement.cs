using LegaFusionCore.Managers;
using SawTapes.Behaviours.Billy;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Enemies;

public class BillyAnnouncement : BillyBike
{
    public bool isMoving;

    public BillyPuppet billyPuppet;
    public int billyValue = 0;

    public void StartFollowingPlayer()
        => isMoving = true;

    public override void Update()
    {
        base.Update();
        if (IsServer && isMoving && targetPlayer != null) MoveTowardsPlayer();
    }

    public void MoveTowardsPlayer()
    {
        StartMovingClientRpc();
        if (Vector3.Distance(targetPlayer.transform.position, transform.position) <= 4f)
        {
            isMoving = false;

            StopMovingClientRpc();
            _ = StartCoroutine(BillyDialogueCoroutine());
        }
    }

    [ClientRpc]
    public void StartMovingClientRpc()
    {
        PlayMovementSound();
        creatureAnimator?.SetBool("isMoving", true);
        SetMovingTowardsTargetPlayer(targetPlayer);
    }

    [ClientRpc]
    public void StopMovingClientRpc()
    {
        moveTowardsDestination = false;
        movingTowardsTargetPlayer = false;
        targetPlayer = null;
        agent.speed = 0f;
        creatureSFX.Stop();
        creatureAnimator?.SetBool("isMoving", false);
    }

    public override void ExecutePostDialogueForServer()
    {
        if (billyPuppet != null) return;

        Vector3 position = transform.position;
        billyPuppet = LFCObjectsManager.SpawnObjectForServer(SawTapes.billyPuppet.spawnPrefab, position) as BillyPuppet;
        SpawnBillyClientRpc(billyPuppet.GetComponent<NetworkObject>());
    }

    [ClientRpc]
    public void SpawnBillyClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        billyPuppet = networkObject.gameObject.GetComponentInChildren<BillyPuppet>();
        if (billyPuppet != null)
        {
            billyPuppet.EnableItemMeshes(false);
            billyPuppet.SetScrapValue(billyValue);
            billyPuppet.billy = this;
        }
        else
        {
            SawTapes.mls.LogError("billyPuppet could not be found.");
        }
    }
}
