using DunGen;
using GameNetcodeStuff;
using SawTapes.Files;
using SawTapes.Files.Values;
using SawTapes.Managers;
using SawTapes.Patches;
using SawTapes.Values;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace SawTapes.Behaviours
{
    internal class SurvivalTape : SawTape
    {
        public ParticleSystem spawnParticle;
        public ParticleSystem despawnParticle;
        public bool wasCampingLastSecond = false;

        public override void Start()
        {
            base.Start();
            GameObject audioObject = Instantiate(SawTapes.sawRecordingSurvival, transform.position, Quaternion.identity);
            sawRecording = audioObject.GetComponent<AudioSource>();
            audioObject.transform.SetParent(transform);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null && !sawRecording.isPlaying)
            {
                PlaySawTapeServerRpc();
                PlayerSTBehaviour playerBehaviour = playerHeldBy.GetComponent<PlayerSTBehaviour>();
                if (!isGameStarted && !isGameEnded)
                {
                    if (playerBehaviour.isInGame && playerBehaviour.tileGame != null)
                    {
                        StartCoroutine(SawGameBegin(playerBehaviour));
                    }
                    else
                    {
                        HUDManager.Instance.DisplayTip("Information", "You are not the tested player, the game can't start");
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlaySawTapeServerRpc()
        {
            PlaySawTapeClientRpc();
        }

        [ClientRpc]
        public void PlaySawTapeClientRpc()
        {
            sawRecording.Play();
            if (ConfigManager.isSubtitles.Value) StartCoroutine(ShowSubtitles());
        }

        public IEnumerator SawGameBegin(PlayerSTBehaviour playerBehaviour)
        {
            Room room = SawTapes.rooms.FirstOrDefault(r => playerBehaviour.tileGame.name.Contains(r.RoomName));

            Horde horde;
            List<Horde> validHordes = room.Hordes.Where(h => h.MinHour <= TimeOfDay.Instance.hour && h.MaxHour >= TimeOfDay.Instance.hour).ToList();
            if (validHordes.Count > 0)
            {
                horde = validHordes[new System.Random().Next(validHordes.Count)];
            }
            else
            {
                horde = room.Hordes[new System.Random().Next(room.Hordes.Count)];
            }

            yield return new WaitUntil(() => sawRecording.isPlaying);
            yield return new WaitWhile(() => sawRecording.isPlaying);

            if (sawTheme != null)
            {
                sawTheme.volume *= 1.5f;
            }
            HUDManager.Instance.StartCoroutine(HUDManagerPatch.StartChronoCoroutine(horde.GameDuration));
            StartSawGameServerRpc((int)playerBehaviour.playerProperties.playerClientId, horde.HordeName);
        }

        public IEnumerator ShowSubtitles()
        {
            while (sawRecording.isPlaying)
            {
                string subtitleText = SubtitleFile.survivalGameSubtitles.Where(s => s.Timestamp <= sawRecording.time).OrderByDescending(s => s.Timestamp).FirstOrDefault()?.Text;
                if (!string.IsNullOrEmpty(subtitleText))
                {
                    if (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, transform.position) <= 25)
                    {
                        HUDManagerPatch.subtitleText.text = subtitleText;
                    }
                    else
                    {
                        HUDManagerPatch.subtitleText.text = "";
                    }
                }
                yield return null;
            }
            HUDManagerPatch.subtitleText.text = "";
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartSawGameServerRpc(int playerId, string hordeName)
        {
            Horde horde = SawTapes.hordes.FirstOrDefault(h => h.HordeName.Equals(hordeName));
            StartGameClientRpc();
            StartCoroutine(StartSawGame(StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>(), horde));
        }

        public IEnumerator StartSawGame(PlayerSTBehaviour playerBehaviour, Horde horde)
        {
            List<NetworkObject> spawnedEnemies = new List<NetworkObject>();
            int timePassed = 0;
            while (timePassed < horde.GameDuration)
            {
                if (playerBehaviour.playerProperties.isPlayerDead)
                {
                    break;
                }
                if (horde.EnemiesSpawn.TryGetValue(timePassed, out EnemyType enemyType) && enemyType != null)
                {
                    StartCoroutine(SpawnEnemy(enemyType, playerBehaviour, spawnedEnemies));
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
                yield return new WaitForSeconds(1f);
                timePassed++;
            }
            EndGame(spawnedEnemies, playerBehaviour, horde.BillyValue);
        }

        public IEnumerator SpawnEnemy(EnemyType enemyType, PlayerSTBehaviour playerBehaviour, List<NetworkObject> spawnedEnemies)
        {
            Vector3 spawnPosition = TileSTManager.GetRandomNavMeshPositionInTile(ref playerBehaviour);
            PlaySpawnParticleClientRpc(spawnPosition, true);

            yield return new WaitUntil(() => !spawnParticle.isPlaying);

            Destroy(spawnParticle.gameObject);
            GameObject gameObject = Instantiate(enemyType.enemyPrefab, spawnPosition, Quaternion.identity);
            NetworkObject networkObject = gameObject.GetComponentInChildren<NetworkObject>();
            networkObject.Spawn(true);
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
                        StartCoroutine(SpawnEnemy(enemyType, playerBehaviour, spawnedEnemies));
                    }
                }
                wasCampingLastSecond = true;
            }
            else
            {
                wasCampingLastSecond = false;
            }
        }

        public void EndGame(List<NetworkObject> spawnedEnemies, PlayerSTBehaviour playerBehaviour, int billyValue)
        {
            foreach (NetworkObject spawnedEnemy in spawnedEnemies.Where(s => s.IsSpawned))
            {
                EnemyAI enemyAI = spawnedEnemy.GetComponentInChildren<EnemyAI>();
                if (enemyAI != null && !enemyAI.isEnemyDead)
                {
                    PlaySpawnParticleClientRpc(spawnedEnemy.transform.position, false);
                    if (enemyAI is NutcrackerEnemyAI nutcrackerEnemyAI)
                    {
                        DestroyObjectClientRpc(nutcrackerEnemyAI.gun.GetComponent<NetworkObject>());
                    }
                    spawnedEnemy.Despawn();
                }
            }
            if (playerBehaviour.playerProperties.isPlayerDead)
            {
                SawTapesNetworkManager.Instance.EnableParticleServerRpc(GetComponent<NetworkObject>(), true);
            }
            else
            {
                StartCoroutine(SpawnBillyCoroutine(playerBehaviour.playerProperties, billyValue));
            }
            SendEndGameClientRpc((int)playerBehaviour.playerProperties.playerClientId);
        }

        [ClientRpc]
        public void DestroyObjectClientRpc(NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                grabbableObject?.DestroyObjectInHand(null);
            }
        }

        [ClientRpc]
        public void SendEndGameClientRpc(int playerId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
            PlayerSTBehaviour playerBehaviour = player.GetComponent<PlayerSTBehaviour>();

            if (!player.isPlayerDead)
            {
                SawTapes.eligibleTiles.Remove(playerBehaviour.tileGame);
                isGameEnded = true;
            }

            isGameStarted = false;
            PlayerSTManager.ResetPlayerGame(ref playerBehaviour);
            if (player == GameNetworkManager.Instance.localPlayerController && sawTheme != null)
            {
                sawTheme.Stop();
                Destroy(sawTheme.gameObject);
            }
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
