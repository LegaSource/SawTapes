using GameNetcodeStuff;
using SawTapes.Behaviours.Items;
using SawTapes.Files;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Tapes
{
    public class EscapeTape : SawTapeGassing
    {
        public Chain chain;
        public Saw saw;
        public bool sawHasBeenUsed = false;

        public override void Start()
        {
            base.Start();

            InstantiateAndAttachAudio(SawTapes.sawRecordingEscape);
            subtitlesGame = SubtitleFile.escapeGameSubtitles;
            currentTestedPlayersIndex = 2;
        }

        public override void ExecutePreGassedSetUpActionForClient() => SelectTestedPlayersServerRpc();

        public override void SetSpecificFieldsForAllClients(PlayerSTBehaviour playerBehaviour) => playerBehaviour.escapeTape = this;

        public override void ExecutePostGassedSetUpActionForClient() => PostGameSetUpActionServerRpc();

        [ServerRpc(RequireOwnership = false)]
        public void PostGameSetUpActionServerRpc() => StartCoroutine(SetUpChainsCouroutine());

        public void AddPathGuide(PlayerControllerB player)
        {
            if (testedPlayers.Count == currentTestedPlayersIndex)
            {
                PathGuideBehaviour pathGuide = player.gameObject.AddComponent<PathGuideBehaviour>();
                pathGuide.saw = saw;
                pathGuide.sawTape = this;
                pathGuide.players = testedPlayers;
            }
        }

        public IEnumerator SetUpChainsCouroutine()
        {
            if (testedPlayers.Count == currentTestedPlayersIndex)
            {
                PlayerControllerB selectedPlayer = testedPlayers.FirstOrDefault(p => p != mainPlayer);

                yield return new WaitUntil(() => mainPlayer != null && selectedPlayer != null && Vector3.Distance(mainPlayer.transform.position, selectedPlayer.transform.position) < 8f);

                Vector3 position = (mainPlayer.transform.position + selectedPlayer.transform.position) / 2f + Vector3.up * 1.2f;
                chain = RoundManagerPatch.SpawnItem(SawTapes.chainObj, position) as Chain;
                chain.SetUpChainClientRpc((int)mainPlayer.playerClientId, (int)selectedPlayer.playerClientId);
            }
        }

        public void SpawnSaw(Vector3 position)
        {
            position = STUtilities.GetFurthestPositionScrapSpawn(position, SawTapes.sawItem);
            saw = RoundManagerPatch.SpawnItem(SawTapes.sawItem.spawnPrefab, position) as Saw;
            SawTapesNetworkManager.Instance.SetScrapValueClientRpc(saw.GetComponent<NetworkObject>(), ConfigManager.sawValue.Value);
        }

        public override void ExecutePreGameActionForServer(PlayerSTBehaviour playerBehaviour)
        {
            SpawnSaw(playerBehaviour.playerProperties.transform.position);
            gameDuration = ConfigManager.escapeDuration.Value;
            billyValue = ConfigManager.escapeBillyValue.Value;
        }

        public override void ExecuteStartGameActionForServer() => AddPathGuide(mainPlayer);

        public override bool DoGameForServer(int iterator)
            => !(testedPlayers.Any(p => p.isPlayerDead) || sawHasBeenUsed);

        public override bool ExecutePreEndGameActionForServer(bool isGameCancelled)
        {
            DestroyChain();
            DestroyPathGuide();
            if (testedPlayers.Any(p => p.isPlayerDead) || !sawHasBeenUsed)
            {
                DestroySaw();
                // On tue les joueurs encore en vie
                if (!isGameCancelled)
                {
                    foreach (PlayerControllerB alivePlayer in testedPlayers.Where(p => !p.isPlayerDead))
                        SawTapesNetworkManager.Instance.KillPlayerClientRpc((int)alivePlayer.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
                }
                return true;
            }
            return false;
        }

        public void DestroyChain()
        {
            if (chain != null)
            {
                NetworkObject networkObject = chain.GetComponent<NetworkObject>();
                if (networkObject != null && networkObject.IsSpawned)
                    networkObject.Despawn(destroy: true);
            }
        }

        public void DestroyPathGuide()
        {
            PathGuideBehaviour pathGuide = mainPlayer?.GetComponent<PathGuideBehaviour>();
            if (pathGuide != null)
                Destroy(pathGuide);
        }

        public void DestroySaw()
        {
            if (saw != null)
                SawTapesNetworkManager.Instance.DestroyObjectClientRpc(saw.GetComponent<NetworkObject>());
        }

        public override void EndGameResetsForAllClients(bool isGameOver, bool isGameCancelled)
        {
            base.EndGameResetsForAllClients(isGameOver, isGameCancelled);
            sawHasBeenUsed = false;
            GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerSTBehaviour>().currentControlTipState = (int)PlayerSTBehaviour.ControlTip.NONE;
        }
    }
}
