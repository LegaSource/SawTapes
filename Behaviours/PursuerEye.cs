using SawTapes.Managers;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours
{
    public class PursuerEye : PhysicsProp
    {
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null)
            {
                PlayerSTBehaviour playerBehaviour = playerHeldBy.GetComponent<PlayerSTBehaviour>();
                if (playerBehaviour.assignedEnemy != null)
                {
                    StartCoroutine(ShowEnemyCoroutine(playerBehaviour.assignedEnemy));
                    SawTapesNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
                }
            }
        }

        public IEnumerator ShowEnemyCoroutine(EnemyAI enemy)
        {
            CustomPassManager.SetupCustomPassForEnemy(enemy);
            yield return new WaitForSeconds(ConfigManager.huntingAura.Value);
            CustomPassManager.RemoveAura();
        }
    }
}
