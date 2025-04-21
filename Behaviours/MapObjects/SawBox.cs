using GameNetcodeStuff;
using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.Tapes;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.MapObjects;

public class SawBox : NetworkBehaviour
{
    public bool isBombContained = false;
    public double lastInteraction = 0d;

    public InteractTrigger boxTrigger;

    public void Update()
    {
        PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(GameNetworkManager.Instance.localPlayerController);
        if (playerBehaviour == null || !playerBehaviour.isInGame || playerBehaviour.sawTape is not ExplosiveTape) return;

        if (Vector3.Distance(playerBehaviour.playerProperties.transform.position, transform.position) > 20f)
        {
            CustomPassManager.SetupAuraForObjects([gameObject], SawTapes.yellowWallhackShader);
            return;
        }
        CustomPassManager.RemoveAuraFromObjects([gameObject]);
    }

    public void BoxInteraction()
        => BoxInteractionByTape();

    public void BoxInteractionByTape()
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

        SawTape sawTape = SawGameSTManager.GetSawTapeFromPlayer(player);
        if (sawTape == null) return;

        switch (sawTape)
        {
            case ExplosiveTape explosiveTape:
                DoExplosiveTape(player, explosiveTape);
                break;
        }
    }

    public void DoExplosiveTape(PlayerControllerB player, ExplosiveTape explosiveTape)
    {
        SawBomb sawBomb = explosiveTape.sawBomb;
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

        SawKey sawKey = null;
        for (int i = 0; i < player.ItemSlots.Length; i++)
        {
            GrabbableObject grabbableObject = player.ItemSlots[i];
            if (grabbableObject == null || grabbableObject is not SawKey) continue;

            sawKey = grabbableObject as SawKey;
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

        SawBomb sawBomb = networkObject.gameObject.GetComponentInChildren<ExplosiveTape>()?.sawBomb;
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

    public IEnumerator BombContainmentCoroutine(SawBomb sawBomb)
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

        SawBomb sawBomb = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as SawBomb;
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
