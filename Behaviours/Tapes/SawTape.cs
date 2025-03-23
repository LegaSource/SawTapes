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
        public bool isGameEnded;
        public bool isPlayerFinded = false;
        public float delayTimer = 10f;

        public int gameDuration;
        public int billyValue;

        public AudioSource sawTheme;
        public AudioSource sawRecording;
        public HashSet<SubtitleMapping> subtitlesGame = new HashSet<SubtitleMapping>();

        public int minPlayersAmount;
        public int maxPlayersAmount;
        public int playersAmount;
        public HashSet<PlayerControllerB> players = new HashSet<PlayerControllerB>();

        public override void Start()
        {
            base.Start();
            isGameEnded = scrapValue != 0 || STUtilities.FindMainEntrancePoint() == null;
        }

        public virtual void InstantiateAndAttachAudio(GameObject audioPrefab)
        {
            GameObject audioObject = Instantiate(audioPrefab, transform.position, Quaternion.identity);
            audioObject.transform.SetParent(transform);
            sawRecording = audioObject.GetComponent<AudioSource>();
        }

        public override void Update()
        {
            base.Update();
            FindPlayerInRange();
        }

        public void FindPlayerInRange()
        {
            if (isGameStarted || isGameEnded || isPlayerFinded) return;

            delayTimer -= Time.deltaTime;
            if (delayTimer > 0f) return;

            PlayerControllerB localPlayer = GameNetworkManager.Instance?.localPlayerController;
            if (localPlayer == null) return;
            if (!localPlayer.IsHost && !localPlayer.IsServer) return;

            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts
                .FirstOrDefault(p =>
                    p.isPlayerControlled
                    && !p.isPlayerDead
                    && Vector3.Distance(p.transform.position, transform.position) <= ConfigManager.gassingDistance.Value);
            if (player == null) return;

            isPlayerFinded = true;
            delayTimer = 10f;

            players.Clear();
            players.Add(player);

            if (!SetPlayersAmount()) return;
            SelectPlayers();
        }

        public bool SetPlayersAmount()
        {
            playersAmount = StartOfRound.Instance.allPlayerScripts.Count(p => p.isPlayerControlled && !p.isPlayerDead);
            if (playersAmount < minPlayersAmount) return false;

            /*
             * Si minPlayersAmount = -1 -> on prend tous les joueurs
             * Si maxPlayersAmount = -1 -> on prend un nombre de joueurs aléatoires entre le minPlayersAmount et le nombre de joueurs disponibles
             */
            if (maxPlayersAmount == -1)
            {
                minPlayersAmount = minPlayersAmount == -1 ? playersAmount : minPlayersAmount;
                maxPlayersAmount = playersAmount;
            }
            playersAmount = new System.Random().Next(minPlayersAmount, Mathf.Min(playersAmount, maxPlayersAmount));
            return true;
        }

        public void SelectPlayers()
        {
            for (int i = 1; i < playersAmount; i++)
            {
                List<PlayerControllerB> eligiblePlayers = StartOfRound.Instance.allPlayerScripts
                    .Where(p => p.isPlayerControlled && !p.isPlayerDead && !players.Contains(p))
                    .ToList();

                PlayerControllerB player = eligiblePlayers.Count > 0
                    ? eligiblePlayers[Random.Range(0, eligiblePlayers.Count)]
                    : null;

                if (player == null)
                {
                    SawTapes.mls.LogWarning("Not enough players to play the game");
                    isGameEnded = true;
                    return;
                }
                players.Add(player);
            }
            AffectPlayersClientRpc(players.Select(p => (int)p.playerClientId).ToArray());
            GasPlayersClientRpc();
            ExecutePostGasActionsForServer();
        }

        [ClientRpc]
        public void AffectPlayersClientRpc(int[] playerIds)
        {
            players.Clear();
            foreach (int playerId in playerIds)
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(player);
                if (playerBehaviour == null) return;

                players.Add(player);
                playerBehaviour.isInGame = true;
                playerBehaviour.sawTape = this;
            }
        }

        [ClientRpc]
        public void GasPlayersClientRpc()
        {
            if (!players.Contains(GameNetworkManager.Instance.localPlayerController)) return;
            StartCoroutine(GasPlayersCoroutine());
        }

        public IEnumerator GasPlayersCoroutine()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            HUDManagerPatch.isFlashFilterUsed = true;
            PlaySteamParticle(localPlayer);
            float timePassed = 0f;
            while (timePassed < 5f)
            {
                yield return new WaitForSeconds(0.2f);
                timePassed += 0.2f;

                ApplyGasEffects(true, intensity: timePassed);
            }

            yield return new WaitForSeconds(1f);

            ExecutePostGasActionsForClient(localPlayer);
        }

        public void PlaySteamParticle(PlayerControllerB player)
        {
            // Steam particle
            GameObject particleObject = Instantiate(SawTapes.steamParticle, player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation);
            particleObject.transform.SetParent(player.transform);

            ParticleSystem steamParticle = particleObject.GetComponent<ParticleSystem>();
            Destroy(particleObject, steamParticle.main.duration + steamParticle.main.startLifetime.constantMax);

            // Audio steam particle
            GameObject audioObject = Instantiate(SawTapes.steamAudio, player.gameplayCamera.transform.position, Quaternion.identity);
            audioObject.transform.SetParent(player.transform);

            AudioSource steamAudio = audioObject.GetComponent<AudioSource>();
            Destroy(steamAudio, steamAudio.clip.length);
        }

        public void ApplyGasEffects(bool isBeingGassed, float intensity = 0f)
        {
            HUDManager.Instance.HideHUD(isBeingGassed);
            if (isBeingGassed)
            {
                HUDManager.Instance.flashbangScreenFilter.weight = Mathf.Min(1f, intensity / 5f);
                return;
            }
            HUDManagerPatch.isFlashFilterUsed = false;
            HUDManager.Instance.flashbangScreenFilter.weight = 0f;
        }

        public virtual void ExecutePostGasActionsForClient(PlayerControllerB player)
        {
            TeleportPlayer(player);
            ApplyGasEffects(false);

            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(player);
            if (playerBehaviour == null) return;

            playerBehaviour.hasBeenGassed = true;
        }

        public void TeleportPlayer(PlayerControllerB player)
        {
            player.DropAllHeldItemsAndSync();
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.isInElevator = false;
            player.isInHangarShipRoom = false;
            player.isInsideFactory = true;

            Transform entrancePoint = STUtilities.FindMainEntrancePoint();
            player.TeleportPlayer(entrancePoint.position, withRotation: true, entrancePoint.eulerAngles.y);
            player.SpawnPlayerAnimation();

            TeleportPlayerServerRpc((int)player.playerClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportPlayerServerRpc(int playerId)
            => TeleportPlayerClientRpc(playerId);

        [ClientRpc]
        public void TeleportPlayerClientRpc(int playerId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player != GameNetworkManager.Instance.localPlayerController)
            {
                Transform entrancePoint = STUtilities.FindMainEntrancePoint();
                player.TeleportPlayer(entrancePoint.position, withRotation: true, entrancePoint.eulerAngles.y);
                player.isInElevator = false;
                player.isInHangarShipRoom = false;
                player.isInsideFactory = true;
            }
            for (int i = 0; i < player.ItemSlots.Length; i++)
            {
                if (player.ItemSlots[i] == null) continue;
                player.ItemSlots[i].isInFactory = true;
            }
        }

        public virtual void ExecutePostGasActionsForServer()
        {
            Transform entrancePoint = STUtilities.FindMainEntrancePoint();
            SawTapesNetworkManager.Instance.ChangeObjectPositionServerRpc(GetComponent<NetworkObject>(), entrancePoint.position + entrancePoint.forward + Vector3.up * 0.5f);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (!buttonDown || playerHeldBy == null || sawRecording.isPlaying) return;

            PlayRecordingServerRpc();

            if (isGameStarted || isGameEnded) return;

            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(playerHeldBy);
            if (playerBehaviour == null || !playerBehaviour.isInGame)
            {
                HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_IMPAC_TESTED_PLAYER);
                return;
            }
            PlaySawThemeServerRpc();
            StartCoroutine(BeginSawGameCoroutine());
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayRecordingServerRpc()
            => PlayRecordingClientRpc();

        [ClientRpc]
        public void PlayRecordingClientRpc()
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
                        HUDManagerPatch.subtitleText.text = subtitleText;
                    else
                        HUDManagerPatch.subtitleText.text = "";
                }
                yield return null;
            }
            HUDManagerPatch.subtitleText.text = "";
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlaySawThemeServerRpc()
            => PlaySawThemeClientRpc();

        [ClientRpc]
        public void PlaySawThemeClientRpc()
        {
            if (!ConfigManager.isSawTheme.Value) return;
            if (sawTheme != null && sawTheme.isPlaying) return;

            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (!players.Contains(player)) return;

            GameObject audioObject = Instantiate(SawTapes.sawTheme, player.transform.position, Quaternion.identity);
            sawTheme = audioObject.GetComponent<AudioSource>();
            sawTheme.Play();
            audioObject.transform.SetParent(player.transform);
        }

        public IEnumerator BeginSawGameCoroutine()
        {
            yield return new WaitUntil(() => sawRecording.isPlaying);
            yield return new WaitUntil(() => !sawRecording.isPlaying);

            if (sawTheme != null) sawTheme.volume *= 1.5f;
            BeginSawGameServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void BeginSawGameServerRpc()
        {
            ExecuteStartGameActionsForServer();
            StartGameClientRpc();
        }

        public virtual void ExecuteStartGameActionsForServer() { }

        [ClientRpc]
        public void StartGameClientRpc()
            => ExecuteStartGameActionsForAllClients();

        public virtual void ExecuteStartGameActionsForAllClients()
        {
            isGameStarted = true;
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

            if (player.IsHost || player.IsServer) StartCoroutine(StartGameCoroutine());

            if (!players.Contains(player)) return;
            HUDManager.Instance.StartCoroutine(HUDManagerPatch.StartChronoCoroutine(gameDuration));
        }

        public IEnumerator StartGameCoroutine()
        {
            int timePassed = 0;
            while (timePassed < gameDuration)
            {
                if (!DoGameForServer(timePassed)) break;
                yield return new WaitForSecondsRealtime(1f);
                timePassed++;
            }
            EndGameForServer();
        }

        public virtual bool DoGameForServer(int iterator) { return true; }

        public virtual void EndGameForServer(bool isGameCancelled = false)
        {
            PlayerControllerB player = players.FirstOrDefault(p => !p.isPlayerDead);

            bool isGameOver = ExecutePreEndGameActionForServer(isGameCancelled);
            EndGameClientRpc(!isGameOver || isGameCancelled);
            
            if (player == null) return;
            if (isGameOver || isGameCancelled) return;

            SawTapesNetworkManager.Instance.SetScrapValueClientRpc(GetComponent<NetworkObject>(), ConfigManager.sawTapeValue.Value);
            StartCoroutine(SpawnBillyCoroutine(player));
        }

        public virtual bool ExecutePreEndGameActionForServer(bool isGameCancelled) { return true; }

        [ClientRpc]
        public void EndGameClientRpc(bool isGameEnded)
            => EndGameForAllClients(isGameEnded);

        public virtual void EndGameForAllClients(bool isGameEnded)
        {
            isGameStarted = false;
            this.isGameEnded = isGameEnded;
            isPlayerFinded = isGameEnded;

            PlayerControllerB mainPlayer = players.FirstOrDefault();
            if (mainPlayer == null) return;

            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(mainPlayer);
            if (playerBehaviour == null) return;

            foreach (PlayerControllerB player in players)
            {
                PlayerSTManager.ResetPlayerGame(player);
                if (player != GameNetworkManager.Instance.localPlayerController) continue;

                if (sawTheme != null)
                {
                    sawTheme.Stop();
                    Destroy(sawTheme.gameObject);
                }

                if (!HUDManagerPatch.chronoText.text.IsNullOrWhiteSpace()) HUDManagerPatch.isChronoEnded = true;
            }

            players.Clear();
            CustomPassManager.RemoveAura();
        }

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

                if (foundPosition) break;
            }

            if (!foundPosition)
            {
                SawTapes.mls.LogWarning("Could not find a valid spawn position for Billy.");
                spawnPosition = player.transform.position;
            }

            NetworkObject networkObject = EnemySTManager.SpawnEnemyForServer(SawTapes.billyEnemy, spawnPosition);
            SpawnBillyClientRpc(networkObject, (int)player.playerClientId, billyValue);
        }

        [ClientRpc]
        public void SpawnBillyClientRpc(NetworkObjectReference enemyObject, int playerId, int billyValue)
        {
            if (!enemyObject.TryGet(out NetworkObject networkObject)) return;

            EnemyAI enemy = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
            if (enemy != null && enemy is Billy billy)
            {
                billy.targetPlayer = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                billy.billyValue = billyValue;
                if (IsServer) billy.StartFollowingPlayer();
            }
        }
    }
}
