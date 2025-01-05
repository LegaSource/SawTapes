using BepInEx;
using GameNetcodeStuff;
using SawTapes.Files.Values;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Tapes
{
    public class SawTape : PhysicsProp
    {
        public bool isGameStarted = false;
        public bool isGameEnded = false;

        public AudioSource sawRecording;
        public AudioSource sawTheme;
        public HashSet<SubtitleMapping> subtitlesGame = new HashSet<SubtitleMapping>();
        public GameObject particleEffect;

        public int billyValue = 120;
        public int gameDuration = 60;

        public PlayerControllerB mainPlayer;
        public HashSet<PlayerControllerB> testedPlayers = new HashSet<PlayerControllerB>();
        public int currentTestedPlayersIndex = 0;

        public void InstantiateAndAttachAudio(GameObject audioPrefab)
        {
            GameObject audioObject = Instantiate(audioPrefab, transform.position, Quaternion.identity);
            audioObject.transform.SetParent(transform);
            sawRecording = audioObject.GetComponent<AudioSource>();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SelectTestedPlayersServerRpc()
        {
            for (int i = 1; i < currentTestedPlayersIndex; i++)
            {
                PlayerControllerB[] eligiblePlayers = StartOfRound.Instance.allPlayerScripts.Where(p => p.isPlayerControlled && !p.isPlayerDead && !testedPlayers.Contains(p)).ToArray();
                if (eligiblePlayers.Length > 0)
                {
                    testedPlayers.Add(eligiblePlayers[new System.Random().Next(eligiblePlayers.Length)]);
                    continue;
                }
            }

            if (testedPlayers.Count == currentTestedPlayersIndex)
            {
                SelectTestedPlayersClientRpc(testedPlayers.Select(p => (int)p.playerClientId).ToArray());
                ExecutePostSelectedPlayersForServer();
            }
            else
            {
                SawTapes.mls.LogWarning("Not enough players to play the game");
            }
        }

        [ClientRpc]
        public void SelectTestedPlayersClientRpc(int[] playerIds)
        {
            testedPlayers.Clear();
            foreach (int playerId in playerIds)
            {
                PlayerSTBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>();
                testedPlayers.Add(playerBehaviour.playerProperties);

                playerBehaviour.isInGame = true;
                SetSpecificFieldsForAllClients(playerBehaviour);
            }
        }

        public virtual void SetSpecificFieldsForAllClients(PlayerSTBehaviour playerBehaviour) { }

        public virtual void ExecutePostSelectedPlayersForServer() { }

        public override void GrabItem()
        {
            base.GrabItem();

            if (particleEffect != null)
                SawTapesNetworkManager.Instance.EnableBlackParticleServerRpc(GetComponent<NetworkObject>(), false);
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
                    if (playerBehaviour.isInGame)
                    {
                        PlaySawThemeServerRpc();
                        StartCoroutine(SawGameBeginCoroutine(playerBehaviour));
                    }
                    else
                    {
                        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_IMPAC_TESTED_PLAYER);
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlaySawTapeServerRpc() => PlaySawTapeClientRpc();

        [ClientRpc]
        public void PlaySawTapeClientRpc()
        {
            sawRecording.Play();
            if (ConfigManager.isSubtitles.Value)
                StartCoroutine(ShowSubtitles());
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlaySawThemeServerRpc() => PlaySawThemeClientRpc();

        [ClientRpc]
        public void PlaySawThemeClientRpc()
        {
            if (ConfigManager.isSawTheme.Value && (sawTheme == null || !sawTheme.isPlaying))
            {
                PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
                if (playerHeldBy == localPlayer || testedPlayers.Contains(localPlayer))
                {
                    GameObject audioObject = Instantiate(SawTapes.sawTheme, playerHeldBy.transform.position, Quaternion.identity);
                    sawTheme = audioObject.GetComponent<AudioSource>();
                    sawTheme.Play();
                    audioObject.transform.SetParent(playerHeldBy.transform);
                }
            }
        }

        public IEnumerator ShowSubtitles()
        {
            while (sawRecording.isPlaying)
            {
                string subtitleText = subtitlesGame.Where(s => s.Timestamp <= sawRecording.time).OrderByDescending(s => s.Timestamp).FirstOrDefault()?.Text;
                if (!string.IsNullOrEmpty(subtitleText))
                {
                    if (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, transform.position) <= 25)
                        HUDManagerPatch.subtitleText.text = subtitleText;
                    else
                        HUDManagerPatch.subtitleText.text = "";
                }
                yield return null;
            }
            HUDManagerPatch.subtitleText.text = "";
        }

        public IEnumerator SawGameBeginCoroutine(PlayerSTBehaviour playerBehaviour)
        {
            ExecutePreGameActionServerRpc((int)playerBehaviour.playerProperties.playerClientId);

            yield return new WaitUntil(() => sawRecording.isPlaying);
            yield return new WaitWhile(() => sawRecording.isPlaying);

            if (sawTheme != null)
                sawTheme.volume *= 1.5f;

            StartSawGameServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ExecutePreGameActionServerRpc(int playerId)
        {
            ExecutePreGameActionForServer(StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>());
            ExecutePreGameActionClientRpc(playerId);
        }

        public virtual void ExecutePreGameActionForServer(PlayerSTBehaviour playerBehaviour) { }

        [ClientRpc]
        public void ExecutePreGameActionClientRpc(int playerId)
            => ExecutePreGameActionForAllClients(StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>());

        public virtual void ExecutePreGameActionForAllClients(PlayerControllerB player)
        {
            mainPlayer = player;
            testedPlayers.Add(mainPlayer);
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartSawGameServerRpc()
        {
            ExecuteStartGameActionForServer();
            StartGameClientRpc(gameDuration);
            StartCoroutine(StartSawGameCoroutine());
        }

        public virtual void ExecuteStartGameActionForServer() { }

        [ClientRpc]
        public void StartGameClientRpc(int gameDuration)
            => ExecuteStartGameActionForAllClients(gameDuration);

        public virtual void ExecuteStartGameActionForAllClients(int gameDuration)
        {
            // Envoyé par le serveur car c'est lui qui met à jour le gameDuration depuis l'enfant
            if (testedPlayers.Contains(GameNetworkManager.Instance.localPlayerController))
                HUDManager.Instance.StartCoroutine(HUDManagerPatch.StartChronoCoroutine(gameDuration));

            isGameStarted = true;
        }

        public IEnumerator StartSawGameCoroutine()
        {
            int timePassed = 0;
            while (timePassed < gameDuration)
            {
                if (!DoGameForServer(timePassed)) break;
                yield return new WaitForSeconds(1f);
                timePassed++;
            }
            EndGameForServer();
        }

        public virtual bool DoGameForServer(int iterator) { return true; }

        public virtual void EndGameForServer(bool isGameCancelled = false)
        {
            bool isGameOver = ExecutePreEndGameActionForServer(isGameCancelled);
            if (!isGameCancelled)
            {
                if (isGameOver)
                    ObjectSTManager.EnableBlackParticle(this, true);
                else
                    StartCoroutine(SpawnBillyCoroutine(mainPlayer));
            }
            SendEndGameClientRpc(isGameOver, isGameCancelled);
        }

        public virtual bool ExecutePreEndGameActionForServer(bool isGameCancelled) { return true; }

        public IEnumerator SpawnBillyCoroutine(PlayerControllerB player)
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
                    break;
            }

            if (!foundPosition)
            {
                SawTapes.mls.LogWarning("Could not find a valid spawn position for Billy.");
                spawnPosition = player.transform.position;
            }

            NetworkObject networkObject = EnemySTManager.SpawnEnemy(SawTapes.billyEnemy, spawnPosition);
            SpawnBillyClientRpc(networkObject, (int)player.playerClientId, billyValue);
        }

        [ClientRpc]
        public void SpawnBillyClientRpc(NetworkObjectReference enemyObject, int playerId, int billyValue)
        {
            if (enemyObject.TryGet(out NetworkObject networkObject))
            {
                EnemyAI enemy = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
                if (enemy != null && enemy is Billy billy)
                {
                    billy.targetPlayer = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                    billy.billyValue = billyValue;
                    if (IsServer)
                        billy.StartFollowingPlayer();
                }
            }
        }

        [ClientRpc]
        public void SendEndGameClientRpc(bool isGameOver, bool isGameCancelled) => EndGameResetsForAllClients(isGameOver, isGameCancelled);

        public virtual void EndGameResetsForAllClients(bool isGameOver, bool isGameCancelled)
        {
            PlayerSTBehaviour playerBehaviour = mainPlayer.GetComponent<PlayerSTBehaviour>();

            if (!isGameOver || isGameCancelled)
            {
                if (playerBehaviour.tileGame != null)
                    SawTapes.eligibleTiles.RemoveAll(t => t == playerBehaviour.tileGame);
                isGameEnded = true;
            }

            isGameStarted = false;
            TileSTManager.OpenTileDoors(playerBehaviour);

            foreach (PlayerControllerB player in testedPlayers)
            {
                PlayerSTManager.ResetPlayerGame(player.GetComponent<PlayerSTBehaviour>());
                if (player == GameNetworkManager.Instance.localPlayerController)
                {
                    if (sawTheme != null)
                    {
                        sawTheme.Stop();
                        Destroy(sawTheme.gameObject);
                    }

                    if (!HUDManagerPatch.chronoText.text.IsNullOrWhiteSpace())
                        HUDManagerPatch.isChronoEnded = true;
                }
            }

            mainPlayer = null;
            testedPlayers.Clear();
        }
    }
}
