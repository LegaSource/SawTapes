using GameNetcodeStuff;
using SawTapes.Files;
using SawTapes.Managers;
using SawTapes.Values;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours
{
    public class SurvivalTape : SawTape
    {
        public ParticleSystem spawnParticle;
        public ParticleSystem despawnParticle;
        public bool wasCampingLastSecond = false;
        public Horde activeHorde;
        List<NetworkObject> spawnedEnemies = new List<NetworkObject>();

        public override void Start()
        {
            base.Start();
            InstantiateAndAttachAudio(SawTapes.sawRecordingSurvival);
            subtitlesGame = SubtitleFile.survivalGameSubtitles;
        }

        public override void ExecutePreGameAction(PlayerSTBehaviour playerBehaviour)
        {
            Room room = SawTapes.rooms.FirstOrDefault(r => playerBehaviour.tileGame.name.Contains(r.RoomName));

            List<Horde> validHordes = room.Hordes.Where(h => h.MinHour <= TimeOfDay.Instance.hour && h.MaxHour >= TimeOfDay.Instance.hour).ToList();
            if (validHordes.Count > 0)
            {
                activeHorde = validHordes[new System.Random().Next(validHordes.Count)];
            }
            else
            {
                activeHorde = room.Hordes[new System.Random().Next(room.Hordes.Count)];
            }
            gameDuration = activeHorde.GameDuration;
            billyValue = activeHorde.BillyValue;
        }

        public override bool DoGame(PlayerSTBehaviour playerBehaviour, int iterator)
        {
            if (playerBehaviour.playerProperties.isPlayerDead)
            {
                return false;
            }
            if (activeHorde.EnemiesSpawn.TryGetValue(iterator, out EnemyType enemyType) && enemyType != null)
            {
                StartCoroutine(SpawnEnemyCoroutine(enemyType, playerBehaviour, spawnedEnemies));
            }
            bool isFirst = true;
            foreach (NetworkObject spawnedEnemy in spawnedEnemies.Where(s => s.IsSpawned))
            {
                EnemyAI enemyAI = spawnedEnemy.GetComponentInChildren<EnemyAI>();
                if (enemyAI != null
                    && enemyAI.thisNetworkObject != null
                    && enemyAI.thisNetworkObject.IsSpawned
                    && !enemyAI.isEnemyDead)
                {
                    SetEnemyFocusClientRpc((int)playerBehaviour.playerProperties.playerClientId, spawnedEnemy);
                    // Vérification si le joueur campe - on vérifie si le path est accessible avec un seul ennemi
                    if (ConfigManager.penalizePlayerWhoCamp.Value && isFirst)
                    {
                        PenalizePlayerWhoCamp(playerBehaviour, enemyAI, spawnedEnemies);
                        isFirst = false;
                    }
                }
            }
            return true;
        }

        public IEnumerator SpawnEnemyCoroutine(EnemyType enemyType, PlayerSTBehaviour playerBehaviour, List<NetworkObject> spawnedEnemies)
        {
            Vector3 spawnPosition = TileSTManager.GetRandomNavMeshPositionInTile(ref playerBehaviour);
            PlaySpawnParticleClientRpc(spawnPosition, true);

            yield return new WaitUntil(() => !spawnParticle.isPlaying);

            Destroy(spawnParticle.gameObject);
            NetworkObject networkObject = EnemySTManager.SpawnEnemy(enemyType, spawnPosition);
            spawnedEnemies.Add(networkObject);
        }

        [ClientRpc]
        public void SetEnemyFocusClientRpc(int playerId, NetworkObjectReference enemyObject)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
            if (enemyObject.TryGet(out NetworkObject networkObject))
            {
                networkObject.GetComponentInChildren<EnemyAI>().SetMovingTowardsTargetPlayer(player);
            }
        }

        public void PenalizePlayerWhoCamp(PlayerSTBehaviour playerBehaviour, EnemyAI enemy, List<NetworkObject> spawnedEnemies)
        {
            if (!enemy.agent.CalculatePath(playerBehaviour.playerProperties.transform.position, enemy.path1))
            {
                if (wasCampingLastSecond)
                {
                    playerBehaviour.campTime++;
                    if (playerBehaviour.campTime == ConfigManager.campDuration.Value)
                    {
                        EnemyType enemyType = SawTapes.allEnemies.FirstOrDefault(e => !e.ToString().Contains("Outside") && e.enemyName.Equals("Nutcracker"));
                        StartCoroutine(SpawnEnemyCoroutine(enemyType, playerBehaviour, spawnedEnemies));
                    }
                }
                wasCampingLastSecond = true;
            }
            else
            {
                wasCampingLastSecond = false;
            }
        }

        public override bool ExecutePreEndGameAction(PlayerSTBehaviour playerBehaviour)
        {
            foreach (NetworkObject spawnedEnemy in spawnedEnemies.Where(s => s.IsSpawned))
            {
                EnemyAI enemyAI = spawnedEnemy.GetComponentInChildren<EnemyAI>();
                if (enemyAI != null && !enemyAI.isEnemyDead)
                {
                    PlaySpawnParticleClientRpc(spawnedEnemy.transform.position, false);
                    if (enemyAI is NutcrackerEnemyAI nutcrackerEnemyAI && nutcrackerEnemyAI.gun != null)
                    {
                        SawTapesNetworkManager.Instance.DestroyObjectClientRpc(nutcrackerEnemyAI.gun.GetComponent<NetworkObject>());
                    }
                    spawnedEnemy.Despawn();
                }
            }
            return playerBehaviour.playerProperties.isPlayerDead;
        }

        [ClientRpc]
        public void PlaySpawnParticleClientRpc(Vector3 position, bool isSpawn)
        {
            if (isSpawn)
            {
                GameObject spawnObject = Instantiate(SawTapes.spawnParticle, position, Quaternion.identity);
                spawnParticle = spawnObject.GetComponent<ParticleSystem>();
            }
            else
            {
                GameObject spawnObject = Instantiate(SawTapes.despawnParticle, position, Quaternion.identity);
                despawnParticle = spawnObject.GetComponent<ParticleSystem>();
                Destroy(spawnObject, despawnParticle.main.duration + despawnParticle.main.startLifetime.constantMax);
            }
        }
    }
}
