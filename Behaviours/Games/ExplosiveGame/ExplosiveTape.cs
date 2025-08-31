using GameNetcodeStuff;
using LegaFusionCore.Managers;
using LegaFusionCore.Managers.NetworkManagers;
using SawTapes.Files;
using SawTapes.Managers;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.ExplosiveGame;
public class ExplosiveTape : SawTape
{
    public SawBombExplosive sawBomb;
    public SawBoxExplosive sawBox;

    public override void Start()
    {
        base.Start();

        subtitlesGame = SubtitleFile.explosiveGameSubtitles;

        minPlayersAmount = ConfigManager.explosiveMinPlayers.Value;
        maxPlayersAmount = ConfigManager.explosiveMaxPlayers.Value;

        gameDuration = ConfigManager.explosiveDuration.Value;
        billyValue = ConfigManager.explosiveBillyValue.Value;
    }

    public override void ExecuteStartGameActionsForServer()
    {
        base.ExecuteStartGameActionsForServer();

        SpawnSawBomb();
        sawBomb.StartTickingForServer();
        SpawnSawBox();
    }

    public void SpawnSawBomb()
    {
        PlayerControllerB player = players.FirstOrDefault();
        if (player == null) return;

        sawBomb = LFCObjectsManager.SpawnObjectForServer(SawTapes.sawBombExplosive.spawnPrefab, player.transform.position) as SawBombExplosive;
        NetworkObject networkObject = sawBomb.GetComponent<NetworkObject>();
        InitializeSawBombClientRpc(networkObject);
        LFCNetworkManager.Instance.ForceGrabObjectClientRpc(networkObject, (int)player.playerClientId);
    }

    [ClientRpc]
    public void InitializeSawBombClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        sawBomb = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as SawBombExplosive;
        if (sawBomb == null) return;
    }

    public void SpawnSawBox()
    {
        foreach (Vector3 position in STUtilities.GetFurthestPositions(transform.position, 1))
        {
            GameObject sawBoxInstance = SawGameSTManager.SpawnHazard(SawTapes.sawBoxExplosiveObj, position, true, false, true, true);
            if (sawBoxInstance != null)
            {
                AffectSawBoxClientRpc(sawBoxInstance.GetComponent<NetworkObject>());
                break;
            }
        }
    }

    [ClientRpc]
    public void AffectSawBoxClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;
        sawBox = networkObject.gameObject.GetComponentInChildren<SawBoxExplosive>();
    }

    public override bool DoGameForServer(int iterator)
        => !(players.All(p => p.isPlayerDead) || sawBomb.hasBeenDestroyed);

    public override bool ExecutePreEndGameActionForServer()
    {
        _ = base.ExecutePreEndGameActionForServer();

        if (!sawBomb.hasBeenDestroyed) sawBomb.SpawnExplosionClientRpc();
        SawGameSTManager.DespawnHazard(sawBox.gameObject);

        return players.All(p => p.isPlayerDead);
    }
}
