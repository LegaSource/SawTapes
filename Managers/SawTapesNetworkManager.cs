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

    public void Awake()
        => Instance = this;

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBathroomServerRpc(int playerId)
    {
        GameObject bathroomObj = Instantiate(SawTapes.bathroomObj, (Vector3.up * ConfigManager.bathroomPosY.Value) + (Vector3.right * ConfigManager.bathroomPosX.Value), Quaternion.identity);
        bathroomObj.GetComponent<NetworkObject>().Spawn();

        SawTapes.bathroom = bathroomObj.GetComponent<Bathroom>();
        SawTapes.bathroom.InitializeBathroomClientRpc(playerId, bathroomObj.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBleedingChainsServerRpc(NetworkObjectReference enemyObject, int playerId)
        => SpawnBleedingChainsClientRpc(enemyObject, playerId);

    [ClientRpc]
    public void SpawnBleedingChainsClientRpc(NetworkObjectReference enemyObject, int playerId)
    {
        if (!enemyObject.TryGet(out NetworkObject networkObject)) return;

        EnemyAI enemy = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
        Vector3 position = enemy.GetComponentInChildren<BoxCollider>()?.bounds.center ?? enemy.transform.position;
        GameObject bleedingChainsObj = Instantiate(SawTapes.bleedingChainsObj, position, enemy.transform.rotation, enemy.transform);
        BCScript bCScript = bleedingChainsObj.GetComponent<BCScript>();
        bCScript.enemy = enemy;
        bCScript.playerWhoHit = playerId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnFinalDetonationServerRpc(NetworkObjectReference enemyObject, int playerId)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        Vector3 position = player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward;
        position = RoundManager.Instance.GetNavMeshPosition(position, sampleRadius: 2f);

        NetworkObject billyObject = EnemySTManager.SpawnEnemyForServer(SawTapes.billyFDEnemy, position);
        SpawnFinalDetonationClientRpc(billyObject, enemyObject);
    }

    [ClientRpc]
    public void SpawnFinalDetonationClientRpc(NetworkObjectReference billyObject, NetworkObjectReference enemyObject)
    {
        if (!billyObject.TryGet(out NetworkObject networkBillyObj) || !enemyObject.TryGet(out NetworkObject networkEnemyObject)) return;

        BillyFD billy = networkBillyObj.gameObject.GetComponentInChildren<EnemyAI>() as BillyFD;
        EnemyAI enemy = networkEnemyObject.gameObject.GetComponentInChildren<EnemyAI>();
        if (billy != null && enemy != null) billy.targetedEnemy = enemy;
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
