using GameNetcodeStuff;
using SawTapes.Behaviours;
using SawTapes.Behaviours.Tapes;
using SawTapes.Patches;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    public class SawGameSTManager
    {
        public static SawTape GetSawTapeFromPlayer(PlayerControllerB player)
            => PlayerSTManager.GetPlayerBehaviour(player)?.sawTape;

        public static GrabbableObject SpawnItemFromNameForServer(string name, Vector3 position)
        {
            GameObject item = null;
            foreach (NetworkPrefabsList networkPrefabList in NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists ?? Enumerable.Empty<NetworkPrefabsList>())
            {
                foreach (NetworkPrefab networkPrefab in networkPrefabList.PrefabList ?? Enumerable.Empty<NetworkPrefab>())
                {
                    GrabbableObject grabbableObject = networkPrefab.Prefab.GetComponent<GrabbableObject>();
                    if (grabbableObject == null || grabbableObject.itemProperties == null) continue;
                    if (!grabbableObject.itemProperties.itemName.Equals(name)) continue;

                    item = networkPrefab.Prefab;
                    if (item != null) break;
                }
            }
            if (item == null) return null;
            return RoundManagerPatch.SpawnItem(item, position + Vector3.up * 0.5f);
        }

        public static void SpawnPathParticle(Vector3 position)
        {
            if (Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f)) position = hit.point;

            GameObject spawnObject = Object.Instantiate(SawTapes.pathParticle, position, Quaternion.Euler(-90f, 0f, 0f));
            ParticleSystem pathParticle = spawnObject.GetComponent<ParticleSystem>();
            Object.Destroy(spawnObject, pathParticle.main.duration + pathParticle.main.startLifetime.constantMax);
        }

        public static void DespawnHazard(GameObject hazard)
        {
            if (hazard == null || hazard.transform == null) return;

            SawTapesNetworkManager.Instance.PlayDespawnParticleClientRpc(hazard.transform.position);
            hazard.GetComponent<NetworkObject>().Despawn(destroy: true);
        }
    }
}
