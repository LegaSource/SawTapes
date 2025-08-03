using GameNetcodeStuff;
using LegaFusionCore.Managers;
using LegaFusionCore.Managers.NetworkManagers;
using SawTapes.Files;
using SawTapes.Managers;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.HuntingGame;

public class HuntingTape : SawTape
{
    public Dictionary<PlayerControllerB, RBTrapHunting> playerRBTs = [];
    public List<NetworkObject> spawnedEnemies = [];
    public Coroutine showAuraCoroutine;

    public override void Start()
    {
        base.Start();

        subtitlesGame = SubtitleFile.huntingGameSubtitles;

        minPlayersAmount = ConfigManager.huntingMinPlayers.Value;
        maxPlayersAmount = ConfigManager.huntingMaxPlayers.Value;

        spawnShovel = true;
        gameDuration = ConfigManager.huntingDuration.Value;
        billyValue = ConfigManager.huntingBillyValue.Value;
    }

    public override void ExecutePostGasActionsForClient(PlayerControllerB player)
    {
        base.ExecutePostGasActionsForClient(player);

        SpawnReverseBearTrapServerRpc((int)player.playerClientId);
        SpawnMonsterEyeServerRpc(player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward, (int)player.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnReverseBearTrapServerRpc(int playerId)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        RBTrapHunting rbt = LFCObjectsManager.SpawnObjectForServer(SawTapes.rBTrapHunting.spawnPrefab, player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation) as RBTrapHunting;
        rbt.InitializeReverseBearTrapClientRpc((int)player.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnMonsterEyeServerRpc(Vector3 position, int playerId)
    {
        GrabbableObject grabbableObject = LFCObjectsManager.SpawnObjectForServer(SawTapes.billyPuppetHunting.spawnPrefab, position);
        LFCNetworkManager.Instance.ForceGrabObjectClientRpc(grabbableObject.GetComponent<NetworkObject>(), playerId);
    }

    public override void ExecuteStartGameActionsForServer()
    {
        base.ExecuteStartGameActionsForServer();
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        Vector3[] spawnPositions = STUtilities.GetFurthestPositions(transform.position, playersAmount);
        int index = 0;
        foreach (PlayerControllerB player in players)
        {
            SpawnEnemy(spawnPositions[index]);
            index++;
        }
    }

    public void SpawnEnemy(Vector3 spawnPosition)
    {
        List<EnemyType> eligibleEnemies = SawTapes.allEnemies
            .Where(e => e.canDie && !e.isOutsideEnemy && ConfigManager.huntingEnemies.Value.Contains(e.enemyName))
            .ToList();

        EnemyType enemyType = eligibleEnemies.Count > 0
            ? eligibleEnemies[Random.Range(0, eligibleEnemies.Count)]
            : null;

        NetworkObject networkObject = EnemySTManager.SpawnEnemyForServer(enemyType, spawnPosition);
        spawnedEnemies.Add(networkObject);
    }

    public void ShowAura(float duration)
    {
        if (showAuraCoroutine != null)
        {
            StopCoroutine(showAuraCoroutine);
            showAuraCoroutine = null;
        }

        List<EnemyAI> enemies = [];
        foreach (NetworkObject spawnedEnemy in spawnedEnemies)
        {
            if (spawnedEnemy == null || !spawnedEnemy.IsSpawned) continue;
            EnemyAI enemyAI = spawnedEnemy.GetComponentInChildren<EnemyAI>();
            if (enemyAI != null) enemies.Add(enemyAI);
        }
        if (enemies.Count == 0) return;

        showAuraCoroutine = StartCoroutine(SawGameSTManager.ShowAuraForHuntCoroutine(enemies.ToArray(), duration));
    }

    public override bool DoGameForServer(int iterator)
        => !players.All(p => p.isPlayerDead || (playerRBTs.TryGetValue(p, out RBTrapHunting rbt) && rbt.isReleased));

    public override bool ExecutePreEndGameActionForServer()
    {
        _ = base.ExecutePreEndGameActionForServer();

        bool hasLivingPlayer = false;
        foreach (PlayerControllerB player in players)
        {
            if (!playerRBTs.TryGetValue(player, out RBTrapHunting rbt)) continue;
            if (!player.isPlayerDead && rbt.isReleased)
            {
                hasLivingPlayer = true;
                continue;
            }

            LFCObjectsManager.DestroyObjectOfTypeForServer(rbt);

            if (player.isPlayerDead) continue;
            LFCNetworkManager.Instance.KillPlayerClientRpc((int)player.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
        }
        EnemySTManager.DespawnEnemiesForServer(spawnedEnemies);
        LFCObjectsManager.DestroyObjectsOfTypeAllForServer<SawKeyHunting>();
        LFCObjectsManager.DestroyObjectsOfTypeAllForServer<BillyPuppetHunting>();

        return !hasLivingPlayer;
    }
}
