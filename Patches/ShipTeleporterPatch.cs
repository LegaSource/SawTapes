using HarmonyLib;
using SawTapes.Behaviours;
using SawTapes.Managers;

namespace SawTapes.Patches
{
    internal class ShipTeleporterPatch
    {
        [HarmonyPatch(typeof(ShipTeleporter), nameof(ShipTeleporter.PressTeleportButtonOnLocalClient))]
        [HarmonyPrefix]
        private static bool PreventTeleport(ref ShipTeleporter __instance)
        {
            if (__instance.isInverseTeleporter || StartOfRound.Instance.mapScreen.targetedPlayer == null) return true;

            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(StartOfRound.Instance.mapScreen.targetedPlayer);
            if (playerBehaviour == null || !playerBehaviour.isInGame) return true;

            return false;
        }
    }
}
