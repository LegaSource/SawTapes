using BepInEx;
using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Managers;
using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Utilities;
using SawTapes.Behaviours.Enemies;
using SawTapes.Files.Values;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games;

public class SawTape : PhysicsProp
{
    public bool isGameStarted = false;
    public bool isPlayerFinded = false;
    public bool spawnShovel = false;
    public float delayTimer = 10f;

    public int gameDuration;
    public int billyValue;

    public AudioSource sawTheme;
    public AudioSource sawRecording;
    public HashSet<SubtitleMapping> subtitlesGame = [];

    public ParticleSystem steamParticle;
    public AudioSource steamAudio;

    public int minPlayersAmount;
    public int maxPlayersAmount;
    public int playersAmount;
    public HashSet<PlayerControllerB> players = [];

    public HashSet<Shovel> shovels = [];

    public override void Update()
    {
        base.Update();
        FindPlayerInRange();
    }

    public void FindPlayerInRange()
    {
        if (isGameStarted || isPlayerFinded) return;

        delayTimer -= Time.deltaTime;
        if (delayTimer > 0f) return;

        PlayerControllerB localPlayer = GameNetworkManager.Instance?.localPlayerController;
        if (localPlayer == null) return;
        if (!localPlayer.IsHost && !localPlayer.IsServer) return;

        PlayerControllerB player = StartOfRound.Instance.allPlayerScripts
            .FirstOrDefault(p => STUtilities.IsEligiblePlayer(p) && Vector3.Distance(p.transform.position, transform.position) <= ConfigManager.gassingDistance.Value);
        if (player == null) return;

        isPlayerFinded = true;
        delayTimer = 10f;

        players.Clear();
        _ = players.Add(player);

        if (!SetPlayersAmount()) return;
        SelectPlayers();
    }

    public bool SetPlayersAmount()
    {
        playersAmount = StartOfRound.Instance.allPlayerScripts.Count(STUtilities.IsEligiblePlayer);
        if (playersAmount < minPlayersAmount) return false;

        /*
         * Si minPlayersAmount = -1 -> on prend tous les joueurs
         * Si maxPlayersAmount = -1 -> on prend un nombre de joueurs aléatoires entre le minPlayersAmount et le nombre de joueurs disponibles
         */
        if (maxPlayersAmount == -1)
        {
            minPlayersAmount = minPlayersAmount == -1 ? playersAmount : minPlayersAmount;
            maxPlayersAmount = playersAmount;
        }
        playersAmount = new System.Random().Next(minPlayersAmount, Mathf.Min(playersAmount, maxPlayersAmount));
        return true;
    }

    public void SelectPlayers()
    {
        for (int i = 1; i < playersAmount; i++)
        {
            List<PlayerControllerB> eligiblePlayers = StartOfRound.Instance.allPlayerScripts
                .Where(p => STUtilities.IsEligiblePlayer(p) && !players.Contains(p))
                .ToList();

            PlayerControllerB player = eligiblePlayers.Count > 0
                ? eligiblePlayers[Random.Range(0, eligiblePlayers.Count)]
                : null;

            if (player == null)
            {
                SawTapes.mls.LogWarning("Not enough players to play the game");
                return;
            }
            _ = players.Add(player);
        }
        AffectPlayersClientRpc(players.Select(p => (int)p.playerClientId).ToArray());
        ApplyGasClientRpc();
        ExecutePostGasActionsForServer();
    }

    [ClientRpc]
    public void AffectPlayersClientRpc(int[] playerIds)
    {
        SawTapes.sawTape = this;

        players.Clear();
        foreach (int playerId in playerIds)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            _ = players.Add(player);
        }
    }

    [ClientRpc]
    public void ApplyGasClientRpc()
    {
        if (!players.Contains(GameNetworkManager.Instance.localPlayerController)) return;
        _ = StartCoroutine(ApplyGasCoroutine());
    }

    public IEnumerator ApplyGasCoroutine()
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        HUDManagerPatch.isFlashFilterUsed = true;

        steamParticle.gameObject.transform.SetParent(player.transform);
        steamParticle.transform.SetPositionAndRotation(player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation);
        steamParticle.Play();

        steamAudio.gameObject.transform.SetParent(player.transform);
        steamAudio.transform.position = player.gameplayCamera.transform.position;
        steamAudio.PlayOneShot(steamAudio.clip);

        float timePassed = 0f;
        while (timePassed < 5f)
        {
            yield return new WaitForSeconds(0.2f);
            timePassed += 0.2f;

            ApplyGasEffects(true, intensity: timePassed);
        }

        yield return new WaitForSeconds(1f);

        ExecutePostGasActionsForClient(player);
    }

    public void ApplyGasEffects(bool isBeingGassed, float intensity = 0f)
    {
        HUDManager.Instance.HideHUD(isBeingGassed);
        if (isBeingGassed)
        {
            HUDManager.Instance.flashbangScreenFilter.weight = Mathf.Min(1f, intensity / 5f);
            return;
        }
        HUDManagerPatch.isFlashFilterUsed = false;
        HUDManager.Instance.flashbangScreenFilter.weight = 0f;
    }

    public virtual void ExecutePostGasActionsForClient(PlayerControllerB player)
    {
        Transform entrancePoint = STUtilities.FindMainEntrancePoint();
        LFCNetworkManager.Instance.TeleportPlayerServerRpc((int)player.playerClientId, entrancePoint.position, false, false, true, true, entrancePoint.eulerAngles.y, true);
        ApplyGasEffects(false);
        if (spawnShovel) SpawnShovelServerRpc(player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward, (int)player.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnShovelServerRpc(Vector3 position, int playerId)
    {
        GameObject gameObject = LFCUtilities.GetPrefabFromName(Constants.SHOVEL);
        GrabbableObject grabbableObject = LFCObjectsManager.SpawnObjectForServer(gameObject, position);
        _ = shovels.Add(grabbableObject as Shovel);

        LFCNetworkManager.Instance.ForceGrabObjectClientRpc(grabbableObject.GetComponent<NetworkObject>(), playerId);
    }

    public virtual void ExecutePostGasActionsForServer()
    {
        PlayerControllerB player = players.FirstOrDefault();
        if (player == null) return;

        LFCNetworkManager.Instance.ForceGrabObjectClientRpc(GetComponent<NetworkObject>(), (int)player.playerClientId);
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown || playerHeldBy == null || sawRecording.isPlaying) return;

        PlayRecordingServerRpc();

        if (isGameStarted) return;
        if (!players.Contains(playerHeldBy))
        {
            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_IMPAC_TESTED_PLAYER);
            return;
        }

        PlaySawThemeServerRpc();
        _ = StartCoroutine(BeginSawGameCoroutine());
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayRecordingServerRpc()
        => PlayRecordingClientRpc();

    [ClientRpc]
    public void PlayRecordingClientRpc()
    {
        sawRecording.Play();
        if (ConfigManager.isSubtitles.Value) _ = StartCoroutine(ShowSubtitles());
    }

    public IEnumerator ShowSubtitles()
    {
        while (sawRecording.isPlaying)
        {
            string subtitleText = subtitlesGame.Where(s => s.Timestamp <= sawRecording.time).OrderByDescending(s => s.Timestamp).FirstOrDefault()?.Text;
            if (!string.IsNullOrEmpty(subtitleText))
            {
                HUDManagerPatch.subtitleText.text = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, transform.position) <= 25
                    ? subtitleText
                    : "";
            }
            yield return null;
        }
        HUDManagerPatch.subtitleText.text = "";
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlaySawThemeServerRpc()
        => PlaySawThemeClientRpc();

    [ClientRpc]
    public void PlaySawThemeClientRpc()
    {
        if (!ConfigManager.isSawTheme.Value || sawTheme.isPlaying) return;

        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (!players.Contains(player)) return;

        sawTheme.Play();
        sawTheme.gameObject.transform.SetParent(player.transform);
    }

    public IEnumerator BeginSawGameCoroutine()
    {
        yield return new WaitUntil(() => sawRecording.isPlaying);
        yield return new WaitUntil(() => !sawRecording.isPlaying);

        if (sawTheme != null) sawTheme.volume *= 1.5f;
        BeginSawGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void BeginSawGameServerRpc()
    {
        ExecuteStartGameActionsForServer();
        StartGameClientRpc();
    }

    public virtual void ExecuteStartGameActionsForServer() { }

    [ClientRpc]
    public void StartGameClientRpc()
        => ExecuteStartGameActionsForAllClients();

    public virtual void ExecuteStartGameActionsForAllClients()
    {
        isGameStarted = true;
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

        if (player.IsHost || player.IsServer) _ = StartCoroutine(StartGameCoroutine());

        if (!players.Contains(player)) return;
        _ = HUDManager.Instance.StartCoroutine(HUDManagerPatch.StartChronoCoroutine(gameDuration));
    }

    public IEnumerator StartGameCoroutine()
    {
        int timePassed = 0;
        while (timePassed < gameDuration)
        {
            if (!DoGameForServer(timePassed)) break;
            if (StartOfRound.Instance.shipIsLeaving)
            {
                players.ToList().ForEach(p => LFCNetworkManager.Instance.KillPlayerClientRpc((int)p.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown));
                break;
            }
            yield return new WaitForSecondsRealtime(1f);
            timePassed++;
        }
        EndGameForServer();
    }

    public virtual bool DoGameForServer(int iterator) => true;

    public virtual void EndGameForServer()
    {
        bool isGameOver = ExecutePreEndGameActionForServer();
        EndGameClientRpc();

        PlayerControllerB player = players.FirstOrDefault(p => !p.isPlayerDead);
        if (player == null || isGameOver) return;
        _ = StartCoroutine(EndGameForServerCoroutine(player));
    }

    public virtual bool ExecutePreEndGameActionForServer()
    {
        shovels.ToList().ForEach(LFCObjectsManager.DestroyObjectOfTypeForServer);
        return true;
    }

    [ClientRpc]
    public void EndGameClientRpc()
    {
        foreach (PlayerControllerB player in players)
        {
            if (player != GameNetworkManager.Instance.localPlayerController) continue;

            Destroy(sawTheme.gameObject);
            if (!HUDManagerPatch.chronoText.text.IsNullOrWhiteSpace()) HUDManagerPatch.isChronoEnded = true;
            player.DropAllHeldItemsAndSync();
        }

        CustomPassManager.RemoveAuraByTag(SawTapes.modName);
    }

    public IEnumerator EndGameForServerCoroutine(PlayerControllerB player)
    {
        SpawnBilly(player);
        yield return new WaitForSecondsRealtime(1f);
        LFCObjectsManager.DestroyObjectOfTypeForServer(this);
    }

    public void SpawnBilly(PlayerControllerB player)
    {
        GameObject[] allAINodes = GameObject.FindGameObjectsWithTag("AINode");
        float maxDistance = 15f;
        float minDistance = 8f;
        Vector3 spawnPosition = player.transform.position;

        foreach (GameObject node in allAINodes)
        {
            if (node == null) continue;

            float distanceToPlayer = Vector3.Distance(player.transform.position, node.transform.position);
            if (distanceToPlayer >= minDistance && distanceToPlayer <= maxDistance)
            {
                Vector3 navMeshPosition = RoundManager.Instance.GetNavMeshPosition(node.transform.position, RoundManager.Instance.navHit);
                if (RoundManager.Instance.GotNavMeshPositionResult)
                {
                    spawnPosition = navMeshPosition;
                    break;
                }
            }
        }

        NetworkObject networkObject = EnemySTManager.SpawnEnemyForServer(SawTapes.billyAnnouncementEnemy, spawnPosition);
        SpawnBillyClientRpc(networkObject, (int)player.playerClientId, billyValue);
    }

    [ClientRpc]
    public void SpawnBillyClientRpc(NetworkObjectReference enemyObject, int playerId, int billyValue)
    {
        if (!enemyObject.TryGet(out NetworkObject networkObject)) return;

        EnemyAI enemy = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
        if (enemy != null && enemy is BillyAnnouncement billy)
        {
            billy.targetPlayer = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            billy.billyValue = billyValue;
        }
    }
}
