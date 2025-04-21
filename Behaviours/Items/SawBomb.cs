using GameNetcodeStuff;
using SawTapes.Behaviours.Tapes;
using SawTapes.Managers;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class SawBomb : PhysicsProp
{
    public bool hasBeenUsedForExplosiveGame = false;
    public bool isTransferred = false;
    public bool isContained = false;
    public bool hasBeenDefused = false;

    public PlayerControllerB aimedPlayer;
    public float movementSpeed;

    public EnemyAI aimedEnemy;
    public EnemyAI attachedEnemy;

    public AudioSource bombAudio;
    public AudioClip tickingSound;

    public Coroutine tickingCoroutine;
    public Coroutine transferBombCoroutine;
    public Coroutine slowDownCoroutine;

    public override void Start()
    {
        base.Start();
        movementSpeed = GameNetworkManager.Instance.localPlayerController.movementSpeed;
    }

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
        attachedEnemy?.KillEnemyOnOwnerClient(true);
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
        hasBeenUsedForExplosiveGame = true;
        DestroyObjectInHand(playerHeldBy);
    }

    public override void Update()
    {
        base.Update();

        if (!isHeld || isPocketed || playerHeldBy == null) return;
        if (isContained || transferBombCoroutine != null) return;
        if (playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;
        if (hasBeenDefused)
        {
            ShowAuraTargetedEnemy();
            return;
        }
        if (hasBeenUsedForExplosiveGame) return;

        SawTape sawTape = SawGameSTManager.GetSawTapeFromPlayer(playerHeldBy);
        if (sawTape == null || sawTape is not ExplosiveTape) return;

        ShowAuraTargetedPlayer();
    }

    public void ShowAuraTargetedEnemy()
    {
        if (Physics.Raycast(new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward), out RaycastHit hit, ConfigManager.eyeDistanceSurvival.Value, 524288, QueryTriggerInteraction.Collide))
        {
            EnemyAICollisionDetect enemyCollision = hit.collider.GetComponent<EnemyAICollisionDetect>();
            if (enemyCollision == null || enemyCollision.mainScript == null) return;

            aimedEnemy = enemyCollision.mainScript;
            CustomPassManager.SetupAuraForObjects([enemyCollision.mainScript.gameObject], SawTapes.redTransparentShader);
            return;
        }
        RemoveAuraFromEnemy();
    }

    public void ShowAuraTargetedPlayer()
    {
        Ray ray = new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, 3f);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.GetComponent<PlayerControllerB>() == playerHeldBy) continue;

            PlayerControllerB targetedPlayer = hit.collider.GetComponent<PlayerControllerB>();
            if (targetedPlayer == null) continue;

            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(targetedPlayer);
            if (playerBehaviour == null || !playerBehaviour.isInGame) continue;

            aimedPlayer = targetedPlayer;
            CustomPassManager.SetupAuraForObjects([targetedPlayer.gameObject], SawTapes.yellowTransparentShader);
            return;
        }
        RemoveAuraFromPlayer();
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown || playerHeldBy == null || transferBombCoroutine != null) return;
        if (aimedEnemy != null) AttachBombServerRpc((int)playerHeldBy.playerClientId, aimedEnemy.thisNetworkObject);
        if (aimedPlayer != null) TransferBombServerRpc((int)playerHeldBy.playerClientId, (int)aimedPlayer.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AttachBombServerRpc(int playerId, NetworkObjectReference enemyObject)
        => AttachBombClientRpc(playerId, enemyObject);

    [ClientRpc]
    public void AttachBombClientRpc(int playerId, NetworkObjectReference enemyObject)
    {
        if (!enemyObject.TryGet(out NetworkObject networkObject)) return;

        _ = StartCoroutine(AttachBombCoroutine(StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>(),
            networkObject.GetComponentInChildren<EnemyAI>()));
    }

    public IEnumerator AttachBombCoroutine(PlayerControllerB player, EnemyAI enemy)
    {
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (player == localPlayer) player.DropAllHeldItemsAndSync();

        yield return new WaitForSeconds(0.2f);

        attachedEnemy = enemy;
        parentObject = enemy.transform;
        hasHitGround = false;
        EnablePhysics(false);
        EnableItemMeshes(false);
        if (localPlayer.IsHost || localPlayer.IsServer) StartTickingForServer();
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

            slowDownCoroutine ??= StartCoroutine(SlowDownCoroutine(targetedPlayer));
            ObjectSTManager.ForceGrabObject(this, targetedPlayer);
        }

        if (localPlayer.IsHost || localPlayer.IsServer) StartTickingForServer();
        isTransferred = false;

        yield return new WaitForSeconds(3.8f);

        transferBombCoroutine = null;
    }

    public IEnumerator SlowDownCoroutine(PlayerControllerB player)
    {
        movementSpeed = player.movementSpeed;
        player.movementSpeed /= 2f;

        yield return new WaitForSeconds(4f);

        player.movementSpeed = movementSpeed;
        slowDownCoroutine = null;
    }

    public override void PocketItem()
    {
        base.PocketItem();

        RemoveAuraFromEnemy();
        RemoveAuraFromPlayer();
    }

    public override void DiscardItem()
    {
        base.DiscardItem();

        RemoveAuraFromEnemy();
        RemoveAuraFromPlayer();
        if (slowDownCoroutine != null)
        {
            GameNetworkManager.Instance.localPlayerController.movementSpeed = movementSpeed;
            StopCoroutine(slowDownCoroutine);
            slowDownCoroutine = null;
        }

        if (isTransferred || isContained || hasBeenDefused || deactivated) return;
        SpawnExplosion();
    }

    public void RemoveAuraFromEnemy()
    {
        if (aimedEnemy == null) return;

        CustomPassManager.RemoveAuraFromObjects([aimedEnemy.gameObject]);
        aimedEnemy = null;
    }

    public void RemoveAuraFromPlayer()
    {
        if (aimedPlayer == null) return;

        CustomPassManager.RemoveAuraFromObjects([aimedPlayer.gameObject]);
        aimedPlayer = null;
    }
}
