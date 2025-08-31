using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Managers;
using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Registries;
using SawTapes.Behaviours.Items;
using SawTapes.Files;
using SawTapes.Managers;
using System.Collections;
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
        SpawnBillyHuntingServerRpc(player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward, (int)player.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnReverseBearTrapServerRpc(int playerId)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        RBTrapHunting rbt = LFCObjectsManager.SpawnObjectForServer(SawTapes.rBTrapHunting.spawnPrefab, player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation) as RBTrapHunting;
        rbt.InitializeReverseBearTrapClientRpc((int)player.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBillyHuntingServerRpc(Vector3 position, int playerId)
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
        AffectEnemyClientRpc(networkObject);
    }

    [ClientRpc]
    public void AffectEnemyClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;
        spawnedEnemies.Add(networkObject);
    }

    public void ShowAura(float duration)
    {
        if (showAuraCoroutine != null)
        {
            StopCoroutine(showAuraCoroutine);
            showAuraCoroutine = null;
        }

        GameObject[] enemies = spawnedEnemies
            .Where(e => e != null && e.IsSpawned && e.GetComponentInChildren<EnemyAI>() != null)
            .Select(e => e.gameObject)
            .ToArray();

        showAuraCoroutine = StartCoroutine(ShowAuraCoroutine(enemies, duration));
    }

    public static IEnumerator ShowAuraCoroutine(GameObject[] enemies, float duration)
    {
        GameObject[] enemiesObjects = enemies.Select(e => e.gameObject).ToArray();
        if (enemiesObjects.Length > 0) CustomPassManager.SetupAuraForObjects(enemiesObjects, LegaFusionCore.LegaFusionCore.wallhackShader, SawTapes.modName, Color.red);

        GameObject[] objects = LFCSpawnRegistry.GetAllAs<SawKeyHunting>().Select(s => s.gameObject).ToArray();
        if (objects.Length > 0) CustomPassManager.SetupAuraForObjects(objects, LegaFusionCore.LegaFusionCore.wallhackShader, SawTapes.modName, Color.yellow);

        yield return new WaitForSeconds(duration);

        if (enemiesObjects.Length > 0) CustomPassManager.RemoveAuraFromObjects(enemiesObjects, SawTapes.modName);
        if (objects.Length > 0) CustomPassManager.RemoveAuraFromObjects(objects, SawTapes.modName);
    }

    public override bool DoGameForServer(int iterator)
        => !players.All(p => p.isPlayerDead || (playerRBTs.TryGetValue(p, out RBTrapHunting rbt) && rbt.isReleased));

    public override bool ExecutePreEndGameActionForServer()
    {
        _ = base.ExecutePreEndGameActionForServer();

        PlayerControllerB activePlayer = null;
        foreach (PlayerControllerB player in players)
        {
            if (!playerRBTs.TryGetValue(player, out RBTrapHunting rbt)) continue;
            if (!player.isPlayerDead && rbt.isReleased)
            {
                if (activePlayer == null) activePlayer = player;
                continue;
            }

            LFCObjectsManager.DestroyObjectOfTypeForServer(rbt);

            if (player.isPlayerDead) continue;
            LFCNetworkManager.Instance.KillPlayerClientRpc((int)player.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
        }
        EnemySTManager.DespawnEnemiesForServer(spawnedEnemies);
        LFCObjectsManager.DestroyObjectsOfTypeAllForServer<SawKeyHunting>();
        LFCObjectsManager.DestroyObjectsOfTypeAllForServer<BillyPuppetHunting>();

        // Si joueur en vie faire apparaître la récompense sur le premier récupéré et retourner faux à game over
        if (activePlayer != null)
        {
            Vector3 position = activePlayer.gameplayCamera.transform.position + activePlayer.gameplayCamera.transform.forward;
            BillyPuppetHM billy = LFCObjectsManager.SpawnObjectForServer(SawTapes.billyPuppetHM.spawnPrefab, position) as BillyPuppetHM;
            billy.InitializeForServer();
            return false;
        }
        return true;
    }
}
