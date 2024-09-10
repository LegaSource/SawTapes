using DunGen;
using GameNetcodeStuff;
using SawTapes.Files;
using SawTapes.Patches;
using SawTapes.Values;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace SawTapes.Behaviours
{
    internal class SurvivalTape : SawTape
    {
        public AudioSource sawRecording;
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
                if (!isGameEnded && playerBehaviour.isInGame && playerBehaviour.tileGame != null)
                {
                    StartCoroutine(SawGameBegin(playerBehaviour));
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
            Horde horde = room.Hordes[new System.Random().Next(room.Hordes.Count)];

            yield return new WaitUntil(() => sawRecording.isPlaying);
            yield return new WaitWhile(() => sawRecording.isPlaying);

            sawTheme.volume *= 1.5f;
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
                    yield break;
                }
                StartCoroutine(SpawnEnemy(timePassed, playerBehaviour.tileGame, horde, spawnedEnemies));
                bool isFirst = true;
                foreach (NetworkObject spawnedEnemy in spawnedEnemies.Where(s => s.IsSpawned))
                {
                    SetEnemyFocusClientRpc((int)playerBehaviour.playerProperties.playerClientId, spawnedEnemy);
                    // Vérification si le joueur campe - on vérifie si le path est accessible avec un seul ennemi
                    if (ConfigManager.killPlayerWhoCamp.Value && isFirst)
                    {
                        KillPlayerWhoCampClientRpc((int)playerBehaviour.playerProperties.playerClientId, spawnedEnemy);
                        isFirst = false;
                    }
                }
                yield return new WaitForSeconds(1f);
                timePassed++;
            }
            EndGame(spawnedEnemies, playerBehaviour);
        }

        public IEnumerator SpawnEnemy(int enemySpawnKey, Tile tile, Horde horde, List<NetworkObject> spawnedEnemies)
        {
            KeyValuePair<int, EnemyAI> enemySpawn = horde.EnemiesSpawn.FirstOrDefault(e => e.Key == enemySpawnKey);
            if (enemySpawn.Value != null)
            {
                Vector3 spawnPosition = GetRandomNavMeshPositionInTile(ref tile);
                PlaySpawnParticleClientRpc(spawnPosition, true);

                yield return new WaitUntil(() => !spawnParticle.isPlaying);

                Destroy(spawnParticle.gameObject);
                GameObject gameObject = Instantiate(enemySpawn.Value.enemyType.enemyPrefab, spawnPosition, Quaternion.identity);
                NetworkObject networkObject = gameObject.GetComponentInChildren<NetworkObject>();
                networkObject.Spawn(true);
                spawnedEnemies.Add(networkObject);
            }
        }

        public Vector3 GetRandomNavMeshPositionInTile(ref Tile tile)
        {
            float padding = 2.0f;
            float randomX = Random.Range(tile.Bounds.min.x + padding, tile.Bounds.max.x - padding);
            float randomY = Random.Range(tile.Bounds.min.y, tile.Bounds.max.y);
            float randomZ = Random.Range(tile.Bounds.min.z + padding, tile.Bounds.max.z - padding);

            Vector3 randomPosition = new Vector3(randomX, randomY, randomZ);

            // Vérifier si la position est valide sur le NavMesh
            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit navHit, Mathf.Max(tile.Bounds.size.x, tile.Bounds.size.z), 1))
            {
                return navHit.position;
            }
            return randomPosition;
        }

        [ClientRpc]
        public void SetEnemyFocusClientRpc(int playerId, NetworkObjectReference enemyObject)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
            if (enemyObject.TryGet(out NetworkObject networkObject) && networkObject.IsSpawned)
            {
                EnemyAI enemyAI = networkObject.GetComponentInChildren<EnemyAI>();
                if (enemyAI != null
                    && enemyAI.thisNetworkObject != null)
                {
                    enemyAI.SetMovingTowardsTargetPlayer(player);
                }
            }
        }

        [ClientRpc]
        public void KillPlayerWhoCampClientRpc(int playerId, NetworkObjectReference enemyObject)
        {
            PlayerSTBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>();
            if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController && enemyObject.TryGet(out NetworkObject networkObject) && networkObject.IsSpawned)
            {
                EnemyAI enemyAI = networkObject.GetComponentInChildren<EnemyAI>();
                if (enemyAI != null
                    && enemyAI.thisNetworkObject != null
                    && !enemyAI.agent.CalculatePath(playerBehaviour.playerProperties.transform.position, enemyAI.path1))
                {
                    if (wasCampingLastSecond)
                    {
                        playerBehaviour.campTime++;

                        if (playerBehaviour.campTime == ConfigManager.campDuration.Value)
                        {
                            playerBehaviour.playerProperties.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Unknown);
                        }
                        else if (playerBehaviour.campTime % 5 == 0)
                        {
                            HUDManager.Instance.DisplayTip("Information", "You must move or there will be consequences!");
                        }
                    }
                    wasCampingLastSecond = true;
                }
                else
                {
                    wasCampingLastSecond = false;
                }
            }
        }

        public void EndGame(List<NetworkObject> spawnedEnemies, PlayerSTBehaviour playerBehaviour)
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
            UnlockDoorsClientRpc((int)playerBehaviour.playerProperties.playerClientId);
            SendEndGameClientRpc((int)playerBehaviour.playerProperties.playerClientId);
        }

        [ClientRpc]
        public void DestroyObjectClientRpc(NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                if (grabbableObject != null)
                {
                    grabbableObject.DestroyObjectInHand(null);
                }
            }
        }

        [ClientRpc]
        public void UnlockDoorsClientRpc(int playerId)
        {
            PlayerSTBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>();
            if (playerBehaviour != null)
            {
                foreach (DoorLock doorLock in playerBehaviour.tileGame.GetComponent<TileSTBehaviour>()?.doorLocks)
                {
                    if (!doorLock.isLocked)
                    {
                        if (doorLock.gameObject.TryGetComponent<AnimatedObjectTrigger>(out var triggerAnimation))
                        {
                            triggerAnimation.TriggerAnimationNonPlayer(playSecondaryAudios: false, overrideBool: true);
                            doorLock.OpenDoorAsEnemyServerRpc();
                        }
                    }
                    DoorLockPatch.blockedDoors.Remove(doorLock);
                }
                foreach (EntranceTeleport entranceTeleport in playerBehaviour.tileGame.GetComponent<TileSTBehaviour>()?.entranceTeleports)
                {
                    HUDManagerPatch.blockedEntrances.Remove(entranceTeleport);
                }
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

                if (player == GameNetworkManager.Instance.localPlayerController)
                {
                    HUDManager.Instance.DisplayTip("Information", "Congratulations, you survived!");
                }
            }

            playerBehaviour.campTime = 0;
            playerBehaviour.isInGame = false;
            playerBehaviour.tileGame = null;
            if (player == GameNetworkManager.Instance.localPlayerController && sawTheme != null)
            {
                sawTheme.Stop();
                Destroy(sawTheme);
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
