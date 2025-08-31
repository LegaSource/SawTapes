using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Managers;
using SawTapes.Behaviours.Items;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.ExplosiveGame;

public class SawBoxExplosive : NetworkBehaviour
{
    public InteractTrigger boxTrigger;

    public void Update()
    {
        if (SawTapes.sawTape == null || SawTapes.sawTape is not ExplosiveTape) return;

        if (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, transform.position) > 20f)
        {
            CustomPassManager.SetupAuraForObjects([gameObject], LegaFusionCore.LegaFusionCore.wallhackShader, SawTapes.modName, Color.yellow);
            return;
        }
        CustomPassManager.RemoveAuraFromObjects([gameObject], SawTapes.modName);
    }

    public void BoxInteraction()
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (SawTapes.sawTape == null || SawTapes.sawTape is not ExplosiveTape explosiveTape) return;

        SawBombExplosive sawBomb = explosiveTape.sawBomb;
        if (sawBomb == null) return;

        for (int i = 0; i < player.ItemSlots.Length; i++)
        {
            GrabbableObject grabbableObject = player.ItemSlots[i];
            if (grabbableObject == null || grabbableObject != sawBomb) continue;

            EndGameServerRpc(sawBomb.GetComponent<NetworkObject>());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndGameServerRpc(NetworkObjectReference obj)
    {
        EndGameClientRpc(obj);

        BillyPuppetFD billy = LFCObjectsManager.SpawnObjectForServer(SawTapes.billyPuppetFD.spawnPrefab, transform.position + (Vector3.up * 0.5f)) as BillyPuppetFD;
        billy.InitializeForServer();
    }

    [ClientRpc]
    public void EndGameClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        SawBombExplosive sawBomb = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as SawBombExplosive;
        if (sawBomb == null) return;

        sawBomb.hasBeenDestroyed = true;
        sawBomb.DestroyObjectInHand(sawBomb.playerHeldBy);
        sawBomb.bombAudio.Stop();

        if (sawBomb.tickingCoroutine != null)
        {
            sawBomb.StopCoroutine(sawBomb.tickingCoroutine);
            sawBomb.tickingCoroutine = null;
        }
    }
}
