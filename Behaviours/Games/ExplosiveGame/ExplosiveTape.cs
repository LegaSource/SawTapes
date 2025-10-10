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
        InitializeSawBombEveryoneRpc(networkObject);
        LFCNetworkManager.Instance.ForceGrabObjectEveryoneRpc(networkObject, (int)player.playerClientId);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void InitializeSawBombEveryoneRpc(NetworkObjectReference obj)
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
                AffectSawBoxEveryoneRpc(sawBoxInstance.GetComponent<NetworkObject>());
                break;
            }
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void AffectSawBoxEveryoneRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;
        sawBox = networkObject.gameObject.GetComponentInChildren<SawBoxExplosive>();
    }

    public override bool DoGameForServer(int iterator) => !(players.All(p => p.isPlayerDead) || sawBomb.hasBeenDestroyed);

    public override bool ExecutePreEndGameActionForServer()
    {
        _ = base.ExecutePreEndGameActionForServer();

        if (!sawBomb.hasBeenDestroyed) sawBomb.SpawnExplosionEveryoneRpc();
        SawGameSTManager.DespawnHazard(sawBox.gameObject);

        return players.All(p => p.isPlayerDead);
    }
}
