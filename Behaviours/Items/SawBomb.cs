using GameNetcodeStuff;
using SawTapes.Managers;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class SawBomb : PhysicsProp
{
    public bool hasExploded = false;
    public bool isTransferred = false;

    public AudioSource bombAudio;
    public AudioClip tickingSound;

    public Coroutine transferBombCoroutine;
    public Coroutine tickingCoroutine;

    public void StartTickingForServer()
    {
        int randomDuration = new System.Random().Next(15, 30);
        int startTime = (int)(tickingSound.length - randomDuration);
        StartTickingClientRpc(startTime);
    }

    [ClientRpc]
    public void StartTickingClientRpc(int startTime)
    {
        if (tickingCoroutine != null) return;
        tickingCoroutine = StartCoroutine(StartTickingCoroutine(startTime));
    }

    public IEnumerator StartTickingCoroutine(int startTime)
    {
        bombAudio.clip = tickingSound;
        bombAudio.time = Mathf.Max(0f, startTime);
        bombAudio.Play();

        yield return new WaitUntil(() => bombAudio.isPlaying);
        yield return new WaitUntil(() => !bombAudio.isPlaying);

        SpawnExplosion();
    }

    [ClientRpc]
    public void SpawnExplosionClientRpc()
        => SpawnExplosion();

    public void SpawnExplosion()
    {
        if (hasExploded) return;

        if (tickingCoroutine != null)
        {
            StopCoroutine(tickingCoroutine);
            tickingCoroutine = null;
        }

        bombAudio.Stop();
        Landmine.SpawnExplosion(transform.position + Vector3.up, spawnExplosionEffect: true, 15f, 15f);
        hasExploded = true;
        DestroyObjectInHand(playerHeldBy);
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown || playerHeldBy == null || transferBombCoroutine != null) return;

        Ray ray = new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, 3f);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.GetComponent<PlayerControllerB>() == playerHeldBy) continue;

            PlayerControllerB targetedPlayer = hit.collider.GetComponent<PlayerControllerB>();
            if (targetedPlayer == null) continue;

            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(targetedPlayer);
            if (playerBehaviour == null || !playerBehaviour.isInGame)
            {
                HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_IMPAC_NOT_SUBJECT);
                continue;
            }

            TransferBombServerRpc((int)playerHeldBy.playerClientId, (int)targetedPlayer.playerClientId);
            break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TransferBombServerRpc(int holdingPlayerId, int targetedPlayerId)
        => TransferBombClientRpc(holdingPlayerId, targetedPlayerId);

    [ClientRpc]
    public void TransferBombClientRpc(int holdingPlayerId, int targetedPlayerId)
    {
        PlayerControllerB holdingPlayer = StartOfRound.Instance.allPlayerObjects[holdingPlayerId].GetComponent<PlayerControllerB>();
        PlayerControllerB targetedPlayer = StartOfRound.Instance.allPlayerObjects[targetedPlayerId].GetComponent<PlayerControllerB>();
        transferBombCoroutine = StartCoroutine(TransferBombCoroutine(holdingPlayer, targetedPlayer));
    }

    public IEnumerator TransferBombCoroutine(PlayerControllerB holdingPlayer, PlayerControllerB targetedPlayer)
    {
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        isTransferred = true;

        if (holdingPlayer == localPlayer) holdingPlayer.DropAllHeldItemsAndSync();

        if (tickingCoroutine != null)
        {
            StopCoroutine(tickingCoroutine);
            tickingCoroutine = null;
        }

        bombAudio.Stop();

        yield return new WaitForSeconds(0.2f);

        if (targetedPlayer == localPlayer)
        {
            targetedPlayer.DropAllHeldItemsAndSync();

            yield return new WaitForSeconds(0.2f);

            _ = StartCoroutine(SlowDownPlayerCoroutine(targetedPlayer));
            STUtilities.ForceGrabObject(this, targetedPlayer);
        }

        if (localPlayer.IsHost || localPlayer.IsServer) StartTickingForServer();
        isTransferred = false;

        yield return new WaitForSeconds(3.8f);

        transferBombCoroutine = null;
    }

    public IEnumerator SlowDownPlayerCoroutine(PlayerControllerB player)
    {
        float savedMovementSpeed = player.movementSpeed;
        player.movementSpeed /= 2f;

        yield return new WaitForSeconds(4f);

        player.movementSpeed = savedMovementSpeed;
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
        if (isTransferred || deactivated) return;
        SpawnExplosion();
    }
}
