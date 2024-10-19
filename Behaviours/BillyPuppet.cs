using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours
{
    internal class BillyPuppet : PhysicsProp
    {
        public Billy billy;
        public AudioSource billyLaugh;

        public override void Start()
        {
            base.Start();
            if (billyLaugh == null)
            {
                billyLaugh = GetComponent<AudioSource>();
            }
            if (billyLaugh == null)
            {
                SawTapes.mls.LogError("billyLaugh is not assigned and could not be found.");
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null)
            {
                BillyLaughServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void BillyLaughServerRpc()
        {
            BillyLaughClientRpc();
        }

        [ClientRpc]
        private void BillyLaughClientRpc()
        {
            billyLaugh.Play();
        }

        public override void GrabItem()
        {
            base.GrabItem();
            EnableItemMeshes(true);
            if (IsServer && billy?.thisNetworkObject && billy.thisNetworkObject.IsSpawned)
            {
                billy.thisNetworkObject.Despawn();
                billy = null;
            }
        }
    }
}
