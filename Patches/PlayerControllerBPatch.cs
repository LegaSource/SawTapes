using GameNetcodeStuff;
using HarmonyLib;
using SawTapes.Behaviours;

namespace SawTapes.Patches
{
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        [HarmonyPostfix]
        private static void StartPlayerControllerB(ref PlayerControllerB __instance)
        {
            if (__instance.isPlayerControlled && __instance.GetComponent<PlayerSTBehaviour>() == null)
            {
                PlayerSTBehaviour playerBehaviour = __instance.gameObject.AddComponent<PlayerSTBehaviour>();
                playerBehaviour.playerProperties = __instance;
            }
        }
    }
}
