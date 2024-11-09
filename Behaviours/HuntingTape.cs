using GameNetcodeStuff;
using SawTapes.Files;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours
{
    public class HuntingTape : SawTape
    {
        public bool isPlayerFinded = false;
        public ReverseBearTrap reverseBearTrap;

        public override void Start()
        {
            base.Start();
            InstantiateAndAttachAudio(SawTapes.sawRecordingHunting);
            subtitlesGame = SubtitleFile.huntingGameSubtitles;
        }

        public override void Update()
        {
            base.Update();
            FindPlayerInRange();
        }

        public void FindPlayerInRange()
        {
            if (!isPlayerFinded
                && !isInShipRoom
                && !GameNetworkManager.Instance.localPlayerController.isPlayerDead
                && Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, transform.position) <= ConfigManager.huntingGassedDistance.Value)
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerScripts
                    .Where(p => Vector3.Distance(p.transform.position, transform.position) <= ConfigManager.huntingGassedDistance.Value)
                    .OrderBy(p => Vector3.Distance(p.transform.position, transform.position))
                    .FirstOrDefault();

                if (GameNetworkManager.Instance.localPlayerController == player)
                {
                    isPlayerFinded = true;
                    PlayerFindedServerRpc();
                    StartCoroutine(TeleportPlayerCoroutine());
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayerFindedServerRpc() => PlayerFindedClientRpc();

        [ClientRpc]
        public void PlayerFindedClientRpc() => isPlayerFinded = true;

        public IEnumerator TeleportPlayerCoroutine()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            float originalMovementSpeed = localPlayer.movementSpeed;

            HUDManagerPatch.isFlashFilterUsed = true;
            PlaySteamParticleServerRpc((int)localPlayer.playerClientId);
            float timePassed = 0f;
            while (timePassed < 5f)
            {
                yield return new WaitForSeconds(0.2f);
                timePassed += 0.2f;

                ApplyGasEffects(ref localPlayer, true, intensity: timePassed);
            }

            yield return new WaitForSeconds(1f);

            TeleportPlayer(ref localPlayer);
            SpawnReverseBearTrapServerRpc((int)localPlayer.playerClientId);
            SpawnShovelServerRpc();
            ApplyGasEffects(ref localPlayer, false, originalMovementSpeed: originalMovementSpeed);
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlaySteamParticleServerRpc(int playerId) => PlaySteamParticleClientRpc(playerId);

        [ClientRpc]
        public void PlaySteamParticleClientRpc(int playerId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();

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

        public void ApplyGasEffects(ref PlayerControllerB player, bool isBeingGassed, float intensity = 0f, float originalMovementSpeed = 0f)
        {
            HUDManager.Instance.HideHUD(isBeingGassed);
            if (isBeingGassed)
            {
                HUDManager.Instance.flashbangScreenFilter.weight = Mathf.Min(1f, intensity / 5f);
                IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint", false).Disable();
                if (intensity >= 3f)
                {
                    IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).Disable();
                }
                else
                {
                    player.movementSpeed -= intensity / 10f;
                }
            }
            else
            {
                HUDManagerPatch.isFlashFilterUsed = false;
                HUDManager.Instance.flashbangScreenFilter.weight = 0f;
                IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).Enable();
                IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint", false).Enable();
                player.movementSpeed = originalMovementSpeed;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnReverseBearTrapServerRpc(int playerId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            GrabbableObject reverseBearTrap = RoundManagerPatch.SpawnItem(ref SawTapes.reverseBearTrapObj, player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation);
            InitializeReverseBearTrapClientRpc(reverseBearTrap.GetComponent<NetworkObject>(), (int)player.playerClientId);
        }

        [ClientRpc]
        public void InitializeReverseBearTrapClientRpc(NetworkObjectReference obj, int playerId)
        {
            if (obj.TryGet(out var networkObject))
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                ReverseBearTrap reverseBearTrap = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as ReverseBearTrap;
                reverseBearTrap.grabbable = false;
                reverseBearTrap.grabbableToEnemies = false;
                reverseBearTrap.hasHitGround = false;
                reverseBearTrap.EnablePhysics(enable: false);
                reverseBearTrap.SetScrapValue(ConfigManager.huntingReverseBearTrapValue.Value);

                if (GameNetworkManager.Instance.localPlayerController == player)
                {
                    reverseBearTrap.parentObject = player.gameplayCamera.transform;
                    StartCoroutine(CheckPlayerPositionCoroutine(transform.position));
                }
                else
                {
                    reverseBearTrap.parentObject = player.playerGlobalHead.transform;
                }

                PlayerSTBehaviour playerBehaviour = player.GetComponent<PlayerSTBehaviour>();
                playerBehaviour.isInGame = true;
                playerBehaviour.assignedReverseBearTrap = reverseBearTrap;
                this.reverseBearTrap = reverseBearTrap;
            }
        }

        public IEnumerator CheckPlayerPositionCoroutine(Vector3 position)
        {
            while (!sawRecording.isPlaying)
            {
                if (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, position) > ConfigManager.huntingCheatDistance.Value)
                {
                    GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Unknown);
                    HUDManager.Instance.DisplayTip(Constants.INFORMATION, Constants.MESSAGE_INFO_CHEAT);
                    ForceEndGameServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, true);
                    yield break;
                }
                yield return new WaitForSeconds(1f);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ForceEndGameServerRpc(int playerId, bool isGameOver)
        {
            DestroyReverseBearTrap();
            SendEndGameClientRpc(playerId, isGameOver);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnShovelServerRpc()
        {
            GameObject shovel = null;
            foreach (NetworkPrefabsList networkPrefabList in NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists ?? Enumerable.Empty<NetworkPrefabsList>())
            {
                foreach (NetworkPrefab networkPrefab in networkPrefabList.PrefabList ?? Enumerable.Empty<NetworkPrefab>())
                {
                    GrabbableObject grabbableObject = networkPrefab.Prefab.GetComponent<GrabbableObject>();
                    if (grabbableObject != null && grabbableObject.itemProperties.itemName.Equals(Constants.SHOVEL))
                    {
                        shovel = networkPrefab.Prefab;
                        if (shovel != null) break;
                    }
                }
            }
            if (shovel != null) RoundManagerPatch.SpawnItem(ref shovel, transform.position + Vector3.up * 0.5f);
        }

        public void TeleportPlayer(ref PlayerControllerB player)
        {
            player.DropAllHeldItemsAndSync();
            Vector3 position = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(transform.position, 3f);
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.TeleportPlayer(position);
            player.transform.rotation = Quaternion.LookRotation(transform.position - player.transform.position);
            player.SpawnPlayerAnimation();
        }

        public override void ExecutePreGameAction(PlayerSTBehaviour playerBehaviour)
        {
            SpawnEnemy(ref playerBehaviour.playerProperties);
            gameDuration = ConfigManager.huntingDuration.Value;
            billyValue = ConfigManager.huntingBillyValue.Value;
        }

        public override void ExecuteStartGameAction(PlayerSTBehaviour playerBehaviour, int gameDuration)
        {
            base.ExecuteStartGameAction(playerBehaviour, gameDuration);
            if (GameNetworkManager.Instance.localPlayerController == playerBehaviour.playerProperties)
            {
                StartCoroutine(STUtilities.ShowEnemyCoroutine(playerBehaviour.assignedEnemy));
            }
        }

        public void SpawnEnemy(ref PlayerControllerB player)
        {
            List<EnemyType> killableEnemies = SawTapes.allEnemies.Where(e => e.canDie && !e.isOutsideEnemy && !ConfigManager.huntingExclusions.Value.Contains(e.enemyName)).ToList();
            EnemyType enemyType = killableEnemies[new System.Random().Next(killableEnemies.Count)];
            Vector3 spawnPosition = EnemySTManager.GetFurthestPositionFromPlayer(player);
            NetworkObject networkObject = EnemySTManager.SpawnEnemy(enemyType, spawnPosition);
            SetAssignedEnemyClientRpc((int)player.playerClientId, networkObject);
        }

        [ClientRpc]
        public void SetAssignedEnemyClientRpc(int playerId, NetworkObjectReference enemyObject)
        {
            if (enemyObject.TryGet(out NetworkObject networkObject))
            {
                PlayerSTBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>();
                playerBehaviour.assignedEnemy = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
                playerBehaviour.assignedEnemy.enemyType.enemyPrefab.AddComponent<EnemySTBehaviour>().isAssignedEnemy = true;
            }
        }

        public override bool DoGame(PlayerSTBehaviour playerBehaviour, int iterator)
            => !(playerBehaviour.playerProperties.isPlayerDead || reverseBearTrap.isReleased);

        public override bool ExecutePreEndGameAction(PlayerSTBehaviour playerBehaviour)
        {
            if (playerBehaviour.playerProperties.isPlayerDead || !reverseBearTrap.isReleased)
            {
                DestroyReverseBearTrap();
                DestroySawKey();
                if (!playerBehaviour.playerProperties.isPlayerDead) SawTapesNetworkManager.Instance.KillPlayerClientRpc((int)playerBehaviour.playerProperties.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
                return true;
            }
            return false;
        }

        public void DestroyReverseBearTrap()
        {
            if (reverseBearTrap != null)
            {
                SawTapesNetworkManager.Instance.DestroyObjectClientRpc(reverseBearTrap.GetComponent<NetworkObject>());
            }
        }

        public void DestroySawKey()
        {
            SawKey sawKey = FindFirstObjectByType<SawKey>();
            if (sawKey != null)
            {
                SawTapesNetworkManager.Instance.DestroyObjectClientRpc(sawKey.GetComponent<NetworkObject>());
            }
        }

        public override void EndGameResets(ref PlayerControllerB player, bool isGameOver)
        {
            base.EndGameResets(ref player, isGameOver);
            isPlayerFinded = !isGameOver;
            reverseBearTrap = null;
        }
    }
}
