using GameNetcodeStuff;
using SawTapes.Behaviours.Tapes;
using SawTapes.Managers;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class PursuerEye : PhysicsProp
{
    public EnemyAI aimedEnemy;

    public override void Update()
    {
        base.Update();

        if (!isHeld || isPocketed || playerHeldBy == null) return;
        if (playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;

        SawTape sawTape = SawGameSTManager.GetSawTapeFromPlayer(playerHeldBy);
        if (sawTape == null || sawTape is not SurvivalTape) return;

        ShowAuraTargetedEnemy();
    }

    public void ShowAuraTargetedEnemy()
    {
        if (Physics.Raycast(new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward), out RaycastHit hit, ConfigManager.eyeDistanceSurvival.Value, 524288, QueryTriggerInteraction.Collide))
        {
            EnemyAICollisionDetect enemyCollision = hit.collider.GetComponent<EnemyAICollisionDetect>();
            if (enemyCollision == null || enemyCollision.mainScript == null) return;

            aimedEnemy = enemyCollision.mainScript;
            CustomPassManager.SetupCustomPassForObjects([enemyCollision.mainScript.gameObject]);
            return;
        }
        aimedEnemy = null;
        CustomPassManager.RemoveAura();
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown || playerHeldBy == null) return;

        SawTape sawTape = SawGameSTManager.GetSawTapeFromPlayer(playerHeldBy);
        ItemActivateByTape(sawTape);
    }

    public void ItemActivateByTape(SawTape sawTape)
    {
        switch (sawTape)
        {
            case HuntingTape huntingTape:
                if (huntingTape == null) return;

                huntingTape.ShowAura(ConfigManager.eyeAuraHunting.Value);
                SawTapesNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
                break;
            case SurvivalTape survivalTape:
                if (survivalTape == null || aimedEnemy == null) return;

                PlayerControllerB player = STUtilities.GetFurthestInGamePlayer(playerHeldBy);
                if (player == null || player == playerHeldBy) return;

                survivalTape.TeleportEnemyServerRpc(aimedEnemy.thisNetworkObject, player.transform.position);
                aimedEnemy = null;
                CustomPassManager.RemoveAura();
                SawTapesNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
                break;
        }
    }

    public override void PocketItem()
    {
        base.PocketItem();
        CustomPassManager.RemoveAura();
    }
}
