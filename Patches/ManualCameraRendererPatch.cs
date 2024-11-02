using HarmonyLib;
using SawTapes.Managers;
using UnityEngine;

namespace SawTapes.Patches
{
    internal class ManualCameraRendererPatch
    {
        [HarmonyPatch(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.updateMapTarget))]
        [HarmonyPostfix]
        private static void MapCameraText(ref ManualCameraRenderer __instance)
        {
            MapCameraSTManager.UpdateMapCamera(ref __instance);
        }
    }
}
