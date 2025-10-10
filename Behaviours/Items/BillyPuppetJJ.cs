using LegaFusionCore.Utilities;
using SawTapes.Behaviours.Items.Addons;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class BillyPuppetJJ : PhysicsProp
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
        LFCUtilities.SetAddonComponent<JigsawJudgement>(this, Constants.JIGSAW_JUDGEMENT_NAME, isPassive: true);
    }
}
