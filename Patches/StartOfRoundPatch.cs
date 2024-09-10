using GameNetcodeStuff;
using HarmonyLib;
using SawTapes.Behaviours;
using SawTapes.Managers;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Patches
{
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
        [HarmonyBefore(["evaisa.lethallib"])]
        [HarmonyPostfix]
        private static void StartRound(ref StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (SawTapesNetworkManager.Instance == null)
                {
                    GameObject gameObject = Object.Instantiate(SawTapes.managerPrefab, __instance.transform.parent);
                    gameObject.GetComponent<NetworkObject>().Spawn();
                    SawTapes.mls.LogInfo("Spawning SawTapesNetworkManager");
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnDisable))]
        [HarmonyPostfix]
        public static void OnDisable()
        {
            SawTapesNetworkManager.Instance = null;
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerConnectedClientRpc))]
        [HarmonyPostfix]
        private static void PlayerConnection(ref StartOfRound __instance)
        {
            foreach (PlayerControllerB player in __instance.allPlayerScripts)
            {
                if (player.isPlayerControlled && player.GetComponent<PlayerSTBehaviour>() == null)
                {
                    PlayerSTBehaviour playerBehaviour = player.gameObject.AddComponent<PlayerSTBehaviour>();
                    playerBehaviour.playerProperties = player;
                }
            }
        }
    }
}
