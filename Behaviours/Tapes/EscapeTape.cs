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
    public class EscapeTape : SawTape
    {
        public Chain chain;
        public Saw saw;
        public bool sawHasBeenUsed = false;

        public override void Start()
        {
            base.Start();

            InstantiateAndAttachAudio(SawTapes.sawRecordingEscape);
            subtitlesGame = SubtitleFile.escapeGameSubtitles;

            minPlayersAmount = 2;
            maxPlayersAmount = 2;

            gameDuration = ConfigManager.escapeDuration.Value;
            billyValue = ConfigManager.escapeBillyValue.Value;
        }

        public override void ExecutePostGasActionsForServer()
        {
            base.ExecutePostGasActionsForServer();
            StartCoroutine(SetUpChainsCouroutine());
        }

        public IEnumerator SetUpChainsCouroutine()
        {
            PlayerControllerB[] players = base.players.ToArray();
            if (players.Length == 2)
            {
                yield return new WaitUntil(() => Vector3.Distance(players[0].transform.position, players[1].transform.position) < 8f);

                Vector3 position = (players[0].transform.position + players[1].transform.position) / 2f + Vector3.up * 1.2f;
                chain = RoundManagerPatch.SpawnItem(SawTapes.chainObj, position) as Chain;
                chain.SetUpChainClientRpc((int)players[0].playerClientId, (int)players[1].playerClientId);
            }
        }

        public override void ExecuteStartGameActionsForServer()
        {
            base.ExecuteStartGameActionsForServer();

            PlayerControllerB player = players.FirstOrDefault();
            if (player == null) return;

            SpawnSaw(player.transform.position);
            AddPathGuide(player);
        }

        public void SpawnSaw(Vector3 position)
        {
            position = STUtilities.GetFurthestPositionScrapSpawn(position, SawTapes.sawItem);
            saw = RoundManagerPatch.SpawnItem(SawTapes.sawItem.spawnPrefab, position) as Saw;
            SawTapesNetworkManager.Instance.SetScrapValueClientRpc(saw.GetComponent<NetworkObject>(), ConfigManager.sawValue.Value);
        }

        public void AddPathGuide(PlayerControllerB player)
        {
            PathGuideBehaviour pathGuide = player.gameObject.AddComponent<PathGuideBehaviour>();
            pathGuide.saw = saw;
            pathGuide.sawTape = this;
            pathGuide.players = players;
        }

        public override bool DoGameForServer(int iterator)
            => !(players.Any(p => p.isPlayerDead) || sawHasBeenUsed);

        public override bool ExecutePreEndGameActionForServer(bool isGameCancelled)
        {
            base.ExecutePreEndGameActionForServer(isGameCancelled);

            DestroyChain();
            DestroyPathGuide();
            if (players.Any(p => p.isPlayerDead) || !sawHasBeenUsed)
            {
                DestroySaw();
                // On tue les joueurs encore en vie
                if (!isGameCancelled)
                {
                    foreach (PlayerControllerB alivePlayer in players.Where(p => !p.isPlayerDead))
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
                if (networkObject == null || !networkObject.IsSpawned) return;

                networkObject.Despawn(destroy: true);
            }
        }

        public void DestroyPathGuide()
        {
            PathGuideBehaviour pathGuide = players.Select(p => p.GetComponent<PathGuideBehaviour>()).FirstOrDefault();
            if (pathGuide != null) Destroy(pathGuide);
        }

        public void DestroySaw()
        {
            if (saw != null)
            {
                NetworkObject networkObject = saw.GetComponent<NetworkObject>();
                if (networkObject == null || !networkObject.IsSpawned) return;

                SawTapesNetworkManager.Instance.DestroyObjectClientRpc(networkObject);
            }
        }

        public override void EndGameForAllClients(bool isGameEnded)
        {
            base.EndGameForAllClients(isGameEnded);

            sawHasBeenUsed = false;

            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(GameNetworkManager.Instance.localPlayerController);
            if (playerBehaviour == null) return;

            playerBehaviour.currentControlTipState = (int)PlayerSTBehaviour.ControlTip.NONE;
        }
    }
}
