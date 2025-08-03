using HarmonyLib;
using LegaFusionCore.Managers;
using LegaFusionCore.Utilities;
using SawTapes.Managers;
using SawTapes.Values;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SawTapes.Patches;

internal class RoundManagerPatch
{
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
    [HarmonyPostfix]
    private static void SpawnItems()
        => AddNewItems();

    public static void AddNewItems()
    {
        List<RandomScrapSpawn> listRandomScrapSpawn = Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => !s.spawnUsed).ToList();
        SpawnTape(listRandomScrapSpawn[new System.Random().Next(0, listRandomScrapSpawn.Count)]);
    }

    public static void SpawnTape(RandomScrapSpawn randomScrapSpawn)
    {
        if (randomScrapSpawn == null) return;

        SawTapeValue sawTapeValue = GetSawTapeValue();
        if (sawTapeValue == null) return;

        if (randomScrapSpawn.spawnedItemsCopyPosition) randomScrapSpawn.spawnUsed = true;
        else randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + (Vector3.up * sawTapeValue.Item.verticalOffset);

        _ = LFCObjectsManager.SpawnObjectForServer(sawTapeValue.Item.spawnPrefab, randomScrapSpawn.transform.position + (Vector3.up * 0.5f));
    }

    public static SawTapeValue GetSawTapeValue()
    {
        SawTapeValue tapeToSpawn = null;

        LFCUtilities.Shuffle(SawTapes.sawTapeValues);
        foreach (SawTapeValue sawTapeValue in SawTapes.sawTapeValues)
        {
            if (!string.IsNullOrEmpty(sawTapeValue.InteriorsExclusion) && sawTapeValue.InteriorsExclusion.Contains(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name)) continue;
            if (sawTapeValue.MinPlayers > StartOfRound.Instance.allPlayerScripts.Count(STUtilities.IsEligiblePlayer)) continue;

            if (tapeToSpawn == null && new System.Random().Next(1, 100) <= sawTapeValue.Rarity)
            {
                tapeToSpawn = sawTapeValue;
                sawTapeValue.Rarity = sawTapeValue.DefaultRarity;
                break;
            }
        }

        // Incrémenter la rareté d'une seule cassette si aucun mini-jeu n'a été sélectionné pour la partie
        if (tapeToSpawn == null)
        {
            SawTapeValue sawTapeValue = SawTapes.sawTapeValues.FirstOrDefault();
            sawTapeValue.Rarity = Mathf.Min(100, sawTapeValue.Rarity + ConfigManager.rarityIncrement.Value);
        }

        return tapeToSpawn;
    }
}
