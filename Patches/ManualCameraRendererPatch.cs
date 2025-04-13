using HarmonyLib;
using SawTapes.Managers;

namespace SawTapes.Patches;

internal class ManualCameraRendererPatch
{
    [HarmonyPatch(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.updateMapTarget))]
    [HarmonyPostfix]
    private static void MapCameraText(ref ManualCameraRenderer __instance)
        => MapCameraSTManager.UpdateMapCamera(__instance);
}
