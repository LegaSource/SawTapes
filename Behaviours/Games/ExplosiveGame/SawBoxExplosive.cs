using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Shaders;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.ExplosiveGame;

public class SawBoxExplosive : NetworkBehaviour
{
    public bool isBombContained = false;
    public double lastInteraction = 0d;

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

        if (!isBombContained)
        {
            bool hasSawBomb = false;

            for (int i = 0; i < player.ItemSlots.Length; i++)
            {
                GrabbableObject grabbableObject = player.ItemSlots[i];
                if (grabbableObject == null || grabbableObject != sawBomb) continue;
                hasSawBomb = true;
            }

            if (hasSawBomb) BombContainmentServerRpc((int)player.playerClientId, explosiveTape.GetComponent<NetworkObject>());
            return;
        }

        double elapsedTime = NetworkManager.ServerTime.Time - lastInteraction;
        if (elapsedTime <= 20d)
        {
            EndGameServerRpc(sawBomb.GetComponent<NetworkObject>(), true);
            return;
        }

        SawKeyExplosive sawKey = null;
        for (int i = 0; i < player.ItemSlots.Length; i++)
        {
            GrabbableObject grabbableObject = player.ItemSlots[i];
            if (grabbableObject == null || grabbableObject is not SawKeyExplosive) continue;

            sawKey = grabbableObject as SawKeyExplosive;
        }

        if (sawKey != null)
        {
            EndGameServerRpc(sawBomb.GetComponent<NetworkObject>(), false);
            return;
        }

        HUDManager.Instance.DisplayTip(Constants.INFORMATION, Constants.MESSAGE_INFO_IMP_END_EXPLOSIVE);
    }

    [ServerRpc(RequireOwnership = false)]
    public void BombContainmentServerRpc(int playerId, NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        ExplosiveTape explosiveTape = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as ExplosiveTape;
        if (explosiveTape == null) return;

        explosiveTape.PrepareDefusing(StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>());
        BombContainmentClientRpc(obj);
    }

    [ClientRpc]
    public void BombContainmentClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        SawBombExplosive sawBomb = networkObject.gameObject.GetComponentInChildren<ExplosiveTape>()?.sawBomb;
        if (sawBomb == null) return;

        if (sawBomb.tickingCoroutine != null)
        {
            sawBomb.StopCoroutine(sawBomb.tickingCoroutine);
            sawBomb.tickingCoroutine = null;
        }

        sawBomb.bombAudio.Stop();
        _ = StartCoroutine(BombContainmentCoroutine(sawBomb));

        HUDManagerPatch.remainedTime += ConfigManager.explosiveExtraDuration.Value;

        isBombContained = true;
        lastInteraction = NetworkManager.ServerTime.Time;
        HUDManager.Instance.DisplayTip(Constants.INFORMATION, Constants.MESSAGE_INFO_FIND_KEY_EXPLOSIVE);
    }

    public IEnumerator BombContainmentCoroutine(SawBombExplosive sawBomb)
    {
        sawBomb.isContained = true;

        PlayerControllerB player = sawBomb.playerHeldBy;
        if (player != null && player == GameNetworkManager.Instance.localPlayerController) player.DropAllHeldItemsAndSync();

        yield return new WaitForSeconds(0.2f);

        sawBomb.EnableItemMeshes(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndGameServerRpc(NetworkObjectReference obj, bool destroy)
        => EndGameClientRpc(obj, destroy);

    [ClientRpc]
    public void EndGameClientRpc(NetworkObjectReference obj, bool destroy)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        SawBombExplosive sawBomb = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as SawBombExplosive;
        if (sawBomb == null) return;

        sawBomb.hasBeenUsedForExplosiveGame = true;

        if (destroy)
        {
            sawBomb.DestroyObjectInHand(sawBomb.playerHeldBy);
            return;
        }
        sawBomb.isContained = false;
        sawBomb.hasBeenDefused = true;
        sawBomb.EnableItemMeshes(true);
    }
}
