using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    public class EnemySTManager
    {
        public static NetworkObject SpawnEnemy(EnemyType enemyType, Vector3 spawnPosition)
        {
            GameObject gameObject = Object.Instantiate(enemyType.enemyPrefab, spawnPosition, Quaternion.identity);
            NetworkObject networkObject = gameObject.GetComponentInChildren<NetworkObject>();
            networkObject.Spawn(true);
            return networkObject;
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
}
