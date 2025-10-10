using GameNetcodeStuff;
using SawTapes.Behaviours.Bathroom;
using SawTapes.Behaviours.Enemies;
using SawTapes.Behaviours.Items.Addons.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers;

public class SawTapesNetworkManager : NetworkBehaviour
{
    public static SawTapesNetworkManager Instance;

    public void Awake() => Instance = this;

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SpawnBathroomServerRpc(int playerId)
    {
        GameObject bathroomObj = Instantiate(SawTapes.bathroomObj, (Vector3.up * ConfigManager.bathroomPosY.Value) + (Vector3.right * ConfigManager.bathroomPosX.Value), Quaternion.identity);
        bathroomObj.GetComponent<NetworkObject>().Spawn();

        SawTapes.bathroom = bathroomObj.GetComponent<Bathroom>();
        SawTapes.bathroom.InitializeBathroomEveryoneRpc(playerId, bathroomObj.GetComponent<NetworkObject>());
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SpawnBleedingChainsEveryoneRpc(NetworkObjectReference entityObj, int playerId)
    {
        if (!entityObj.TryGet(out NetworkObject networkObject)) return;

        GameObject entity = networkObject.gameObject;
        Vector3 position = entity.GetComponentInChildren<BoxCollider>()?.bounds.center ?? entity.transform.position;
        GameObject bleedingChainsObj = Instantiate(SawTapes.bleedingChainsObj, position, entity.transform.rotation, entity.transform);
        BCScript bCScript = bleedingChainsObj.GetComponent<BCScript>();
        bCScript.entity = entity;
        bCScript.playerWhoHit = playerId;
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SpawnFinalDetonationServerRpc(NetworkObjectReference entityObj, int playerId)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        Vector3 position = player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward;
        position = RoundManager.Instance.GetNavMeshPosition(position, sampleRadius: 2f);

        NetworkObject billyObj = EnemySTManager.SpawnEnemyForServer(SawTapes.billyFDEnemy, position);
        SpawnFinalDetonationEveryoneRpc(billyObj, entityObj, playerId);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SpawnFinalDetonationEveryoneRpc(NetworkObjectReference billyObj, NetworkObjectReference entityObj, int playerId)
    {
        if (!billyObj.TryGet(out NetworkObject networkBillyObj) || !entityObj.TryGet(out NetworkObject networkEntityObject)) return;

        BillyFD billy = networkBillyObj.gameObject.GetComponentInChildren<EnemyAI>() as BillyFD;
        if (billy != null && networkEntityObject.gameObject != null)
        {
            billy.player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            billy.targetedEntity = networkEntityObject.gameObject;
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SpawnHunterMarkServerRpc(int playerId)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        Vector3 position = player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward;
        position = RoundManager.Instance.GetNavMeshPosition(position, sampleRadius: 2f);

        _ = EnemySTManager.SpawnEnemyForServer(SawTapes.billyHMEnemy, position);
    }
}
