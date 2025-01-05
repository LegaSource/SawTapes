using SawTapes.Files;
using SawTapes.Managers;
using SawTapes.Values;
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
        public bool wasCampingLastSecond = false;
        public SurvivalHorde activeHorde;
        public List<NetworkObject> spawnedEnemies = new List<NetworkObject>();

        public override void Start()
        {
            base.Start();

            InstantiateAndAttachAudio(SawTapes.sawRecordingSurvival);
            subtitlesGame = SubtitleFile.survivalGameSubtitles;
        }

        public override void ExecutePreGameActionForServer(PlayerSTBehaviour playerBehaviour)
        {
            SurvivalRoom room = SawTapes.rooms.FirstOrDefault(r => playerBehaviour.tileGame.name.Equals(r.RoomName));

            List<SurvivalHorde> validHordes = room.Hordes.Where(h => h.MinHour <= TimeOfDay.Instance.hour && h.MaxHour >= TimeOfDay.Instance.hour).ToList();
            if (validHordes.Count > 0)
                activeHorde = validHordes[new System.Random().Next(validHordes.Count)];
            else
                activeHorde = room.Hordes[new System.Random().Next(room.Hordes.Count)];

            gameDuration = activeHorde.GameDuration;
            billyValue = activeHorde.BillyValue;
        }

        public override bool DoGameForServer(int iterator)
        {
            if (mainPlayer.isPlayerDead) return false;

            if (activeHorde.EnemiesSpawn.TryGetValue(iterator, out EnemyType enemyType) && enemyType != null)
                StartCoroutine(SpawnEnemyCoroutine(enemyType, spawnedEnemies));

            bool isFirst = true;
            foreach (NetworkObject spawnedEnemy in spawnedEnemies.Where(s => s.IsSpawned))
            {
                EnemyAI enemy = spawnedEnemy.GetComponentInChildren<EnemyAI>();
                if (enemy != null
                    && enemy.thisNetworkObject != null
                    && enemy.thisNetworkObject.IsSpawned
                    && !enemy.isEnemyDead)
                {
                    SetEnemyFocusClientRpc(spawnedEnemy);
                    // Vérification si le joueur campe - on vérifie si le path est accessible avec un seul ennemi
                    if (ConfigManager.penalizePlayerWhoCamp.Value && isFirst)
                    {
                        PenalizePlayerWhoCamp(enemy, spawnedEnemies);
                        isFirst = false;
                    }
                }
            }
            return true;
        }

        public IEnumerator SpawnEnemyCoroutine(EnemyType enemyType, List<NetworkObject> spawnedEnemies)
        {
            Vector3 spawnPosition = TileSTManager.GetRandomNavMeshPositionInTile(mainPlayer.GetComponent<PlayerSTBehaviour>());
            PlaySpawnParticleClientRpc(spawnPosition);

            yield return new WaitUntil(() => !spawnParticle.isPlaying);

            Destroy(spawnParticle.gameObject);
            NetworkObject networkObject = EnemySTManager.SpawnEnemy(enemyType, spawnPosition);
            spawnedEnemies.Add(networkObject);
        }

        [ClientRpc]
        public void SetEnemyFocusClientRpc(NetworkObjectReference enemyObject)
        {
            if (enemyObject.TryGet(out NetworkObject networkObject))
                networkObject.GetComponentInChildren<EnemyAI>().SetMovingTowardsTargetPlayer(mainPlayer);
        }

        public void PenalizePlayerWhoCamp(EnemyAI enemy, List<NetworkObject> spawnedEnemies)
        {
            if (!enemy.agent.CalculatePath(mainPlayer.transform.position, enemy.path1))
            {
                if (wasCampingLastSecond)
                {
                    PlayerSTBehaviour playerBehaviour = mainPlayer.GetComponent<PlayerSTBehaviour>();
                    playerBehaviour.campTime++;
                    if (playerBehaviour.campTime == ConfigManager.campDuration.Value)
                    {
                        EnemyType enemyType = SawTapes.allEnemies.FirstOrDefault(e => !e.ToString().Contains("Outside") && e.enemyName.Equals("Nutcracker"));
                        StartCoroutine(SpawnEnemyCoroutine(enemyType, spawnedEnemies));
                    }
                }
                wasCampingLastSecond = true;
            }
            else
            {
                wasCampingLastSecond = false;
            }
        }

        public override bool ExecutePreEndGameActionForServer(bool isGameCancelled)
        {
            foreach (NetworkObject spawnedEnemy in spawnedEnemies.Where(s => s.IsSpawned))
                EnemySTManager.DespawnEnemy(spawnedEnemy);
            return mainPlayer.isPlayerDead;
        }

        [ClientRpc]
        public void PlaySpawnParticleClientRpc(Vector3 position)
        {
            GameObject spawnObject = Instantiate(SawTapes.spawnParticle, position, Quaternion.identity);
            spawnParticle = spawnObject.GetComponent<ParticleSystem>();
        }
    }
}
