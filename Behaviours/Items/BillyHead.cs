using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Managers;
using LegaFusionCore.Managers.NetworkManagers;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class BillyHead : PhysicsProp
{
    public BillyBody aimedBody;

    public override void Update()
    {
        base.Update();

        if (!isHeld || isPocketed || playerHeldBy == null) return;
        if (playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;

        ShowAuraTargetedBody();
    }

    public void ShowAuraTargetedBody()
    {
        if (Physics.Raycast(new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward), out RaycastHit hit, 3f, 832, QueryTriggerInteraction.Collide))
        {
            BillyBody billyBody = hit.collider.GetComponent<BillyBody>();
            if (billyBody == null) return;

            if (aimedBody != null && aimedBody != billyBody) RemoveAuraFromBody();
            aimedBody = billyBody;
            CustomPassManager.SetupAuraForObjects([aimedBody.gameObject], LegaFusionCore.LegaFusionCore.transparentShader, $"{SawTapes.modName}BillyPiece", Color.yellow);
            return;
        }
        RemoveAuraFromBody();
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown || playerHeldBy == null || aimedBody == null) return;

        SpawnBillyServerRpc(aimedBody.GetComponent<NetworkObject>());
        RemoveAuraFromBody();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBillyServerRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        GrabbableObject billyBody = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        Vector3 position = billyBody.transform.position + (Vector3.up * 0.5f);

        BillyPuppetJJ billy = LFCObjectsManager.SpawnObjectForServer(SawTapes.billyPuppetJJ.spawnPrefab, position) as BillyPuppetJJ;
        billy.InitializeForServer();

        LFCNetworkManager.Instance.DestroyObjectClientRpc(networkObject);
        LFCNetworkManager.Instance.DestroyObjectClientRpc(GetComponent<NetworkObject>());
    }

    public override void PocketItem()
    {
        base.PocketItem();
        RemoveAuraFromBody();
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
        RemoveAuraFromBody();
    }

    public void RemoveAuraFromBody()
    {
        if (aimedBody == null) return;

        CustomPassManager.RemoveAuraByTag($"{SawTapes.modName}BillyPiece");
        aimedBody = null;
    }
}
