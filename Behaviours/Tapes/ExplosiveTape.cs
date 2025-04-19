using GameNetcodeStuff;
using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.MapObjects;
using SawTapes.Files;
using SawTapes.Managers;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Tapes;
public class ExplosiveTape : SawTape
{
    public SawBomb sawBomb;
    public SawBox sawBox;

    public override void Start()
    {
        base.Start();

        InstantiateAndAttachAudio(SawTapes.sawRecordingExplosive);
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

        GameObject gameObject = Instantiate(SawTapes.sawBomb.spawnPrefab, player.transform.position, Quaternion.identity, StartOfRound.Instance.propsContainer);

        GrabbableObject grabbableObject = gameObject.GetComponent<GrabbableObject>();
        grabbableObject.fallTime = 0f;
        grabbableObject.Start();

        NetworkObject networkObject = grabbableObject.GetComponent<NetworkObject>();
        networkObject.Spawn();

        ForceGrabBombClientRpc(networkObject, (int)player.playerClientId);
    }

    [ClientRpc]
    public void ForceGrabBombClientRpc(NetworkObjectReference obj, int playerId)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        sawBomb = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as SawBomb;
        if (sawBomb == null) return;

        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        if (player == GameNetworkManager.Instance.localPlayerController) _ = StartCoroutine(ForceGrabBombCoroutine(player));
    }

    public IEnumerator ForceGrabBombCoroutine(PlayerControllerB player)
    {
        player.DropAllHeldItemsAndSync();
        yield return new WaitForSeconds(0.2f);
        ObjectSTManager.ForceGrabObject(sawBomb, player);
    }

    public void SpawnSawBox()
    {
        foreach (Vector3 position in STUtilities.GetFurthestPositions(transform.position, 1))
        {
            GameObject sawBoxInstance = SawGameSTManager.SpawnHazard(SawTapes.sawBoxObj, position, true, false, true, true);
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
        sawBox = networkObject.gameObject.GetComponentInChildren<SawBox>();
    }

    public override bool DoGameForServer(int iterator)
        => !(players.All(p => p.isPlayerDead) || sawBomb.hasExploded);

    public override bool ExecutePreEndGameActionForServer(bool isGameCancelled)
    {
        _ = base.ExecutePreEndGameActionForServer(isGameCancelled);

        sawBomb.SpawnExplosionClientRpc();
        SawGameSTManager.DespawnHazard(sawBox.gameObject);

        return players.All(p => p.isPlayerDead);
    }
}
