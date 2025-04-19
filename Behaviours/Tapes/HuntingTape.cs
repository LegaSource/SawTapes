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

public class HuntingTape : SawTape
{
    public HashSet<Shovel> shovels = [];
    public List<NetworkObject> spawnedEnemies = [];
    public static Coroutine showAuraCoroutine;

    public override void Start()
    {
        base.Start();

        InstantiateAndAttachAudio(SawTapes.sawRecordingHunting);
        subtitlesGame = SubtitleFile.huntingGameSubtitles;

        minPlayersAmount = ConfigManager.huntingMinPlayers.Value;
        maxPlayersAmount = ConfigManager.huntingMaxPlayers.Value;

        gameDuration = ConfigManager.huntingDuration.Value;
        billyValue = ConfigManager.huntingBillyValue.Value;
    }

    public override void ExecutePostGasActionsForClient(PlayerControllerB player)
    {
        base.ExecutePostGasActionsForClient(player);

        SpawnReverseBearTrapServerRpc((int)player.playerClientId);
        SpawnShovelServerRpc(player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnReverseBearTrapServerRpc(int playerId)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        ReverseBearTrap reverseBearTrap = RoundManagerPatch.SpawnItem(SawTapes.reverseBearTrap.spawnPrefab, player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation) as ReverseBearTrap;
        reverseBearTrap.InitializeReverseBearTrapClientRpc((int)player.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnShovelServerRpc(Vector3 position)
        => shovels.Add(SawGameSTManager.SpawnItemFromNameForServer(Constants.SHOVEL, position) as Shovel);

    public override void ExecuteStartGameActionsForServer()
    {
        base.ExecuteStartGameActionsForServer();
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        Vector3[] spawnPositions = STUtilities.GetFurthestPositions(transform.position, playersAmount);
        if (spawnPositions.Length < playersAmount)
        {
            SawTapes.mls.LogWarning("Not enough positions available");
            EndGameForServer(true);
            return;
        }

        int index = 0;
        foreach (PlayerControllerB player in players)
        {
            SpawnEnemy(spawnPositions[index]);
            index++;
        }
        ShowEnemiesClientRpc(spawnedEnemies.Select(e => e.NetworkObjectId).ToArray());
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

    [ClientRpc]
    public void ShowEnemiesClientRpc(ulong[] enemyIds)
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (!players.Contains(player)) return;

        List<EnemyAI> enemies = [];
        foreach (ulong enemyId in enemyIds)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyId, out NetworkObject networkObject))
            {
                enemies.Add(networkObject.GetComponentInChildren<EnemyAI>());
                if (player.IsHost || player.IsServer) continue;

                spawnedEnemies.Add(networkObject);
            }
        }
        ShowAura(ConfigManager.huntingAura.Value);
    }

    public void ShowAura(float duration)
    {
        if (showAuraCoroutine != null) HUDManager.Instance.StopCoroutine(showAuraCoroutine);

        List<EnemyAI> enemies = [];
        foreach (NetworkObject spawnedEnemy in spawnedEnemies)
        {
            if (spawnedEnemy == null || !spawnedEnemy.IsSpawned) continue;
            enemies.Add(spawnedEnemy.GetComponentInChildren<EnemyAI>());
        }
        if (enemies.Count == 0) return;

        showAuraCoroutine = HUDManager.Instance.StartCoroutine(ShowAuraCoroutine(enemies, duration));
    }

    public IEnumerator ShowAuraCoroutine(List<EnemyAI> enemies, float duration)
    {
        List<GameObject> objects = enemies.Select(e => e.gameObject).ToList();
        foreach (SawKey sawKey in Resources.FindObjectsOfTypeAll<SawKey>())
        {
            if (sawKey == null || !sawKey.IsSpawned) continue;
            objects.Add(sawKey.gameObject);
        }
        CustomPassManager.SetupAuraForObjects(objects.ToArray(), SawTapes.redWallhackShader);

        yield return new WaitForSeconds(duration);

        CustomPassManager.RemoveAuraFromObjects(objects.ToArray());
        showAuraCoroutine = null;
    }

    public override bool DoGameForServer(int iterator)
        => !players.All(p => p.isPlayerDead || (p.TryGetComponent(out PlayerSTBehaviour b) && b.reverseBearTrap != null && b.reverseBearTrap.isReleased));

    public override bool ExecutePreEndGameActionForServer(bool isGameCancelled)
    {
        _ = base.ExecutePreEndGameActionForServer(isGameCancelled);

        bool hasLivingPlayer = false;
        foreach (PlayerControllerB player in players)
        {
            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(player);
            if (playerBehaviour?.reverseBearTrap == null
                || (!player.isPlayerDead && playerBehaviour.reverseBearTrap.isReleased))
            {
                hasLivingPlayer = true;
                continue;
            }

            ObjectSTManager.DestroyReverseBearTrapForServer(playerBehaviour.playerProperties);

            if (isGameCancelled || player.isPlayerDead) continue;
            SawTapesNetworkManager.Instance.KillPlayerClientRpc((int)player.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
        }
        EnemySTManager.DespawnEnemiesForServer(spawnedEnemies);
        ObjectSTManager.DestroyObjectsOfTypeAllForServer<SawKey>();
        ObjectSTManager.DestroyObjectsOfTypeAllForServer<PursuerEye>();
        foreach (Shovel shovel in shovels) ObjectSTManager.DestroyObjectOfTypeForServer(shovel);

        return !hasLivingPlayer;
    }

    public override void EndGameForAllClients(bool isGameEnded)
    {
        base.EndGameForAllClients(isGameEnded);
        spawnedEnemies.Clear();
    }
}
