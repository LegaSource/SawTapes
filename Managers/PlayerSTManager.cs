using GameNetcodeStuff;
using SawTapes.Behaviours;
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

        public static bool PreventTeleportPlayer(ref PlayerControllerB player)
        {
            PlayerSTBehaviour playerBehaviour = player.GetComponent<PlayerSTBehaviour>();
            if (playerBehaviour != null && playerBehaviour.isInGame && (playerBehaviour.tileGame != null || playerBehaviour.huntingTape?.assignedEnemy != null))
            {
                player.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Unknown);
                if (player == GameNetworkManager.Instance.localPlayerController)
                {
                    HUDManager.Instance.DisplayTip(Constants.INFORMATION, Constants.MESSAGE_INFO_CHEAT);
                }
                return true;
            }
            return false;
        }

        public static void ResetPlayerGame(ref PlayerSTBehaviour playerBehaviour)
        {
            TileSTBehaviour tileSTBehaviour = playerBehaviour.tileGame?.GetComponent<TileSTBehaviour>();
            TileSTManager.UnlockDoors(ref tileSTBehaviour);
            playerBehaviour.campTime = 0;
            playerBehaviour.isInGame = false;
            playerBehaviour.tileGame = null;
            playerBehaviour.huntingTape = null;
        }
    }
}
