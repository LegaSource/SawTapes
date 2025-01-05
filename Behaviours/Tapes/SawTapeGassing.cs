using GameNetcodeStuff;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Tapes
{
    public class SawTapeGassing : SawTape
    {
        public bool isPlayerFinded = false;

        public override void ExecutePostSelectedPlayersForServer() => TeleportSelectedPlayersClientRpc();

        [ClientRpc]
        public void TeleportSelectedPlayersClientRpc()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (localPlayer != mainPlayer && testedPlayers.Contains(localPlayer))
                StartCoroutine(GameSetUpCoroutine(false));
        }

        public override void Update()
        {
            base.Update();
            FindPlayerInRange();
        }

        public void FindPlayerInRange()
        {
            if (!isPlayerFinded
                && GameNetworkManager.Instance?.localPlayerController != null
                && !GameNetworkManager.Instance.localPlayerController.isPlayerDead
                && Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, transform.position) <= ConfigManager.gassingDistance.Value)
            {
                if (currentTestedPlayersIndex == 0 || StartOfRound.Instance.allPlayerScripts.Where(p => p.isPlayerControlled && !p.isPlayerDead).Count() >= currentTestedPlayersIndex)
                {
                    PlayerControllerB player = StartOfRound.Instance.allPlayerScripts
                    .Where(p => Vector3.Distance(p.transform.position, transform.position) <= ConfigManager.gassingDistance.Value)
                    .OrderBy(p => Vector3.Distance(p.transform.position, transform.position))
                    .FirstOrDefault();

                    if (GameNetworkManager.Instance.localPlayerController == player)
                    {
                        isPlayerFinded = true;
                        PlayerFindedServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                        StartCoroutine(GameSetUpCoroutine());
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayerFindedServerRpc(int playerId) => PlayerFindedClientRpc(playerId);

        [ClientRpc]
        public void PlayerFindedClientRpc(int playerId)
        {
            isPlayerFinded = true;
            mainPlayer = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            testedPlayers.Add(mainPlayer);
            ExecutePlayerFlindedActionForAllClients();
        }

        public virtual void ExecutePlayerFlindedActionForAllClients() { }

        public IEnumerator GameSetUpCoroutine(bool isFindedPlayer = true)
        {
            if (isFindedPlayer)
                ExecutePreGassedSetUpActionForClient();

            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            float originalMovementSpeed = localPlayer.movementSpeed;

            HUDManagerPatch.isFlashFilterUsed = true;
            PlaySteamParticleServerRpc((int)localPlayer.playerClientId);
            float timePassed = 0f;
            while (timePassed < 5f)
            {
                yield return new WaitForSeconds(0.2f);
                timePassed += 0.2f;

                ApplyGasEffects(localPlayer, true, intensity: timePassed);
            }

            yield return new WaitForSeconds(1f);

            TeleportPlayer(localPlayer);

            Vector3 position = localPlayer.gameplayCamera.transform.position + localPlayer.gameplayCamera.transform.forward;
            if (isFindedPlayer)
            {
                SawTapesNetworkManager.Instance.ChangeObjectPositionServerRpc(GetComponent<NetworkObject>(), position);
                ExecutePostGassedSetUpActionForClient();
            }

            ExecutePostGassedSetUpActionServerRpc((int)localPlayer.playerClientId);
            ApplyGasEffects(localPlayer, false, originalMovementSpeed: originalMovementSpeed);
            StartCoroutine(CheckPlayerPositionCoroutine(position));
        }

        public virtual void ExecutePreGassedSetUpActionForClient() { }

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

        public void ApplyGasEffects(PlayerControllerB player, bool isBeingGassed, float intensity = 0f, float originalMovementSpeed = 0f)
        {
            HUDManager.Instance.HideHUD(isBeingGassed);
            if (isBeingGassed)
            {
                HUDManager.Instance.flashbangScreenFilter.weight = Mathf.Min(1f, intensity / 5f);
                IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint", false).Disable();
                if (intensity >= 3f)
                    IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).Disable();
                else
                    player.movementSpeed -= intensity / 10f;
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

        public void TeleportPlayer(PlayerControllerB player)
        {
            Transform entrancePoint = STUtilities.FindMainEntrancePoint();
            player.DropAllHeldItemsAndSync();
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.TeleportPlayer(entrancePoint.position, withRotation: true, entrancePoint.eulerAngles.y);
            player.isInElevator = false;
            player.isInHangarShipRoom = false;
            player.isInsideFactory = true;
            TeleportPlayerServerRpc((int)player.playerClientId);
            player.SpawnPlayerAnimation();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportPlayerServerRpc(int playerId) => TeleportPlayerClientRpc(playerId);

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
                if (player.ItemSlots[i] != null)
                    player.ItemSlots[i].isInFactory = true;
            }
        }

        public virtual void ExecutePostGassedSetUpActionForClient() { }

        [ServerRpc(RequireOwnership = false)]
        public void ExecutePostGassedSetUpActionServerRpc(int playerId) => ExecutePostGassedSetUpActionClientRpc(playerId);

        [ClientRpc]
        public void ExecutePostGassedSetUpActionClientRpc(int playerId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            ExecutePostGassedSetUpActionForAllClients(player);
        }

        public virtual void ExecutePostGassedSetUpActionForAllClients(PlayerControllerB player)
            => player.GetComponent<PlayerSTBehaviour>().hasBeenGassed = true;

        public IEnumerator CheckPlayerPositionCoroutine(Vector3 position)
        {
            while (!sawRecording.isPlaying)
            {
                PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
                if (Vector3.Distance(localPlayer.transform.position, position) > ConfigManager.gassingCheatDistance.Value)
                {
                    localPlayer.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Unknown);
                    HUDManager.Instance.DisplayTip(Constants.INFORMATION, Constants.MESSAGE_INFO_CHEAT);
                    ForceEndGameServerRpc();
                    yield break;
                }
                yield return new WaitForSeconds(1f);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ForceEndGameServerRpc() => EndGameForServer();

        public override void EndGameResetsForAllClients(bool isGameOver, bool isGameCancelled)
        {
            base.EndGameResetsForAllClients(isGameOver, isGameCancelled);
            isPlayerFinded = isGameCancelled || !isGameOver;
        }
    }
}
