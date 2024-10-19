using GameNetcodeStuff;
using SawTapes.Behaviours;
using UnityEngine;

namespace SawTapes.Managers
{
    internal class PlayerSTManager
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
            if (playerBehaviour != null && playerBehaviour.isInGame)
            {
                player.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Unknown);
                if (player == GameNetworkManager.Instance.localPlayerController)
                {
                    HUDManager.Instance.DisplayTip("Information", "You tried to cheat, the rules were clear");
                }
                return true;
            }
            return false;
        }
    }
}
