using LegaFusionCore.Managers;
using SawTapes.Behaviours.Items;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.EscapeGame;

public class SawEscape : PhysicsProp
{
    public override void GrabItem()
    {
        if (playerHeldBy == null || playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;
        if (SawTapes.sawTape == null || SawTapes.sawTape is not EscapeTape escapeTape) return;
        if (escapeTape.players.Contains(playerHeldBy)) return;

        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_IMPAC_SAW);
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        if (!buttonDown || playerHeldBy == null) return;
        if (SawTapes.sawTape == null || SawTapes.sawTape is not EscapeTape) return;

        ActivateForEscapeGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateForEscapeGameServerRpc()
    {
        Vector3 position = playerHeldBy.gameplayCamera.transform.position + playerHeldBy.gameplayCamera.transform.forward;
        SawBC saw = LFCObjectsManager.SpawnObjectForServer(SawTapes.sawEscape.spawnPrefab, position) as SawBC;
        saw.InitializeSawForServer();

        ActivateForEscapeGameClientRpc();
    }

    [ClientRpc]
    public void ActivateForEscapeGameClientRpc()
    {
        DestroyObjectInHand(playerHeldBy);

        if (SawTapes.sawTape == null || SawTapes.sawTape is not EscapeTape escapeTape) return;
        escapeTape.sawHasBeenUsed = true;
    }
}
