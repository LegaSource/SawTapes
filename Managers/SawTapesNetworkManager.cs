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

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBathroomServerRpc(int playerId)
    {
        GameObject bathroomObj = Instantiate(SawTapes.bathroomObj, (Vector3.up * ConfigManager.bathroomPosY.Value) + (Vector3.right * ConfigManager.bathroomPosX.Value), Quaternion.identity);
        bathroomObj.GetComponent<NetworkObject>().Spawn();

        SawTapes.bathroom = bathroomObj.GetComponent<Bathroom>();
        SawTapes.bathroom.InitializeBathroomClientRpc(playerId, bathroomObj.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBleedingChainsServerRpc(NetworkObjectReference entityObj, int playerId) => SpawnBleedingChainsClientRpc(entityObj, playerId);

    [ClientRpc]
    public void SpawnBleedingChainsClientRpc(NetworkObjectReference entityObj, int playerId)
    {
        if (!entityObj.TryGet(out NetworkObject networkObject)) return;

        GameObject entity = networkObject.gameObject;
        Vector3 position = entity.GetComponentInChildren<BoxCollider>()?.bounds.center ?? entity.transform.position;
        GameObject bleedingChainsObj = Instantiate(SawTapes.bleedingChainsObj, position, entity.transform.rotation, entity.transform);
        BCScript bCScript = bleedingChainsObj.GetComponent<BCScript>();
        bCScript.entity = entity;
        bCScript.playerWhoHit = playerId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnFinalDetonationServerRpc(NetworkObjectReference entityObj, int playerId)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        Vector3 position = player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward;
        position = RoundManager.Instance.GetNavMeshPosition(position, sampleRadius: 2f);

        NetworkObject billyObj = EnemySTManager.SpawnEnemyForServer(SawTapes.billyFDEnemy, position);
        SpawnFinalDetonationClientRpc(billyObj, entityObj, playerId);
    }

    [ClientRpc]
    public void SpawnFinalDetonationClientRpc(NetworkObjectReference billyObj, NetworkObjectReference entityObj, int playerId)
    {
        if (!billyObj.TryGet(out NetworkObject networkBillyObj) || !entityObj.TryGet(out NetworkObject networkEntityObject)) return;

        BillyFD billy = networkBillyObj.gameObject.GetComponentInChildren<EnemyAI>() as BillyFD;
        if (billy != null && networkEntityObject.gameObject != null)
        {
            billy.player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            billy.targetedEntity = networkEntityObject.gameObject;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnHunterMarkServerRpc(int playerId)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        Vector3 position = player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward;
        position = RoundManager.Instance.GetNavMeshPosition(position, sampleRadius: 2f);

        _ = EnemySTManager.SpawnEnemyForServer(SawTapes.billyHMEnemy, position);
    }
}
