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

public class EscapeTape : SawTape
{
    public Chain chain;
    public Saw saw;
    public HashSet<Shovel> shovels = [];
    public bool sawHasBeenUsed = false;

    public override void Start()
    {
        base.Start();

        InstantiateAndAttachAudio(SawTapes.sawRecordingEscape);
        subtitlesGame = SubtitleFile.escapeGameSubtitles;

        minPlayersAmount = 2;
        maxPlayersAmount = 2;

        gameDuration = ConfigManager.escapeDuration.Value;
        billyValue = ConfigManager.escapeBillyValue.Value;
    }

    public override void ExecutePostGasActionsForClient(PlayerControllerB player)
    {
        base.ExecutePostGasActionsForClient(player);
        SpawnShovelServerRpc(player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnShovelServerRpc(Vector3 position)
        => shovels.Add(SawGameSTManager.SpawnItemFromNameForServer(Constants.SHOVEL, position) as Shovel);

    public override void ExecutePostGasActionsForServer()
    {
        base.ExecutePostGasActionsForServer();
        _ = StartCoroutine(SetUpChainsCouroutine());
    }

    public IEnumerator SetUpChainsCouroutine()
    {
        PlayerControllerB[] players = base.players.ToArray();
        if (players.Length == 2)
        {
            yield return new WaitUntil(() => Vector3.Distance(players[0].transform.position, players[1].transform.position) < 8f);

            Vector3 position = ((players[0].transform.position + players[1].transform.position) / 2f) + (Vector3.up * 1.2f);
            chain = RoundManagerPatch.SpawnItem(SawTapes.chainObj, position) as Chain;
            chain.SetUpChainClientRpc((int)players[0].playerClientId, (int)players[1].playerClientId);
        }
    }

    public override void ExecuteStartGameActionsForServer()
    {
        base.ExecuteStartGameActionsForServer();

        PlayerControllerB player = players.FirstOrDefault();
        if (player == null) return;

        SpawnSaw(player.transform.position);
        AddPathGuide(player);
    }

    public void SpawnSaw(Vector3 position)
    {
        position = STUtilities.GetFurthestPositionScrapSpawn(position, SawTapes.sawItem);
        saw = RoundManagerPatch.SpawnItem(SawTapes.sawItem.spawnPrefab, position) as Saw;
        SawTapesNetworkManager.Instance.SetScrapValueClientRpc(saw.GetComponent<NetworkObject>(), ConfigManager.sawValue.Value);
    }

    public void AddPathGuide(PlayerControllerB player)
    {
        PathGuideBehaviour pathGuide = player.gameObject.AddComponent<PathGuideBehaviour>();
        pathGuide.saw = saw;
        pathGuide.sawTape = this;
        pathGuide.players = players.ToHashSet();
    }

    public override bool DoGameForServer(int iterator)
        => !(players.All(p => p.isPlayerDead) || sawHasBeenUsed);

    [ClientRpc]
    public void PlayerEndPathGuideClientRpc(int playerId, NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        if (player != GameNetworkManager.Instance.localPlayerController) return;

        Saw saw = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as Saw;
        CustomPassManager.SetupCustomPassForObjects([saw.gameObject]);
    }

    [ClientRpc]
    public void SpawnPathParticleClientRpc(Vector3 leftPosition, Vector3 rightPosition, int[] playerIds)
    {
        if (GameNetworkManager.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.localPlayerController.IsServer) return;

        foreach (int playerId in playerIds)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (GameNetworkManager.Instance.localPlayerController == player)
            {
                SawGameSTManager.SpawnPathParticle(leftPosition);
                SawGameSTManager.SpawnPathParticle(rightPosition);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TeleportSawToPlayerServerRpc(int playerId)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        if (Vector3.Distance(saw.transform.position, player.transform.position) > ConfigManager.escapeAuraDistance.Value) return;

        SawTapesNetworkManager.Instance.ChangeObjectPositionClientRpc(saw.GetComponent<NetworkObject>(), player.transform.position + (Vector3.up * 0.5f));
    }

    public override bool ExecutePreEndGameActionForServer(bool isGameCancelled)
    {
        _ = base.ExecutePreEndGameActionForServer(isGameCancelled);

        DestroyChain();
        DestroyPathGuide();
        foreach (Shovel shovel in shovels) ObjectSTManager.DestroyObjectOfTypeForServer(shovel);
        if (players.All(p => p.isPlayerDead) || !sawHasBeenUsed)
        {
            ObjectSTManager.DestroyObjectOfTypeForServer(saw);
            if (!isGameCancelled)
            {
                foreach (PlayerControllerB player in players)
                {
                    if (player.isPlayerDead) continue;
                    SawTapesNetworkManager.Instance.KillPlayerClientRpc((int)player.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
                }
            }
            return true;
        }
        return false;
    }

    public void DestroyChain()
    {
        if (chain != null)
        {
            NetworkObject networkObject = chain.GetComponent<NetworkObject>();
            if (networkObject == null || !networkObject.IsSpawned) return;

            networkObject.Despawn(destroy: true);
        }
    }

    public void DestroyPathGuide()
    {
        PathGuideBehaviour pathGuide = players.Select(p => p.GetComponent<PathGuideBehaviour>()).FirstOrDefault();
        if (pathGuide != null) Destroy(pathGuide);
    }

    public override void EndGameForAllClients(bool isGameEnded)
    {
        base.EndGameForAllClients(isGameEnded);
        sawHasBeenUsed = false;
    }
}
