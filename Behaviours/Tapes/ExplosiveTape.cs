using GameNetcodeStuff;
using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.MapObjects;
using SawTapes.Files;
using SawTapes.Managers;
using System.Collections;
using System.Collections.Generic;
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

    public override void ExecutePostGasActionsForServer()
    {
        base.ExecutePostGasActionsForServer();
        SpawnSawBox();
    }

    public void SpawnSawBox()
    {
        Vector3 position = STUtilities.GetFurthestPositions(transform.position, 1).FirstOrDefault();
        if (position == null) return;

        System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 587);

        List<RandomMapObject> nearbyMapObjects = FindObjectsOfType<RandomMapObject>().Where(m => Vector3.Distance(position, m.transform.position) <= 5f).ToList();
        // Recherche de la meilleure position de spawn autour de la position donnée
        if (nearbyMapObjects.Any())
        {
            // Prendre le premier objet proche, et placer la box à proximité de cet objet
            RandomMapObject nearestMapObject = nearbyMapObjects.First();
            position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(nearestMapObject.transform.position, 2f, default, random);
        }

        // Ajout de légers ajustements à la position pour placer la box au sol
        if (Physics.Raycast(position + (Vector3.up * 2f), Vector3.down, out RaycastHit hit, 80f, 268437760, QueryTriggerInteraction.Ignore))
        {
            position = hit.point;
        }
        else
        {
            SawTapes.mls.LogWarning("No surface detected to place the box.");
            return;
        }

        // Instancie la box à la position calculée
        GameObject gameObject = Instantiate(SawTapes.sawBoxObj, position, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);

        gameObject.transform.eulerAngles = new Vector3(0f, RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(position + (Vector3.up * 0.2f)), 0f);

        if (Physics.Raycast(gameObject.transform.position, -gameObject.transform.forward, out RaycastHit hitInfo, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
        {
            gameObject.transform.position = hitInfo.point;
            gameObject.transform.forward = hitInfo.normal;
            gameObject.transform.eulerAngles = new Vector3(0f, gameObject.transform.eulerAngles.y, 0f);
        }

        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Spawn(destroyWithScene: true);
        AffectSawBoxClientRpc(networkObject);
    }

    [ClientRpc]
    public void AffectSawBoxClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;
        sawBox = networkObject.gameObject.GetComponentInChildren<SawBox>();
    }

    public override void ExecuteStartGameActionsForServer()
    {
        base.ExecuteStartGameActionsForServer();

        SpawnSawBomb();
        sawBomb.StartTickingForServer();
        ShowSawBoxAuraClientRpc();
    }

    public void SpawnSawBomb()
    {
        PlayerControllerB player = players.FirstOrDefault();
        if (player == null) return;

        GameObject gameObject = Instantiate(SawTapes.sawBombObj, player.transform.position, Quaternion.identity, StartOfRound.Instance.propsContainer);

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
        STUtilities.ForceGrabObject(sawBomb, player);
    }

    [ClientRpc]
    public void ShowSawBoxAuraClientRpc()
        => CustomPassManager.SetupCustomPassForObjects([sawBox.gameObject]);

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
