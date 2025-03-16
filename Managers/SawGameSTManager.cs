using SawTapes.Behaviours;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    public class SawGameSTManager
    {
        public static void SpawnPathParticle(Vector3 position)
        {
            if (Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f)) position = hit.point;

            GameObject spawnObject = Object.Instantiate(SawTapes.pathParticle, position, Quaternion.Euler(-90f, 0f, 0f));
            ParticleSystem pathParticle = spawnObject.GetComponent<ParticleSystem>();
            Object.Destroy(spawnObject, pathParticle.main.duration + pathParticle.main.startLifetime.constantMax);
        }

        public static void PlayerEndPathGuide(PlayerSTBehaviour playerBehaviour)
        {
            if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController
                && playerBehaviour.currentControlTipState != (int)PlayerSTBehaviour.ControlTip.SAW_ITEM)
            {
                playerBehaviour.currentControlTipState = (int)PlayerSTBehaviour.ControlTip.SAW_ITEM;
                HUDManager.Instance.ChangeControlTip(0, "Attract Saw : [Q]", clearAllOther: true);
            }
        }

        public static void DespawnHazard(GameObject hazard)
        {
            if (hazard == null || hazard.transform == null) return;

            SawTapesNetworkManager.Instance.PlayDespawnParticleClientRpc(hazard.transform.position);
            hazard.GetComponent<NetworkObject>().Despawn(destroy: true);
        }
    }
}
