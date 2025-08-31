using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Addons;
using SawTapes.Managers;
using UnityEngine;

namespace SawTapes.Behaviours.Items.Addons;

public class FinalDetonation : AddonComponent
{
    public override void ActivateAddonAbility()
    {
        if (onCooldown) return;

        PlayerControllerB player = GetComponentInParent<GrabbableObject>()?.playerHeldBy;
        if (player == null) return;

        foreach (RaycastHit hit in Physics.RaycastAll(player.gameplayCamera.transform.position, player.gameplayCamera.transform.forward, 10f, 524288, QueryTriggerInteraction.Collide))
        {
            EnemyAICollisionDetect enemyCollision = hit.collider.GetComponent<EnemyAICollisionDetect>();
            if (enemyCollision == null || enemyCollision.mainScript.isEnemyDead || !player.HasLineOfSightToPosition(enemyCollision.transform.position, 70f, 15)) continue;

            EnemyType enemyType = enemyCollision.mainScript.enemyType;
            if (enemyType == null || ConfigManager.finalDetonationEnemiesExclusions.Value.Contains(enemyType.enemyName)) continue;

            StartCooldown(ConfigManager.finalDetonationCooldown.Value);
            SawTapesNetworkManager.Instance.SpawnFinalDetonationServerRpc(enemyCollision.mainScript.thisNetworkObject, (int)player.playerClientId);
            break;
        }
    }
}
