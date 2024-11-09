using SawTapes.Managers;
using Unity.Netcode;

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
                    StartCoroutine(STUtilities.ShowEnemyCoroutine(playerBehaviour.assignedEnemy));
                    SawTapesNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
                }
            }
        }
    }
}
