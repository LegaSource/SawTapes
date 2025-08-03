using SawTapes.Behaviours.Enemies;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Billy;

public class BillyPuppet : PhysicsProp
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
}
