using LegaFusionCore.Utilities;
using SawTapes.Behaviours.Items.Addons;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class BillyPuppetFD : PhysicsProp
{
    public void InitializeForServer()
    {
        int value = Random.Range(20, 50);
        InitializeEveryoneRpc(value);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void InitializeEveryoneRpc(int value)
    {
        SetScrapValue(value);
        LFCUtilities.SetAddonComponent<FinalDetonation>(this, Constants.FINAL_DETONATION);
    }
}
