using GameNetcodeStuff;
using SawTapes.Files;
using SawTapes.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Tapes
{
    public class SurvivalTape : SawTape
    {
        public ParticleSystem spawnParticle;
        public List<NetworkObject> spawnedEnemies = new List<NetworkObject>();

        public override void Start()
        {
            base.Start();

            InstantiateAndAttachAudio(SawTapes.sawRecordingSurvival);
            subtitlesGame = SubtitleFile.survivalGameSubtitles;
            
            minPlayersAmount = ConfigManager.survivalMinPlayers.Value;
            maxPlayersAmount = ConfigManager.survivalMaxPlayers.Value;
            
            gameDuration = ConfigManager.survivalDuration.Value;
            billyValue = ConfigManager.survivalBillyValue.Value;
        }

        public override bool DoGameForServer(int iterator)
        {
            base.DoGameForServer(iterator);

            if (players.All(p => p.isPlayerDead)) return false;

            SpawnEnemies(iterator);
            SetEnemiesTargets();

            return true;
        }

        public void SpawnEnemies(int iterator)
        {
            if (iterator % 5 != 0) return;

            foreach (PlayerControllerB player in players)
            {
                List<EnemyType> eligibleEnemies = SawTapes.allEnemies
                    .Where(e => ConfigManager.survivalEnemies.Value.Contains(e.enemyName))
                    .ToList();

                EnemyType enemyType = eligibleEnemies.Count > 0
                    ? eligibleEnemies[Random.Range(0, eligibleEnemies.Count)]
                    : null;

                StartCoroutine(SpawnEnemyCoroutine(enemyType, player.transform.position));
            }
        }

        public IEnumerator SpawnEnemyCoroutine(EnemyType enemyType, Vector3 playerPosition)
        {
            Vector3 spawnPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadius(playerPosition, 5);
            PlaySpawnParticleClientRpc(spawnPosition);

            yield return new WaitUntil(() => !spawnParticle.isPlaying);

            Destroy(spawnParticle.gameObject);
            NetworkObject networkObject = EnemySTManager.SpawnEnemy(enemyType, spawnPosition);
            spawnedEnemies.Add(networkObject);
        }

        [ClientRpc]
        public void PlaySpawnParticleClientRpc(Vector3 position)
        {
            GameObject spawnObject = Instantiate(SawTapes.spawnParticle, position, Quaternion.identity);
            spawnParticle = spawnObject.GetComponent<ParticleSystem>();
        }

        public void SetEnemiesTargets()
        {
            foreach (NetworkObject spawnedEnemy in spawnedEnemies)
            {
                if (spawnedEnemy == null) continue;

                EnemyAI enemy = spawnedEnemy.GetComponentInChildren<EnemyAI>();
                if (enemy?.thisNetworkObject == null || !enemy.thisNetworkObject.IsSpawned) continue;
                if (enemy.isEnemyDead) continue;

                PlayerControllerB closestPlayer = players.OrderBy(p => Vector3.Distance(p.transform.position, enemy.transform.position)).FirstOrDefault();
                if (closestPlayer == null) continue;

                enemy.SetMovingTowardsTargetPlayer(closestPlayer);
            }
        }

        public override bool ExecutePreEndGameActionForServer(bool isGameCancelled)
        {
            base.ExecutePreEndGameActionForServer(isGameCancelled);

            foreach (NetworkObject spawnedEnemy in spawnedEnemies)
            {
                if (spawnedEnemy == null) continue;
                if (!spawnedEnemy.IsSpawned) continue;

                EnemySTManager.DespawnEnemy(spawnedEnemy);
            }
            return players.All(p => p.isPlayerDead);
        }

        public override void EndGameForAllClients(bool isGameEnded)
        {
            base.EndGameForAllClients(isGameEnded);
            spawnedEnemies.Clear();
        }
    }
}
