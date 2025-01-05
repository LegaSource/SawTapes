using DunGen;
using HarmonyLib;
using SawTapes.Managers;
using UnityEngine;

namespace SawTapes.Patches
{
    internal class TilePatch
    {
        [HarmonyPatch(typeof(Tile), nameof(Tile.OnTriggerEnter))]
        [HarmonyPrefix]
        private static void EnterTile(ref Tile __instance, Collider other)
        {
            TileSTManager.LogTileDebugInfo(__instance, other);
            TileSTManager.HandleTileBehaviour(__instance, other);
        }
    }
}
