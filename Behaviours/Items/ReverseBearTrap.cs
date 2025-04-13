using GameNetcodeStuff;
using SawTapes.Managers;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class ReverseBearTrap : PhysicsProp
{
    public BoxCollider scanNodeCollider;
    public bool isReleased = false;

    public override void Start()
    {
        base.Start();

        scanNodeCollider = GetComponentsInChildren<BoxCollider>().FirstOrDefault(c => c.gameObject.name.Equals("ScanNode"));
        if (scanNodeCollider == null) SawTapes.mls.LogWarning("The scan node collider could not be found for Reverse Bear Trap.");

        SetCarriedState(true);
    }

    public void SetCarriedState(bool isCarried)
        => scanNodeCollider.enabled = !isCarried;

    [ClientRpc]
    public void InitializeReverseBearTrapClientRpc(int playerId)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        grabbable = false;
        grabbableToEnemies = false;
        hasHitGround = false;
        EnablePhysics(enable: false);
        SetScrapValue(ConfigManager.reverseBearTrapValue.Value);

        parentObject = GameNetworkManager.Instance.localPlayerController == player
            ? player.gameplayCamera.transform
            : player.playerGlobalHead.transform;

        PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(player);
        if (playerBehaviour == null) return;

        playerBehaviour.reverseBearTrap = this;
    }

    public override void LateUpdate()
    {
        if (isReleased)
        {
            base.LateUpdate();
            return;
        }

        if (parentObject != null)
        {
            PlayerControllerB player = parentObject.GetComponentInParent<PlayerControllerB>();
            if (player != null)
            {
                transform.rotation = parentObject.rotation;
                transform.position = GameNetworkManager.Instance.localPlayerController == player
                    ? parentObject.TransformPoint(Vector3.down * 0.17f)
                    : parentObject.TransformPoint(Vector3.up * 0.01f);
            }
        }

        if (radarIcon != null) radarIcon.position = transform.position;
    }
}
