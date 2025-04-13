using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers;

public class EnemySTManager
{
    public static NetworkObject SpawnEnemyForServer(EnemyType enemyType, Vector3 spawnPosition)
    {
        GameObject gameObject = Object.Instantiate(enemyType.enemyPrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = gameObject.GetComponentInChildren<NetworkObject>();
        networkObject.Spawn(true);
        return networkObject;
    }

    public static void DespawnEnemiesForServer(List<NetworkObject> spawnedEnemies)
    {
        foreach (NetworkObject spawnedEnemy in spawnedEnemies)
        {
            if (spawnedEnemy == null) continue;

            EnemyAI enemy = spawnedEnemy.GetComponentInChildren<EnemyAI>();
            if (enemy?.thisNetworkObject == null || !enemy.thisNetworkObject.IsSpawned) continue;
            if (enemy.isEnemyDead) continue;

            DespawnEnemy(spawnedEnemy);
        }
    }

    public static void DespawnEnemy(NetworkObject spawnedEnemy)
    {
        EnemyAI enemy = spawnedEnemy.GetComponentInChildren<EnemyAI>();
        if (enemy == null || enemy.isEnemyDead) return;

        SawTapesNetworkManager.Instance.PlayDespawnParticleClientRpc(spawnedEnemy.transform.position);
        if (enemy is NutcrackerEnemyAI nutcrackerEnemyAI && nutcrackerEnemyAI.gun != null)
            SawTapesNetworkManager.Instance.DestroyObjectClientRpc(nutcrackerEnemyAI.gun.GetComponent<NetworkObject>());
        spawnedEnemy.Despawn();
    }
}
