using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Managers;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.ExplosiveGame;

public class SawBombExplosive : PhysicsProp
{
    public bool hasBeenDestroyed = false;
    public bool isTransferred = false;
    public PlayerControllerB aimedPlayer;

    public AudioSource bombAudio;
    public AudioClip tickingSound;

    public Coroutine tickingCoroutine;
    public Coroutine transferBombCoroutine;

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
        if (tickingCoroutine != null)
        {
            StopCoroutine(tickingCoroutine);
            tickingCoroutine = null;
        }

        bombAudio.Stop();
        Landmine.SpawnExplosion(transform.position + Vector3.up, spawnExplosionEffect: true, 15f, 15f);
        hasBeenDestroyed = true;
        DestroyObjectInHand(playerHeldBy);
    }

    public override void Update()
    {
        base.Update();

        if (!isHeld || isPocketed || hasBeenDestroyed || playerHeldBy == null || transferBombCoroutine != null) return;
        if (playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;
        if (SawTapes.sawTape == null || SawTapes.sawTape is not ExplosiveTape) return;

        ShowAuraTargetedPlayer();
    }

    public void ShowAuraTargetedPlayer()
    {
        RaycastHit[] hits = Physics.RaycastAll(new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward), 3f);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            PlayerControllerB targetedPlayer = hit.collider.GetComponent<PlayerControllerB>();
            if (targetedPlayer == null || targetedPlayer == playerHeldBy) continue;
            if (SawTapes.sawTape == null || !SawTapes.sawTape.players.Contains(targetedPlayer)) continue;

            if (aimedPlayer != null && aimedPlayer != targetedPlayer) RemoveAuraFromPlayer();
            aimedPlayer = targetedPlayer;
            CustomPassManager.SetupAuraForObjects([targetedPlayer.gameObject], LegaFusionCore.LegaFusionCore.transparentShader, SawTapes.modName, Color.yellow);
            return;
        }
        RemoveAuraFromPlayer();
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown || playerHeldBy == null || transferBombCoroutine != null) return;
        if (aimedPlayer != null) TransferBombServerRpc((int)playerHeldBy.playerClientId, (int)aimedPlayer.playerClientId);
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
            LFCObjectsManager.ForceGrabObject(this, targetedPlayer);
        }

        if (localPlayer.IsHost || localPlayer.IsServer) StartTickingForServer();
        isTransferred = false;

        yield return new WaitForSeconds(3.8f);

        transferBombCoroutine = null;
    }

    public override void PocketItem()
    {
        base.PocketItem();
        RemoveAuraFromPlayer();
    }

    public override void DiscardItem()
    {
        base.DiscardItem();

        RemoveAuraFromPlayer();
        if (!isTransferred && !hasBeenDestroyed && !deactivated) SpawnExplosion();
    }

    public void RemoveAuraFromPlayer()
    {
        if (aimedPlayer == null) return;

        CustomPassManager.RemoveAuraFromObjects([aimedPlayer.gameObject], SawTapes.modName);
        aimedPlayer = null;
    }
}
