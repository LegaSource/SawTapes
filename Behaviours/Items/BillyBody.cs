using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Managers;
using LegaFusionCore.Managers.NetworkManagers;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class BillyBody : PhysicsProp
{
    public BillyHead aimedHead;

    public override void Update()
    {
        base.Update();

        if (!isHeld || isPocketed || playerHeldBy == null) return;
        if (playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;

        ShowAuraTargetedHead();
    }

    public void ShowAuraTargetedHead()
    {
        if (Physics.Raycast(new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward), out RaycastHit hit, 3f, 832, QueryTriggerInteraction.Collide))
        {
            BillyHead billyHead = hit.collider.GetComponent<BillyHead>();
            if (billyHead == null) return;

            if (aimedHead != null && aimedHead != billyHead) RemoveAuraFromHead();
            aimedHead = billyHead;
            CustomPassManager.SetupAuraForObjects([aimedHead.gameObject], LegaFusionCore.LegaFusionCore.transparentShader, $"{SawTapes.modName}BillyPiece", Color.yellow);
            return;
        }
        RemoveAuraFromHead();
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown || playerHeldBy == null || aimedHead == null) return;

        SpawnBillyServerRpc(aimedHead.GetComponent<NetworkObject>());
        RemoveAuraFromHead();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBillyServerRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        GrabbableObject billyHead = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        Vector3 position = billyHead.transform.position + (Vector3.up * 0.5f);

        BillyPuppetJJ billy = LFCObjectsManager.SpawnObjectForServer(SawTapes.billyPuppetJJ.spawnPrefab, position) as BillyPuppetJJ;
        billy.InitializeForServer();

        LFCNetworkManager.Instance.DestroyObjectClientRpc(networkObject);
        LFCNetworkManager.Instance.DestroyObjectClientRpc(GetComponent<NetworkObject>());
    }

    public override void PocketItem()
    {
        base.PocketItem();
        RemoveAuraFromHead();
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
        RemoveAuraFromHead();
    }

    public void RemoveAuraFromHead()
    {
        if (aimedHead == null) return;

        CustomPassManager.RemoveAuraByTag($"{SawTapes.modName}BillyPiece");
        aimedHead = null;
    }
}
