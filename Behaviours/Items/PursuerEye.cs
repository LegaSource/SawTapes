using SawTapes.Managers;
using Unity.Netcode;

namespace SawTapes.Behaviours.Items
{
    public class PursuerEye : PhysicsProp
    {
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown && playerHeldBy != null)
            {
                EnemyAI assignedEnemy = playerHeldBy.GetComponent<PlayerSTBehaviour>().huntingTape?.assignedEnemy;
                if (assignedEnemy != null)
                {
                    StartCoroutine(STUtilities.ShowEnemyCoroutine(assignedEnemy));
                    SawTapesNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
                }
            }
        }
    }
}
