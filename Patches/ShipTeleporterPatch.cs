using HarmonyLib;
using SawTapes.Behaviours;

namespace SawTapes.Patches
{
    internal class ShipTeleporterPatch
    {
        [HarmonyPatch(typeof(ShipTeleporter), nameof(ShipTeleporter.PressTeleportButtonOnLocalClient))]
        [HarmonyPrefix]
        private static bool PreventTeleport(ref ShipTeleporter __instance)
        {
            if (!__instance.isInverseTeleporter && StartOfRound.Instance.mapScreen.targetedPlayer != null)
            {
                PlayerSTBehaviour playerBehaviour = StartOfRound.Instance.mapScreen.targetedPlayer.GetComponent<PlayerSTBehaviour>();
                if (playerBehaviour != null && playerBehaviour.isInGame)
                    return false;
            }
            return true;
        }
    }
}
