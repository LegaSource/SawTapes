using DunGen;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours
{
    public class SawTape : PhysicsProp
    {
        public bool isGameStarted = false;
        public bool isGameEnded = false;
        public AudioSource sawTheme;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (ConfigManager.isSawTheme.Value
                && buttonDown
                && playerHeldBy != null
                && !isGameEnded
                && !isGameStarted
                && (sawTheme == null || !sawTheme.isPlaying))
            {
                GameObject audioObject = Instantiate(SawTapes.sawTheme, playerHeldBy.transform.position, Quaternion.identity);
                sawTheme = audioObject.GetComponent<AudioSource>();
                sawTheme.Play();
                audioObject.transform.SetParent(playerHeldBy.transform);
            }
        }

        [ClientRpc]
        public void StartGameClientRpc()
        {
            isGameStarted = true;
        }

        public void SpawnBilly(ref PlayerControllerB player)
        {
            /*Vector3 position = player.transform.position + Vector3.up;
            Vector3 backward = -player.transform.forward;
            if (!Physics.Raycast(position, backward, out var hitInfo, 8f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                hitInfo.distance = 8f;
            }
            if (hitInfo.distance > 3f)
            {
                position += backward * hitInfo.distance;
                Vector3 navMeshPosition = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, 2f);
                if (!RoundManager.Instance.GotNavMeshPositionResult)
                {
                    navMeshPosition = RoundManager.Instance.GetNavMeshPosition(navMeshPosition, RoundManager.Instance.navHit, -1f);
                }
                if (RoundManager.Instance.GotNavMeshPositionResult)
                {
                    GameObject gameObject = Instantiate(SawTapes.billyEnemy.enemyPrefab, navMeshPosition, Quaternion.identity);
                    NetworkObject networkObject = gameObject.GetComponentInChildren<NetworkObject>();
                    networkObject.Spawn(true);
                    EnemyAI enemyAI = networkObject.GetComponentInChildren<EnemyAI>();
                    if (enemyAI != null && enemyAI is Billy billy)
                    {
                        billy.followedPlayer = player;
                    }
                }
            }*/

            GameObject[] allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            float maxDistance = 15f;
            float minDistance = 8f;
            Vector3 spawnPosition = Vector3.zero;
            bool foundPosition = false;

            for (int i = 0; i < allAINodes.Length; i++)
            {
                if (!Physics.Linecast(player.gameplayCamera.transform.position, allAINodes[i].transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault)
                    && !player.HasLineOfSightToPosition(allAINodes[i].transform.position, 80f, 100, 8f))
                {
                    continue;
                }

                float distanceToPlayer = Vector3.Distance(player.transform.position, allAINodes[i].transform.position);
                if (distanceToPlayer >= minDistance && distanceToPlayer <= maxDistance)
                {
                    Vector3 navMeshPosition = RoundManager.Instance.GetNavMeshPosition(allAINodes[i].transform.position, RoundManager.Instance.navHit);
                    if (RoundManager.Instance.GotNavMeshPositionResult)
                    {
                        spawnPosition = navMeshPosition;
                        foundPosition = true;
                        break;
                    }
                }
            }

            if (foundPosition)
            {
                GameObject gameObject = Instantiate(SawTapes.billyEnemy.enemyPrefab, spawnPosition, Quaternion.identity);
                NetworkObject networkObject = gameObject.GetComponentInChildren<NetworkObject>();
                networkObject.Spawn(true);
                SpawnBillyClientRpc(networkObject, (int)player.playerClientId);
            }
            else
            {
                SawTapes.mls.LogWarning("Could not find a valid spawn position for Billy.");
            }
        }

        [ClientRpc]
        public void SpawnBillyClientRpc(NetworkObjectReference enemyObject, int playerId)
        {
            if (enemyObject.TryGet(out NetworkObject networkObject))
            {
                EnemyAI enemyAI = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
                if (enemyAI != null && enemyAI is Billy billy)
                {
                    billy.targetPlayer = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                    if (IsServer)
                    {
                        billy.StartFollowingPlayer();
                    }
                }
            }
        }
    }
}
