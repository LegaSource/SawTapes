using GameNetcodeStuff;
using SawTapes.Behaviours.Items;
using SawTapes.Files;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Tapes;

public class SurvivalTape : SawTape
{
    public HashSet<Shovel> shovels = [];
    public List<NetworkObject> spawnedEnemies = [];

    public ParticleSystem spawnParticle;
    public ParticleSystem teleportParticle;

    public override void Start()
    {
        base.Start();

        InstantiateAndAttachAudio(SawTapes.sawRecordingSurvival);
        subtitlesGame = SubtitleFile.survivalGameSubtitles;

        minPlayersAmount = ConfigManager.survivalMinPlayers.Value;
        maxPlayersAmount = ConfigManager.survivalMaxPlayers.Value;

        gameDuration = ConfigManager.survivalDuration.Value;
        billyValue = ConfigManager.survivalBillyValue.Value;
    }

    public override void ExecutePostGasActionsForClient(PlayerControllerB player)
    {
        base.ExecutePostGasActionsForClient(player);
        SpawnShovelServerRpc(player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnShovelServerRpc(Vector3 position)
        => shovels.Add(SawGameSTManager.SpawnItemFromNameForServer(Constants.SHOVEL, position) as Shovel);

    public override void ExecuteStartGameActionsForServer()
    {
        base.ExecuteStartGameActionsForServer();
        SpawnPursuerEyes();
    }

    public void SpawnPursuerEyes()
    {
        foreach (RandomScrapSpawn randomScrapSpawn in FindObjectsOfType<RandomScrapSpawn>())
        {
            if (randomScrapSpawn == null) return;

            if (!randomScrapSpawn.spawnedItemsCopyPosition)
                randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + (Vector3.up * SawTapes.pursuerEye.verticalOffset);

            _ = RoundManagerPatch.SpawnItem(SawTapes.pursuerEye.spawnPrefab, randomScrapSpawn.transform.position + (Vector3.up * 0.5f));
        }
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
        if (iterator % 5 != 0) return;
        if (gameDuration - iterator <= 5f) return;

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
        Vector3 spawnPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, 5);
        PlaySpawnParticleClientRpc(spawnPosition);

        yield return new WaitUntil(() => !spawnParticle.isPlaying);

        Destroy(spawnParticle.gameObject);
        NetworkObject networkObject = EnemySTManager.SpawnEnemyForServer(enemyType, spawnPosition);
        spawnedEnemies.Add(networkObject);
    }

    [ClientRpc]
    public void PlaySpawnParticleClientRpc(Vector3 position)
    {
        GameObject spawnObject = Instantiate(SawTapes.spawnParticle, position, Quaternion.identity);
        spawnParticle = spawnObject.GetComponent<ParticleSystem>();
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
        Vector3 endPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, 5);
        PlayTeleportParticleClientRpc(enemy.transform.position, endPosition);

        yield return new WaitUntil(() => !teleportParticle.isPlaying);

        Destroy(teleportParticle.gameObject);

        if (enemy == null || enemy.isEnemyDead || !enemy.IsSpawned) yield break;
        TeleportEnemyClientRpc(enemy.thisNetworkObject, endPosition);
    }

    [ClientRpc]
    public void PlayTeleportParticleClientRpc(Vector3 startPosition, Vector3 endPosition)
    {
        _ = Instantiate(SawTapes.teleportParticle, startPosition, Quaternion.identity);

        GameObject spawnObject = Instantiate(SawTapes.teleportParticle, endPosition, Quaternion.identity);
        teleportParticle = spawnObject.GetComponent<ParticleSystem>();
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

    public override bool ExecutePreEndGameActionForServer(bool isGameCancelled)
    {
        _ = base.ExecutePreEndGameActionForServer(isGameCancelled);

        EnemySTManager.DespawnEnemiesForServer(spawnedEnemies);
        ObjectSTManager.DestroyObjectsOfTypeAllForServer<PursuerEye>();
        foreach (Shovel shovel in shovels) ObjectSTManager.DestroyObjectOfTypeForServer(shovel);

        return players.All(p => p.isPlayerDead);
    }

    public override void EndGameForAllClients(bool isGameEnded)
    {
        base.EndGameForAllClients(isGameEnded);
        spawnedEnemies.Clear();
    }
}
