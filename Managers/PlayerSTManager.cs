using GameNetcodeStuff;
using SawTapes.Behaviours;
using SawTapes.Behaviours.Tapes;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    public class PlayerSTManager
    {
        public static Dictionary<PlayerControllerB, PlayerSTBehaviour> playerBehavioursCache = new();

        public static void AddPlayerBehaviour(PlayerControllerB player)
        {
            if (!player.isPlayerControlled || player.GetComponent<PlayerSTBehaviour>() != null) return;

            PlayerSTBehaviour playerBehaviour = player.gameObject.AddComponent<PlayerSTBehaviour>();
            playerBehaviour.playerProperties = player;
        }

        public static PlayerSTBehaviour GetPlayerBehaviour(PlayerControllerB player)
        {
            if (player == null) return null;
            if (playerBehavioursCache.TryGetValue(player, out PlayerSTBehaviour playerBehaviour)) return playerBehaviour;
            
            playerBehaviour = player.GetComponent<PlayerSTBehaviour>();
            if (playerBehaviour == null)
            {
                AddPlayerBehaviour(player);
                playerBehaviour = playerBehavioursCache[player];
                return playerBehaviour;
            }
            playerBehavioursCache[player] = playerBehaviour;
            return playerBehaviour;
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
            if (playerBehaviour == null) return false;
            if (!playerBehaviour.isInGame || !playerBehaviour.hasBeenGassed) return false;

            if (player != GameNetworkManager.Instance.localPlayerController) return true;
            HUDManager.Instance.DisplayTip(Constants.INFORMATION, Constants.MESSAGE_INFO_IMP_ACTION);
            return true;
        }

        public static void ResetPlayerGame(PlayerControllerB player)
        {
            PlayerSTBehaviour playerBehaviour = GetPlayerBehaviour(player);
            if (playerBehaviour == null) return;

            playerBehaviour.isInGame = false;
            playerBehaviour.hasBeenGassed = false;
            playerBehaviour.sawTape = null;
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
            }
        }

        public static void TeleportSawToPlayer(PlayerSTBehaviour playerBehaviour)
        {
            if (playerBehaviour == null) return;

            EscapeTape escapeTape = playerBehaviour.sawTape as EscapeTape;
            if (escapeTape == null || escapeTape.saw == null) return;
            
            SawTapesNetworkManager.Instance.ChangeObjectPositionServerRpc(escapeTape.saw.GetComponent<NetworkObject>(), playerBehaviour.playerProperties.transform.position + Vector3.up * 0.5f);
        }
    }
}
