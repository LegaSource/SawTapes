using GameNetcodeStuff;
using LegaFusionCore.Managers;
using SawTapes.Behaviours.Enemies;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class BillyPuppet : PhysicsProp, IHittable
{
    public BillyAnnouncement billy;
    public AudioSource billyLaugh;

    public override void Start()
    {
        base.Start();

        if (billyLaugh == null) billyLaugh = GetComponent<AudioSource>();
        if (billyLaugh == null) SawTapes.mls.LogError("billyLaugh is not assigned and could not be found.");
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (buttonDown && playerHeldBy != null)
            BillyLaughServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void BillyLaughServerRpc()
        => BillyLaughClientRpc();

    [ClientRpc]
    private void BillyLaughClientRpc()
        => billyLaugh.Play();

    public override void GrabItem()
    {
        base.GrabItem();

        EnableItemMeshes(true);
        if ((IsHost || IsServer) && billy?.thisNetworkObject != null && billy.thisNetworkObject.IsSpawned)
        {
            billy.thisNetworkObject.Despawn();
            billy = null;
        }
    }

    public bool Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
    {
        SpawnBillyPieceServerRpc();
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBillyPieceServerRpc()
    {
        List<GameObject> billyPieces = [SawTapes.billyHead.spawnPrefab, SawTapes.billyBody.spawnPrefab];
        GrabbableObject billyPiece = LFCObjectsManager.SpawnObjectForServer(billyPieces[new System.Random().Next(billyPieces.Count)], transform.position);
        InitializeBillyPieceClientRpc(billyPiece.GetComponent<NetworkObject>(), Random.Range(20, 50));
    }

    [ClientRpc]
    public void InitializeBillyPieceClientRpc(NetworkObjectReference obj, int value)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        GrabbableObject billyPiece = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        billyPiece.SetScrapValue(value);
        DestroyObjectInHand(playerHeldBy);
    }
}
