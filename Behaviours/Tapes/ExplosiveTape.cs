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
    public SawKey sawKey;
    public HoarderBugAI hoardingBug;

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

    public void PrepareDefusing(PlayerControllerB player)
    {
        gameDuration += ConfigManager.explosiveExtraDuration.Value;

        Vector3 position = STUtilities.GetClosestPositionScrapSpawn(player.transform.position, 17.5f, SawTapes.sawKey)
            ?? player.transform.position;

        SpawnSawKey(position);
        SpawnHoardingBug(RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, 5f));
    }

    public void SpawnSawKey(Vector3 position)
    {
        GameObject gameObject = Instantiate(SawTapes.sawKey.spawnPrefab, position, Quaternion.identity, StartOfRound.Instance.propsContainer);

        sawKey = gameObject.GetComponent<GrabbableObject>() as SawKey;
        sawKey.fallTime = 0f;
        sawKey.GetComponent<NetworkObject>().Spawn();
    }

    public void SpawnHoardingBug(Vector3 position)
    {
        EnemyType enemyType = SawTapes.allEnemies.FirstOrDefault(e => e.enemyName.Equals("Hoarding bug"));
        if (enemyType == null) return;

        NetworkObject networkObject = EnemySTManager.SpawnEnemyForServer(enemyType, position);
        hoardingBug = networkObject.GetComponentInChildren<HoarderBugAI>();
        hoardingBug.Start();

        Vector3 nestPosition = STUtilities.GetFurthestValidPosition(hoardingBug.transform.position, hoardingBug.agent, hoardingBug.allAINodes)
            ?? hoardingBug.transform.position;
        GrabSawKey(nestPosition);
        PrepareDefusingClientRpc(networkObject, nestPosition);
    }

    public void GrabSawKey(Vector3 nestPosition)
    {
        hoardingBug.targetItem = sawKey;

        hoardingBug.choseNestPosition = true;
        hoardingBug.nestPosition = nestPosition;
        _ = hoardingBug.SetDestinationToPosition(hoardingBug.nestPosition);

        NetworkObject networkObject = sawKey.GetComponent<NetworkObject>();
        hoardingBug.SwitchToBehaviourStateOnLocalClient(1);
        hoardingBug.GrabItem(networkObject);
        hoardingBug.sendingGrabOrDropRPC = true;
        hoardingBug.GrabItemServerRpc(networkObject);
    }

    [ClientRpc]
    public void PrepareDefusingClientRpc(NetworkObjectReference enemyObject, Vector3 nestPosition)
    {
        if (!enemyObject.TryGet(out NetworkObject networkObject)) return;

        hoardingBug = networkObject.GetComponentInChildren<HoarderBugAI>();
        hoardingBug.nestPosition = nestPosition;
        _ = StartCoroutine(SawGameSTManager.ShowAuraForHuntCoroutine([hoardingBug], ConfigManager.explosiveAura.Value));
    }

    public override bool DoGameForServer(int iterator)
        => !(players.All(p => p.isPlayerDead) || sawBomb.hasBeenUsedForExplosiveGame);

    public override bool ExecutePreEndGameActionForServer(bool isGameCancelled)
    {
        _ = base.ExecutePreEndGameActionForServer(isGameCancelled);

        if (!sawBomb.hasBeenUsedForExplosiveGame) sawBomb.SpawnExplosionClientRpc();
        SawGameSTManager.DespawnHazard(sawBox.gameObject);

        if (sawKey != null) ObjectSTManager.DestroyObjectOfTypeForServer(sawKey);
        if (hoardingBug != null) EnemySTManager.DespawnEnemy(hoardingBug.thisNetworkObject);

        return players.All(p => p.isPlayerDead);
    }
}
