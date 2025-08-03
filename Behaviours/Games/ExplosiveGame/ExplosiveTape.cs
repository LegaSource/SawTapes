using GameNetcodeStuff;
using LegaFusionCore.Managers;
using LegaFusionCore.Managers.NetworkManagers;
using SawTapes.Behaviours.Games.HuntingGame;
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
    public SawKeyHunting sawKey;
    public HoarderBugAI hoardingBug;

    public override void Start()
    {
        base.Start();

        //InstantiateAndAttachAudio(SawTapes.sawRecordingExplosive);
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

    public void PrepareDefusing(PlayerControllerB player)
    {
        gameDuration += ConfigManager.explosiveExtraDuration.Value;

        Vector3 position = STUtilities.GetClosestPositionScrapSpawn(player.transform.position, 17.5f, SawTapes.sawKeyExplosive)
            ?? player.transform.position;

        SpawnSawKey(position);
        SpawnHoardingBug(RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, 5f));
    }

    public void SpawnSawKey(Vector3 position)
    {
        GameObject gameObject = Instantiate(SawTapes.sawKeyExplosive.spawnPrefab, position, Quaternion.identity, StartOfRound.Instance.propsContainer);

        sawKey = gameObject.GetComponent<GrabbableObject>() as SawKeyHunting;
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

    public override bool ExecutePreEndGameActionForServer()
    {
        _ = base.ExecutePreEndGameActionForServer();

        if (!sawBomb.hasBeenUsedForExplosiveGame) sawBomb.SpawnExplosionClientRpc();
        SawGameSTManager.DespawnHazard(sawBox.gameObject);

        LFCObjectsManager.DestroyObjectOfTypeForServer(sawKey);
        EnemySTManager.DespawnEnemy(hoardingBug.thisNetworkObject);

        return players.All(p => p.isPlayerDead);
    }
}
