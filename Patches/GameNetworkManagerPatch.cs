using HarmonyLib;
using SawTapes.Files;
using System.Linq;
using UnityEngine;

namespace SawTapes.Patches
{
    internal class GameNetworkManagerPatch
    {
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
        [HarmonyPostfix]
        private static void StartGameNetworkManager()
        {
            foreach (EnemyAI enemyAI in Resources.FindObjectsOfTypeAll<EnemyAI>().Distinct())
            {
                SawTapes.allEnemies.Add(enemyAI);
            }

            SurvivalGameFile.LoadJSON();
            SubtitleFile.LoadJSON();
        }
    }
}
