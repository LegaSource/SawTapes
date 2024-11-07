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
                .OrderByDescending(n => (player.transform.position - n.transform.position).sqrMagnitude)
                .FirstOrDefault()
                .transform
                .position;
        }

        public static NetworkObject SpawnEnemy(EnemyType enemyType, Vector3 spawnPosition)
        {
            Debug.Log("--------- SpawnEnemy: " + enemyType.enemyName);
            GameObject gameObject = Object.Instantiate(enemyType.enemyPrefab, spawnPosition, Quaternion.identity);
            NetworkObject networkObject = gameObject.GetComponentInChildren<NetworkObject>();
            networkObject.Spawn(true);
            return networkObject;
        }
    }
}
