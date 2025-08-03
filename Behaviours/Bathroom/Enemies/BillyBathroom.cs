using LegaFusionCore.Utilities;
using SawTapes.Behaviours.Enemies;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Bathroom.Enemies;

public class BillyBathroom : BillyBike
{
    public bool isMoving = false;

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
        => StartGameClientRpc();

    [ClientRpc]
    public void StartGameClientRpc()
    {
        SawTapes.bathroom.sawTheme.Play();

        if (SawTapes.bathroom.player != GameNetworkManager.Instance.localPlayerController) return;

        List<int> indexes = Enumerable.Range(0, 14).ToList();
        LFCUtilities.Shuffle(indexes);
        List<int> enabledIndexes = indexes.Take(3).ToList();

        foreach (InteractTrigger keySpotTrigger in SawTapes.bathroom.GetComponentsInChildren<InteractTrigger>())
        {
            int index = int.Parse(keySpotTrigger.name["KeySpot".Length..]);
            keySpotTrigger.interactable = enabledIndexes.Contains(index);
        }

        LFCUtilities.Shuffle(enabledIndexes);
        SawTapes.bathroom.currentSpotIndex = enabledIndexes.First();

        _ = StartCoroutine(StartGameCoroutine());
        _ = HUDManager.Instance.StartCoroutine(HUDManagerPatch.StartChronoCoroutine(ConfigManager.bathroomDuration.Value));
    }

    public IEnumerator StartGameCoroutine()
    {
        int timePassed = 0;
        while (timePassed < ConfigManager.bathroomDuration.Value)
        {
            yield return new WaitForSeconds(1f);
            timePassed++;
        }
        SawTapes.bathroom.EndOfGame();
    }
}