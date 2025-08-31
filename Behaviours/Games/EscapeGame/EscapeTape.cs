using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Managers;
using LegaFusionCore.Managers.NetworkManagers;
using SawTapes.Files;
using SawTapes.Managers;
using SawTapes.Values;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.EscapeGame;

public class EscapeTape : SawTape
{
    public HashSet<ChainEscape> chains = [];

    public SawEscape saw;
    public bool sawHasBeenUsed = false;

    public List<EscapeHazard> eligibleHazards = [];
    public HashSet<GameObject> hazards = [];

    public override void Start()
    {
        base.Start();

        subtitlesGame = SubtitleFile.escapeGameSubtitles;

        minPlayersAmount = ConfigManager.escapeMinPlayers.Value;
        maxPlayersAmount = ConfigManager.escapeMaxPlayers.Value;

        spawnShovel = true;
        gameDuration = ConfigManager.escapeDuration.Value;
        billyValue = ConfigManager.escapeBillyValue.Value;

        ParseHazards();
    }

    public void ParseHazards()
    {
        string[] hazardEntries = ConfigManager.escapeHazards.Value.Split(',');
        foreach (string entry in hazardEntries)
        {
            string[] keyValue = entry.Split(':');
            if (keyValue.Length == 6
                && int.TryParse(keyValue[1], out int weight)
                && bool.TryParse(keyValue[2], out bool spawnFacingAwayFromWall)
                && bool.TryParse(keyValue[3], out bool spawnFacingWall)
                && bool.TryParse(keyValue[4], out bool spawnWithBackToWall)
                && bool.TryParse(keyValue[5], out bool spawnWithBackFlushAgainstWall))
            {
                for (int i = 0; i < weight; i++)
                {
                    eligibleHazards.Add(new EscapeHazard(
                        keyValue[0],
                        spawnFacingAwayFromWall,
                        spawnFacingWall,
                        spawnWithBackToWall,
                        spawnWithBackFlushAgainstWall));
                }
            }
        }
        foreach (EscapeHazard escapeHazard in eligibleHazards.ToList())
        {
            GameObject hazard = SearchHazard(escapeHazard);
            if (hazard == null) _ = eligibleHazards.Remove(escapeHazard);
        }
    }

    public GameObject SearchHazard(EscapeHazard escapeHazard)
    {
        // Parcours de la liste des préfabriqués pour trouver le type de piège demandé
        GameObject hazard = null;
        foreach (NetworkPrefabsList networkPrefabList in NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists ?? Enumerable.Empty<NetworkPrefabsList>())
        {
            foreach (NetworkPrefab networkPrefab in networkPrefabList.PrefabList ?? Enumerable.Empty<NetworkPrefab>())
            {
                if (!networkPrefab.Prefab.name.Equals(escapeHazard.HazardName)) continue;
                hazard = networkPrefab.Prefab;
            }
            if (hazard != null) break;
        }
        return hazard;
    }

    public override void ExecutePostGasActionsForServer()
    {
        base.ExecutePostGasActionsForServer();
        _ = StartCoroutine(SetUpChainsCouroutine());
    }

    public IEnumerator SetUpChainsCouroutine()
    {
        Transform entrancePoint = STUtilities.FindMainEntrancePoint();
        yield return new WaitUntil(() => this.players.All(p => Vector3.Distance(p.transform.position, entrancePoint.position) < 5f));

        List<PlayerControllerB> players = this.players.ToList();
        for (int i = 0; i < players.Count - 1; i++)
        {
            PlayerControllerB player1 = players[i];
            PlayerControllerB player2 = players[i + 1];

            Vector3 position = ((player1.transform.position + player2.transform.position) / 2f) + (Vector3.up * 1.2f);
            GameObject gameObject = Instantiate(SawTapes.chainEscapeObj, position, Quaternion.identity, StartOfRound.Instance.propsContainer);
            gameObject.GetComponent<NetworkObject>().Spawn();

            ChainEscape chainEscape = gameObject.GetComponent<ChainEscape>();
            chainEscape.SetUpChainClientRpc((int)player1.playerClientId, (int)player2.playerClientId);

            _ = chains.Add(chainEscape);
        }
    }

    public override void ExecuteStartGameActionsForServer()
    {
        base.ExecuteStartGameActionsForServer();
        SpawnSawForServer();
    }

    public void SpawnSawForServer()
    {
        Vector3 position = STUtilities.GetFurthestPositionScrapSpawn(transform.position, SawTapes.sawEscape);
        saw = LFCObjectsManager.SpawnObjectForServer(SawTapes.sawEscape.spawnPrefab, position) as SawEscape;
        InitializeSawClientRpc(saw.GetComponent<NetworkObject>());
    }

    [ClientRpc]
    public void InitializeSawClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;
        saw = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as SawEscape;
    }

    public override bool DoGameForServer(int iterator)
    {
        if (players.All(p => p.isPlayerDead) || sawHasBeenUsed) return false;

        SpawnHazardsForServer(iterator);
        if (iterator % 20 != 0) return true;

        List<PlayerControllerB> livingPlayers = players.Where(p => !p.isPlayerDead).ToList();
        ShowAuraSawClientRpc((int)livingPlayers[Random.Range(0, livingPlayers.Count)].playerClientId);

        return true;
    }

    public void SpawnHazardsForServer(int iterator)
    {
        if (iterator % 5 != 0) return;

        foreach (PlayerControllerB player in players)
        {
            EscapeHazard escapeHazard = eligibleHazards[new System.Random().Next(eligibleHazards.Count)];
            GameObject hazard = SearchHazard(escapeHazard);
            if (hazard == null) continue;

            _ = StartCoroutine(SpawnHazardCoroutine(escapeHazard, hazard, player.transform.position));
        }
    }

    public IEnumerator SpawnHazardCoroutine(EscapeHazard escapeHazard, GameObject hazard, Vector3 position)
    {
        Vector3 spawnPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, 8f);
        LFCNetworkManager.Instance.PlayParticleClientRpc($"{LegaFusionCore.LegaFusionCore.modName}{LegaFusionCore.LegaFusionCore.bluePortalParticle.name}", spawnPosition, Quaternion.Euler(-90, 0, 0));

        yield return new WaitForSecondsRealtime(2f);

        GameObject hazardInstance = SawGameSTManager.SpawnHazard(hazard, spawnPosition, escapeHazard.SpawnFacingAwayFromWall, escapeHazard.SpawnFacingWall, escapeHazard.SpawnWithBackToWall, escapeHazard.SpawnWithBackFlushAgainstWall);
        if (hazardInstance != null) InitializeHazardClientRpc(hazardInstance.GetComponent<NetworkObject>());
    }

    [ClientRpc]
    public void InitializeHazardClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;
        _ = hazards.Add(networkObject.gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnHazardServerRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;
        SawGameSTManager.DespawnHazard(networkObject.gameObject);
    }

    [ClientRpc]
    public void ShowAuraSawClientRpc(int playerId)
    {
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (!players.Contains(localPlayer)) return;
        CustomPassManager.RemoveAuraFromObjects([saw.gameObject], SawTapes.modName);

        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        if (localPlayer != player) return;
        CustomPassManager.SetupAuraForObjects([saw.gameObject], LegaFusionCore.LegaFusionCore.wallhackShader, SawTapes.modName, Color.yellow);
    }

    public override bool ExecutePreEndGameActionForServer()
    {
        _ = base.ExecutePreEndGameActionForServer();

        chains.Where(c => c != null).ToList().ForEach(c => Destroy(c.gameObject));
        hazards.Where(h => h != null).ToList().ForEach(SawGameSTManager.DespawnHazard);
        if (players.All(p => p.isPlayerDead) || !sawHasBeenUsed)
        {
            LFCObjectsManager.DestroyObjectOfTypeForServer(saw);
            foreach (PlayerControllerB player in players)
            {
                if (player.isPlayerDead) continue;
                LFCNetworkManager.Instance.KillPlayerClientRpc((int)player.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
            }
            return true;
        }
        return false;
    }

    public override void OnDestroy()
    {
        chains.Where(c => c != null).ToList().ForEach(c => Destroy(c.gameObject));
        base.OnDestroy();
    }
}
