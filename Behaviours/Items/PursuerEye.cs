using SawTapes.Behaviours.Tapes;
using SawTapes.Managers;
using System.Collections.Generic;
using Unity.Netcode;

namespace SawTapes.Behaviours.Items
{
    public class PursuerEye : PhysicsProp
    {
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (!buttonDown || playerHeldBy == null) return;

            HuntingTape huntingTape = PlayerSTManager.GetPlayerBehaviour(playerHeldBy)?.sawTape as HuntingTape;
            if (huntingTape == null) return;

            List<EnemyAI> enemies = new List<EnemyAI>();
            foreach (NetworkObject spawnedEnemy in huntingTape.spawnedEnemies)
            {
                if (spawnedEnemy == null || !spawnedEnemy.IsSpawned) continue;
                enemies.Add(spawnedEnemy.GetComponentInChildren<EnemyAI>());
            }
            if (enemies.Count == 0) return;

            STUtilities.ShowAura(enemies);
            SawTapesNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
        }
    }
}
