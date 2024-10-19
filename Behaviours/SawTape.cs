using GameNetcodeStuff;
using SawTapes.Managers;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours
{
    public class SawTape : PhysicsProp
    {
        public bool isGameStarted = false;
        public bool isGameEnded = false;
        public AudioSource sawRecording;
        public AudioSource sawTheme;
        public GameObject particleEffect;

        public override void GrabItem()
        {
            base.GrabItem();
            if (particleEffect != null)
            {
                SawTapesNetworkManager.Instance.EnableParticleServerRpc(GetComponent<NetworkObject>(), false);
            }
        }

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

        public IEnumerator SpawnBillyCoroutine(PlayerControllerB player, int billyValue)
        {
            GameObject[] allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            float maxDistance = 15f;
            float minDistance = 8f;
            Vector3 spawnPosition = Vector3.zero;
            bool foundPosition = false;
            float maxDuration = 10f;
            float startTime = Time.time;

            while (Time.time - startTime < maxDuration)
            {
                for (int i = 0; i < allAINodes.Length; i++)
                {
                    yield return null;

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
                    break;
                }
            }

            if (!foundPosition)
            {
                SawTapes.mls.LogWarning("Could not find a valid spawn position for Billy.");
                spawnPosition = player.transform.position;
            }

            GameObject gameObject = Instantiate(SawTapes.billyEnemy.enemyPrefab, spawnPosition, Quaternion.identity);
            NetworkObject networkObject = gameObject.GetComponentInChildren<NetworkObject>();
            networkObject.Spawn(true);
            SpawnBillyClientRpc(networkObject, (int)player.playerClientId, billyValue);
        }

        [ClientRpc]
        public void SpawnBillyClientRpc(NetworkObjectReference enemyObject, int playerId, int billyValue)
        {
            if (enemyObject.TryGet(out NetworkObject networkObject))
            {
                EnemyAI enemyAI = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
                if (enemyAI != null && enemyAI is Billy billy)
                {
                    billy.targetPlayer = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                    billy.billyValue = billyValue;
                    if (IsServer)
                    {
                        billy.StartFollowingPlayer();
                    }
                }
            }
        }
    }
}
