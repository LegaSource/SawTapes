using GameNetcodeStuff;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    public class EnemySTManager
    {
        public static Vector3 GetFurthestPositionFromPlayer(PlayerControllerB player)
        {
            return RoundManager.Instance.insideAINodes
                .OrderByDescending(n => Vector3.Distance(player.transform.position, n.transform.position))
                .FirstOrDefault()
                .transform
                .position;
        }

        public static NetworkObject SpawnEnemy(EnemyType enemyType, Vector3 spawnPosition)
        {
            GameObject gameObject = Object.Instantiate(enemyType.enemyPrefab, spawnPosition, Quaternion.identity);
            NetworkObject networkObject = gameObject.GetComponentInChildren<NetworkObject>();
            networkObject.Spawn(true);
            return networkObject;
        }

        public static void DespawnEnemy(NetworkObject spawnedEnemy)
        {
            EnemyAI enemyAI = spawnedEnemy.GetComponentInChildren<EnemyAI>();
            if (enemyAI != null && !enemyAI.isEnemyDead)
            {
                SawTapesNetworkManager.Instance.PlayDespawnParticleClientRpc(spawnedEnemy.transform.position);
                if (enemyAI is NutcrackerEnemyAI nutcrackerEnemyAI && nutcrackerEnemyAI.gun != null)
                {
                    SawTapesNetworkManager.Instance.DestroyObjectClientRpc(nutcrackerEnemyAI.gun.GetComponent<NetworkObject>());
                }
                spawnedEnemy.Despawn();
            }
        }
    }
}
