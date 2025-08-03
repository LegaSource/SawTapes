using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Addons;
using SawTapes.Behaviours.Items.Addons.Scripts;
using UnityEngine;

namespace SawTapes.Behaviours.Items.Addons;

public class BleedingChains : AddonComponent
{
    public override void ActivateAddonAbility()
    {
        if (onCooldown) return;

        PlayerControllerB player = GetComponentInParent<GrabbableObject>()?.playerHeldBy;
        if (player == null) return;

        if (Physics.Raycast(player.gameplayCamera.transform.position, player.gameplayCamera.transform.forward, out RaycastHit hit, 10f, 524288, QueryTriggerInteraction.Collide))
        {
            EnemyAICollisionDetect enemyCollision = hit.collider.GetComponent<EnemyAICollisionDetect>();
            if (enemyCollision == null) return;

            StartCooldown(10);
            Vector3 position = enemyCollision.mainScript.GetComponentInChildren<SkinnedMeshRenderer>()?.bounds.center ?? enemyCollision.mainScript.transform.position;
            GameObject bleedingChainsObj = Instantiate(SawTapes.bleedingChainsObj, position, enemyCollision.mainScript.transform.rotation, enemyCollision.mainScript.transform);
            BCScript bCScript = bleedingChainsObj.GetComponent<BCScript>();
            bCScript.enemy = enemyCollision.mainScript;
        }
    }
}
