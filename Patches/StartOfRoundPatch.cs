using GameNetcodeStuff;
using HarmonyLib;
using SawTapes.Files;
using SawTapes.Managers;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Patches;

internal class StartOfRoundPatch
{
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
    [HarmonyBefore(["evaisa.lethallib"])]
    [HarmonyPostfix]
    private static void StartRound(ref StartOfRound __instance)
    {
        if (NetworkManager.Singleton.IsHost && SawTapesNetworkManager.Instance == null)
        {
            GameObject gameObject = Object.Instantiate(SawTapes.managerPrefab, __instance.transform.parent);
            gameObject.GetComponent<NetworkObject>().Spawn();
            SawTapes.mls.LogInfo("Spawning SawTapesNetworkManager");
        }

        AffectEnemiesForSawGames();
        SubtitleFile.LoadJSON();
    }

    public static void AffectEnemiesForSawGames()
    {
        SawTapes.allEnemies.Clear();
        foreach (EnemyType enemyType in Resources.FindObjectsOfTypeAll<EnemyType>().Distinct())
        {
            if (enemyType == null || enemyType.enemyPrefab == null) continue;
            if (!(enemyType.enemyPrefab.TryGetComponent<EnemyAI>(out EnemyAI enemyAI) && enemyAI != null)) continue;

            _ = SawTapes.allEnemies.Add(enemyType);
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnDisable))]
    [HarmonyPostfix]
    public static void OnDisable()
        => SawTapesNetworkManager.Instance = null;

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerConnectedClientRpc))]
    [HarmonyPostfix]
    private static void PlayerConnection(ref StartOfRound __instance)
    {
        foreach (PlayerControllerB player in __instance.allPlayerScripts)
            PlayerSTManager.AddPlayerBehaviour(player);
    }
}
