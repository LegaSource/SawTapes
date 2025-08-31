using HarmonyLib;
using LegaFusionCore.Registries;
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
            if (!(enemyType.enemyPrefab.TryGetComponent(out EnemyAI enemyAI) && enemyAI != null)) continue;

            _ = SawTapes.allEnemies.Add(enemyType);
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ShipLeave))]
    [HarmonyPostfix]
    public static void EndRound()
        => LFCStatRegistry.RemoveModifier(LegaFusionCore.Constants.STAT_SPEED, $"{SawTapes.modName}JigsawJudgement");

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnDisable))]
    [HarmonyPostfix]
    public static void OnDisable()
        => SawTapesNetworkManager.Instance = null;
}
