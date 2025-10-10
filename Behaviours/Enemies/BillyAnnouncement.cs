using LegaFusionCore.Managers;
using SawTapes.Behaviours.Items;
using SawTapes.Files;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Enemies;

public class BillyAnnouncement : BillyBike
{
    public bool isMoving = true;

    public BillyPuppet billyPuppet;
    public int billyValue = 0;

    public override void Start()
    {
        base.Start();
        subtitlesBilly = SubtitleFile.billyAnnouncementSubtitles;
    }

    public override void Update()
    {
        base.Update();
        if (IsServer && isMoving && targetPlayer != null) MoveTowardsPlayer();
    }

    public void MoveTowardsPlayer()
    {
        StartMovingEveryoneRpc();
        if (Vector3.Distance(targetPlayer.transform.position, transform.position) <= 4f)
        {
            isMoving = false;

            StopMovingEveryoneRpc();
            _ = StartCoroutine(BillyDialogueCoroutine());
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void StartMovingEveryoneRpc()
    {
        PlayMovementSound();
        creatureAnimator?.SetBool("isMoving", true);
        SetMovingTowardsTargetPlayer(targetPlayer);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void StopMovingEveryoneRpc()
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
        SpawnBillyEveryoneRpc(billyPuppet.GetComponent<NetworkObject>());
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SpawnBillyEveryoneRpc(NetworkObjectReference obj)
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
