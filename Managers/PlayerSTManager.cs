using GameNetcodeStuff;
using SawTapes.Behaviours;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    public class PlayerSTManager
    {
        public static void AddPlayerBehaviour(PlayerControllerB player)
        {
            if (player.isPlayerControlled && player.GetComponent<PlayerSTBehaviour>() == null)
            {
                PlayerSTBehaviour playerBehaviour = player.gameObject.AddComponent<PlayerSTBehaviour>();
                playerBehaviour.playerProperties = player;
            }
        }

        public static IEnumerator SetUntargetablePlayerCoroutine(PlayerSTBehaviour playerBehaviour, float duration)
        {
            playerBehaviour.isTargetable = false;
            yield return new WaitForSeconds(duration);
            playerBehaviour.isTargetable = true;
        }

        public static bool PreventTeleportPlayer(PlayerControllerB player)
        {
            PlayerSTBehaviour playerBehaviour = player.GetComponent<PlayerSTBehaviour>();
            if (playerBehaviour != null
                && playerBehaviour.isInGame
                && (playerBehaviour.tileGame != null || playerBehaviour.hasBeenGassed))
            {
                player.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Unknown);
                if (player == GameNetworkManager.Instance.localPlayerController)
                    HUDManager.Instance.DisplayTip(Constants.INFORMATION, Constants.MESSAGE_INFO_CHEAT);
                return true;
            }
            return false;
        }

        public static void ResetPlayerGame(PlayerSTBehaviour playerBehaviour)
        {
            playerBehaviour.campTime = 0;
            playerBehaviour.isInGame = false;
            playerBehaviour.tileGame = null;
            playerBehaviour.hasBeenGassed = false;
            playerBehaviour.huntingTape = null;
            playerBehaviour.escapeTape = null;
        }

        public static void SecondaryUsePerformed(PlayerSTBehaviour playerBehaviour)
        {
            switch (playerBehaviour.currentControlTipState)
            {
                case (int)PlayerSTBehaviour.ControlTip.NONE:
                    return;
                case (int)PlayerSTBehaviour.ControlTip.SAW_ITEM:
                    TeleportSawToPlayer(playerBehaviour);
                    break;

                default:
                    return;
            }
        }

        public static void TeleportSawToPlayer(PlayerSTBehaviour playerBehaviour)
        {
            if (playerBehaviour.escapeTape?.saw != null)
                SawTapesNetworkManager.Instance.ChangeObjectPositionServerRpc(playerBehaviour.escapeTape.saw.GetComponent<NetworkObject>(), playerBehaviour.playerProperties.transform.position);
        }
    }
}
