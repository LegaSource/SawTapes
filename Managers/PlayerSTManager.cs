using GameNetcodeStuff;
using SawTapes.Behaviours;
using System.Collections;
using System.Collections.Generic;
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
                if (playerBehavioursCache.TryGetValue(player, out playerBehaviour)) return playerBehaviour;
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
            PlayerSTBehaviour playerBehaviour = GetPlayerBehaviour(player);
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

            playerBehaviour.isTargetable = true;
            playerBehaviour.isInGame = false;
            playerBehaviour.hasBeenGassed = false;
            playerBehaviour.sawTape = null;
            ObjectSTManager.DestroyReverseBearTrapForServer(playerBehaviour.playerProperties);
            playerBehaviour.reverseBearTrap = null;
        }
    }
}
