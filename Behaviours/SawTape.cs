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

namespace SawTapes.Behaviours
{
    public abstract class SawTape : PhysicsProp
    {
        public bool isGameStarted = false;
        public bool isGameEnded = false;
        public AudioSource sawRecording;
        public AudioSource sawTheme;
        public GameObject particleEffect;
        public List<SubtitleMapping> subtitlesGame = new List<SubtitleMapping>();
        public int gameDuration = 60;
        public int billyValue = 120;

        public void InstantiateAndAttachAudio(GameObject audioPrefab)
        {
            GameObject audioObject = Instantiate(audioPrefab, transform.position, Quaternion.identity);
            audioObject.transform.SetParent(transform);
            sawRecording = audioObject.GetComponent<AudioSource>();
        }

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
            if (buttonDown && playerHeldBy != null && !sawRecording.isPlaying)
            {
                PlaySawTapeServerRpc();
                PlayerSTBehaviour playerBehaviour = playerHeldBy.GetComponent<PlayerSTBehaviour>();
                if (!isGameStarted && !isGameEnded)
                {
                    if (playerBehaviour.isInGame)
                    {
                        if (ConfigManager.isSawTheme.Value && (sawTheme == null || !sawTheme.isPlaying))
                        {
                            GameObject audioObject = Instantiate(SawTapes.sawTheme, playerHeldBy.transform.position, Quaternion.identity);
                            sawTheme = audioObject.GetComponent<AudioSource>();
                            sawTheme.Play();
                            audioObject.transform.SetParent(playerHeldBy.transform);
                        }
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
            if (ConfigManager.isSubtitles.Value) StartCoroutine(ShowSubtitles());
        }

        public IEnumerator ShowSubtitles()
        {
            while (sawRecording.isPlaying)
            {
                string subtitleText = subtitlesGame.Where(s => s.Timestamp <= sawRecording.time).OrderByDescending(s => s.Timestamp).FirstOrDefault()?.Text;
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

        public virtual IEnumerator SawGameBeginCoroutine(PlayerSTBehaviour playerBehaviour)
        {
            ExecutePreGameActionServerRpc((int)playerBehaviour.playerProperties.playerClientId);

            yield return new WaitUntil(() => sawRecording.isPlaying);
            yield return new WaitWhile(() => sawRecording.isPlaying);

            if (sawTheme != null)
            {
                sawTheme.volume *= 1.5f;
            }

            StartSawGameServerRpc((int)playerBehaviour.playerProperties.playerClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ExecutePreGameActionServerRpc(int playerId)
            => ExecutePreGameAction(StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>());

        public abstract void ExecutePreGameAction(PlayerSTBehaviour playerBehaviour);

        [ServerRpc(RequireOwnership = false)]
        public void StartSawGameServerRpc(int playerId)
        {
            StartGameClientRpc(playerId, gameDuration);
            StartCoroutine(StartSawGameCoroutine(StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>()));
        }

        [ClientRpc]
        public void StartGameClientRpc(int playerId, int gameDuration)
        {
            ExecuteStartGameAction(StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>(), gameDuration);
        }

        public virtual void ExecuteStartGameAction(PlayerSTBehaviour playerBehaviour, int gameDuration)
        {
            if (GameNetworkManager.Instance.localPlayerController == playerBehaviour.playerProperties)
            {
                // Envoyé par le serveur car c'est lui qui met à jour cette durée depuis l'enfant
                HUDManager.Instance.StartCoroutine(HUDManagerPatch.StartChronoCoroutine(gameDuration));
            }
            isGameStarted = true;
        }

        public IEnumerator StartSawGameCoroutine(PlayerSTBehaviour playerBehaviour)
        {
            int timePassed = 0;
            while (timePassed < gameDuration)
            {
                if (!DoGame(playerBehaviour, timePassed)) break;
                yield return new WaitForSeconds(1f);
                timePassed++;
            }
            EndGame(playerBehaviour);
        }

        public abstract bool DoGame(PlayerSTBehaviour playerBehaviour, int iterator);

        public virtual void EndGame(PlayerSTBehaviour playerBehaviour)
        {
            bool isGameOver = ExecutePreEndGameAction(playerBehaviour);
            if (isGameOver) TapeSTManager.EnableParticle(this, true);
            else StartCoroutine(SpawnBillyCoroutine(playerBehaviour.playerProperties));
            SendEndGameClientRpc((int)playerBehaviour.playerProperties.playerClientId, isGameOver);
        }

        public abstract bool ExecutePreEndGameAction(PlayerSTBehaviour playerBehaviour);

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
                {
                    break;
                }
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

        [ClientRpc]
        public void SendEndGameClientRpc(int playerId, bool isGameOver)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
            EndGameResets(ref player, isGameOver);
        }

        public virtual void EndGameResets(ref PlayerControllerB player, bool isGameOver)
        {
            PlayerSTBehaviour playerBehaviour = player.GetComponentInChildren<PlayerSTBehaviour>();

            if (!isGameOver)
            {
                if (playerBehaviour.tileGame != null) SawTapes.eligibleTiles.RemoveAll(t => t == playerBehaviour.tileGame);
                isGameEnded = true;
            }

            isGameStarted = false;
            PlayerSTManager.ResetPlayerGame(ref playerBehaviour);
            if (player == GameNetworkManager.Instance.localPlayerController)
            {
                if (sawTheme != null)
                {
                    sawTheme.Stop();
                    Destroy(sawTheme.gameObject);
                }

                if (!HUDManagerPatch.chronoText.text.IsNullOrWhiteSpace())
                {
                    HUDManagerPatch.isChronoEnded = true;
                }
            }
        }
    }
}
