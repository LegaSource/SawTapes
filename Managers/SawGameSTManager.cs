using GameNetcodeStuff;
using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.Tapes;
using SawTapes.Patches;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers;

public class SawGameSTManager
{
    public static SawTape GetSawTapeFromPlayer(PlayerControllerB player)
        => PlayerSTManager.GetPlayerBehaviour(player)?.sawTape;

    public static IEnumerator ShowAuraForHuntCoroutine(EnemyAI[] enemies, float duration)
    {
        GameObject[] enemiesObjects = enemies.Select(e => e.gameObject).ToArray();
        if (enemiesObjects.Length > 0) CustomPassManager.SetupAuraForObjects(enemiesObjects, SawTapes.redWallhackShader);

        GameObject[] objects = Resources.FindObjectsOfTypeAll<SawKey>().Where(s => s != null && s.IsSpawned).Select(s => s.gameObject).ToArray();
        if (objects.Length > 0) CustomPassManager.SetupAuraForObjects(objects, SawTapes.yellowWallhackShader);

        yield return new WaitForSeconds(duration);

        if (enemiesObjects.Length > 0) CustomPassManager.RemoveAuraFromObjects(enemiesObjects);
        if (objects.Length > 0) CustomPassManager.RemoveAuraFromObjects(objects);
    }

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
        return item == null ? null : RoundManagerPatch.SpawnItem(item, position + (Vector3.up * 0.5f));
    }

    public static void SpawnPathParticle(Vector3 position)
    {
        if (Physics.Raycast(position + (Vector3.up * 2f), Vector3.down, out RaycastHit hit, 10f)) position = hit.point;

        GameObject spawnObject = Object.Instantiate(SawTapes.pathParticle, position, Quaternion.Euler(-90f, 0f, 0f));
        ParticleSystem pathParticle = spawnObject.GetComponent<ParticleSystem>();
        Object.Destroy(spawnObject, pathParticle.main.duration + pathParticle.main.startLifetime.constantMax);
    }

    public static GameObject SpawnHazard(GameObject hazardPrefab, Vector3 position, bool spawnFacingAwayFromWall, bool spawnFacingWall, bool spawnWithBackToWall, bool spawnWithBackFlushAgainstWall)
    {
        System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 587);

        List<RandomMapObject> nearbyMapObjects = Object.FindObjectsOfType<RandomMapObject>().Where(m => Vector3.Distance(position, m.transform.position) <= 5f).ToList();
        // Recherche de la meilleure position de spawn autour de la position donnée
        if (nearbyMapObjects.Any())
        {
            // Prendre le premier objet proche, et placer la box à proximité de cet objet
            RandomMapObject nearestMapObject = nearbyMapObjects.First();
            position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(nearestMapObject.transform.position, 2f, default, random);
        }

        // Ajout de légers ajustements à la position pour placer le piège au sol
        if (Physics.Raycast(position + (Vector3.up * 2f), Vector3.down, out RaycastHit hit, 80f, 268437760, QueryTriggerInteraction.Ignore))
        {
            position = hit.point;
        }
        else
        {
            SawTapes.mls.LogWarning("No surface detected to place the trap.");
            return null;
        }

        // Instancie le piège à la position calculée
        GameObject hazardInstance = Object.Instantiate(hazardPrefab, position, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);
        hazardInstance.transform.eulerAngles = spawnFacingAwayFromWall
            ? new Vector3(0f, RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(position + (Vector3.up * 0.2f)), 0f)
        : spawnFacingWall
                ? new Vector3(0f, RoundManager.Instance.YRotationThatFacesTheNearestFromPosition(position + (Vector3.up * 0.2f)), 0f)
                : new Vector3(hazardInstance.transform.eulerAngles.x, random.Next(0, 360), hazardInstance.transform.eulerAngles.z);

        if (spawnWithBackToWall && Physics.Raycast(hazardInstance.transform.position, -hazardInstance.transform.forward, out RaycastHit hitInfo, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
        {
            hazardInstance.transform.position = hitInfo.point;
            if (spawnWithBackFlushAgainstWall)
            {
                hazardInstance.transform.forward = hitInfo.normal;
                hazardInstance.transform.eulerAngles = new Vector3(0f, hazardInstance.transform.eulerAngles.y, 0f);
            }
        }

        hazardInstance.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
        return hazardInstance;
    }

    public static void DespawnHazard(GameObject hazard)
    {
        if (hazard == null || hazard.transform == null) return;

        SawTapesNetworkManager.Instance.PlayDespawnParticleClientRpc(hazard.transform.position);
        hazard.GetComponent<NetworkObject>().Despawn(destroy: true);
    }
}
