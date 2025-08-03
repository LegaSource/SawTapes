using GameNetcodeStuff;
using LegaFusionCore.Managers;
using LegaFusionCore.Managers.NetworkManagers;
using SawTapes.Files;
using SawTapes.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.SurvivalGame;

public class SurvivalTape : SawTape
{
    public List<NetworkObject> spawnedEnemies = [];

    public override void Start()
    {
        base.Start();

        subtitlesGame = SubtitleFile.survivalGameSubtitles;

        minPlayersAmount = ConfigManager.survivalMinPlayers.Value;
        maxPlayersAmount = ConfigManager.survivalMaxPlayers.Value;

        spawnShovel = true;
        gameDuration = ConfigManager.survivalDuration.Value;
        billyValue = ConfigManager.survivalBillyValue.Value;
    }

    public override void ExecutePostGasActionsForClient(PlayerControllerB player)
    {
        base.ExecutePostGasActionsForClient(player);
        SpawnMonsterEyeServerRpc(player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward, (int)player.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnMonsterEyeServerRpc(Vector3 position, int playerId)
    {
        GrabbableObject grabbableObject = LFCObjectsManager.SpawnObjectForServer(SawTapes.billyPuppetSurvival.spawnPrefab, position);
        LFCNetworkManager.Instance.ForceGrabObjectClientRpc(grabbableObject.GetComponent<NetworkObject>(), playerId);
    }

    public override bool DoGameForServer(int iterator)
    {
        _ = base.DoGameForServer(iterator);

        if (players.All(p => p.isPlayerDead)) return false;

        SpawnEnemies(iterator);
        SetEnemiesTargets();

        return true;
    }

    public void SpawnEnemies(int iterator)
    {
        if (iterator % 10 != 0) return;
        if (gameDuration - iterator <= 10f) return;

        foreach (PlayerControllerB player in players)
        {
            List<EnemyType> eligibleEnemies = SawTapes.allEnemies
                .Where(e => ConfigManager.survivalEnemies.Value.Contains(e.enemyName))
                .ToList();

            EnemyType enemyType = eligibleEnemies.Count > 0
                ? eligibleEnemies[Random.Range(0, eligibleEnemies.Count)]
                : null;

            _ = StartCoroutine(SpawnEnemyCoroutine(enemyType, player.transform.position));
        }
    }

    public IEnumerator SpawnEnemyCoroutine(EnemyType enemyType, Vector3 position)
    {
        Vector3 spawnPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, 5f);
        LFCNetworkManager.Instance.PlayParticleClientRpc($"{LegaFusionCore.LegaFusionCore.modName}BluePortalParticle", spawnPosition, Quaternion.Euler(-90, 0, 0));

        yield return new WaitForSecondsRealtime(2f);

        NetworkObject networkObject = EnemySTManager.SpawnEnemyForServer(enemyType, spawnPosition);
        spawnedEnemies.Add(networkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TeleportEnemyServerRpc(NetworkObjectReference enemyObject, Vector3 position)
    {
        if (!enemyObject.TryGet(out NetworkObject networkObject)) return;

        EnemyAI enemy = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
        if (enemy == null) return;

        _ = StartCoroutine(TeleportEnemyCoroutine(enemy, position));
    }

    public IEnumerator TeleportEnemyCoroutine(EnemyAI enemy, Vector3 position)
    {
        Vector3 endPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, 5f);
        LFCNetworkManager.Instance.PlayParticleClientRpc($"{LegaFusionCore.LegaFusionCore.modName}RedPortalParticle", endPosition, Quaternion.Euler(-90, 0, 0));

        yield return new WaitForSecondsRealtime(2f);

        if (enemy == null || enemy.isEnemyDead || !enemy.IsSpawned) yield break;
        TeleportEnemyClientRpc(enemy.thisNetworkObject, endPosition);
    }

    [ClientRpc]
    public void TeleportEnemyClientRpc(NetworkObjectReference enemyObject, Vector3 position)
    {
        if (!enemyObject.TryGet(out NetworkObject networkObject)) return;

        EnemyAI enemy = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
        if (enemy == null) return;

        enemy.serverPosition = position;
        enemy.transform.position = position;
        _ = enemy.agent.Warp(position);
        enemy.SyncPositionToClients();
    }

    public void SetEnemiesTargets()
    {
        foreach (NetworkObject spawnedEnemy in spawnedEnemies)
        {
            if (spawnedEnemy == null) continue;

            EnemyAI enemy = spawnedEnemy.GetComponentInChildren<EnemyAI>();
            if (enemy?.thisNetworkObject == null || !enemy.thisNetworkObject.IsSpawned) continue;
            if (enemy.isEnemyDead) continue;

            PlayerControllerB closestPlayer = players.OrderBy(p => Vector3.Distance(p.transform.position, enemy.transform.position)).FirstOrDefault();
            if (closestPlayer == null) continue;

            enemy.SetMovingTowardsTargetPlayer(closestPlayer);
        }
    }

    public override bool ExecutePreEndGameActionForServer()
    {
        _ = base.ExecutePreEndGameActionForServer();

        EnemySTManager.DespawnEnemiesForServer(spawnedEnemies);
        LFCObjectsManager.DestroyObjectsOfTypeAllForServer<BillyPuppetSurvival>();

        return players.All(p => p.isPlayerDead);
    }
}
