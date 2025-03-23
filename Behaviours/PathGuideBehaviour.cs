using DunGen;
using GameNetcodeStuff;
using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.Tapes;
using SawTapes.Managers;
using SawTapes.Values;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace SawTapes.Behaviours
{
    public class PathGuideBehaviour : MonoBehaviour
    {
        public Saw saw;
        public SawTape sawTape;
        public Vector3 startingPosition;
        public NavMeshAgent tempAgent;
        public NavMeshPath path;
        public Doorway[] doorways;
        public HashSet<PlayerControllerB> players = new HashSet<PlayerControllerB>();

        public bool pathFound = false;
        public static int timeOut = 5;
        public float timer = 1f;
        public float maxDistanceFromPath = 10.0f;

        // Particules
        public int minParticleSpacing = 2;
        public int maxParticleSpacing = 5;
        public float particleGenerationInterval = 1.25f;

        // Pièges
        public float hazardSpacing = 10.0f;
        public List<EscapeHazard> eligibleHazards = new List<EscapeHazard>();
        public HashSet<GameObject> hazards = new HashSet<GameObject>();

        public void Start()
        {
            // Créer un NavMeshAgent temporaire pour le calcul de chemin
            startingPosition = transform.position;
            tempAgent = gameObject.AddComponent<NavMeshAgent>();
            tempAgent.enabled = false;
            tempAgent.stoppingDistance = 1f;

            path = new NavMeshPath();
            doorways = FindObjectsOfType<Doorway>();
            ParseHazards();
            StartCoroutine(UpdatePathCoroutine());

            transform.position = startingPosition;
        }

        public void ParseHazards()
        {
            string[] hazardEntries = ConfigManager.escapeHazards.Value.Split(',');
            foreach (string entry in hazardEntries)
            {
                string[] keyValue = entry.Split(':');
                if (keyValue.Length == 6
                    && int.TryParse(keyValue[1], out int weight)
                    && bool.TryParse(keyValue[2], out bool spawnFacingAwayFromWall)
                    && bool.TryParse(keyValue[3], out bool spawnFacingWall)
                    && bool.TryParse(keyValue[4], out bool spawnWithBackToWall)
                    && bool.TryParse(keyValue[5], out bool spawnWithBackFlushAgainstWall))
                {
                    for (int i = 0; i < weight; i++)
                        eligibleHazards.Add(new EscapeHazard(
                            keyValue[0],
                            spawnFacingAwayFromWall,
                            spawnFacingWall,
                            spawnWithBackToWall,
                            spawnWithBackFlushAgainstWall));
                }
            }
            foreach (EscapeHazard escapeHazard in eligibleHazards.ToList())
            {
                GameObject hazard = SearchHazard(escapeHazard);
                if (hazard == null) eligibleHazards.Remove(escapeHazard);
            }
        }

        public IEnumerator UpdatePathCoroutine()
        {
            float timePassed = 0f;
            // Activer temporairement l'agent pour calculer un chemin
            while (!pathFound)
            {
                tempAgent.enabled = true;
                pathFound = tempAgent.CalculatePath(saw.transform.position, path);
                tempAgent.enabled = false;

                if (!pathFound)
                {
                    SawTapes.mls.LogWarning("Retrying path calculation for Saw Game...");
                    yield return new WaitForSeconds(0.5f);
                    timePassed += 0.5f;

                    if (timePassed >= timeOut) break;
                }
            }

            if (path.corners.Length < 2)
                pathFound = false;

            if (pathFound)
            {
                AdjustPathForDoorways();
                UnlockDoorLocks();
                PlaceHazardsAlongPath();
            }
            else
            {
                SendErrPathMessageClientRpc();
                sawTape.EndGameForServer(true);
                SawTapes.mls.LogError("Path could not be generated");
            }
        }

        [ClientRpc]
        public void SendErrPathMessageClientRpc()
        {
            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(GameNetworkManager.Instance.localPlayerController);
            if (playerBehaviour == null || !playerBehaviour.isInGame) return;
            
            HUDManager.Instance.DisplayTip(Constants.INFORMATION, Constants.MESSAGE_INFO_ERR_PATH_ESCAPE);
        }

        public void AdjustPathForDoorways()
        {
            for (int i = 0; i < path.corners.Length; i++)
            {
                foreach (Doorway doorway in doorways)
                {
                    float doorwayRadius = doorway.Socket.Size.magnitude * 0.5f;
                    if (Vector3.Distance(path.corners[i], doorway.transform.position) < doorwayRadius)
                    {
                        path.corners[i] = doorway.transform.position;
                        break;
                    }
                }
            }
        }

        public void UnlockDoorLocks()
        {
            for (int i = 0; i < path.corners.Length; i++)
            {
                foreach (DoorLock doorLock in FindObjectsOfType<DoorLock>())
                {
                    if (Vector3.Distance(path.corners[i], doorLock.transform.position) > 2f) continue;
                    if (!doorLock.isLocked || doorLock.isPickingLock) continue;

                    doorLock.UnlockDoorSyncWithServer();
                    break;
                }
            }
        }

        public void PlaceHazardsAlongPath()
        {
            float distanceSinceLastHazard = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                Vector3 start = path.corners[i - 1];
                Vector3 end = path.corners[i];
                float segmentLength = Vector3.Distance(start, end);

                // Place des pièges à intervalles de trapSpacing le long de chaque segment
                while (distanceSinceLastHazard + segmentLength >= hazardSpacing)
                {
                    // Calcul de la position pour placer le piège
                    float t = (hazardSpacing - distanceSinceLastHazard) / segmentLength;
                    Vector3 hazardPosition = Vector3.Lerp(start, end, t);

                    // Vérifier la proximité avec une doorway
                    foreach (Doorway doorway in doorways)
                    {
                        float doorwayRadius = doorway.Socket.Size.magnitude * 0.5f;
                        if (Vector3.Distance(hazardPosition, doorway.transform.position) < doorwayRadius)
                        {
                            // Décaler le piège d'un mètre le long de la normale du segment
                            hazardPosition += (hazardPosition - doorway.transform.position).normalized * 1f;
                            break;
                        }
                    }

                    // Placement du piège
                    if (Physics.Raycast(hazardPosition + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 4f))
                    {
                        hazardPosition.y = hit.point.y;
                        EscapeHazard escapeHazard = eligibleHazards[new System.Random().Next(eligibleHazards.Count)];
                        GameObject hazard = SearchHazard(escapeHazard);
                        // Positionne le piège légèrement au-dessus du sol pour éviter tout problème d'intersection
                        if (hazard != null) SpawnHazard(hazard, hazardPosition + Vector3.up * 0.5f, escapeHazard);
                    }

                    // Met à jour distanceSinceLastTrap
                    distanceSinceLastHazard = 0f;
                    segmentLength -= (hazardSpacing - distanceSinceLastHazard);
                    start = hazardPosition;
                }

                // Met à jour la distance cumulée si aucun piège n'a été placé sur le segment
                distanceSinceLastHazard += segmentLength;
            }
        }

        public GameObject SearchHazard(EscapeHazard escapeHazard)
        {
            // Parcours de la liste des préfabriqués pour trouver le type de piège demandé
            GameObject hazard = null;
            foreach (NetworkPrefabsList networkPrefabList in NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists ?? Enumerable.Empty<NetworkPrefabsList>())
            {
                foreach (NetworkPrefab networkPrefab in networkPrefabList.PrefabList ?? Enumerable.Empty<NetworkPrefab>())
                {
                    if (!networkPrefab.Prefab.name.Equals(escapeHazard.HazardName)) continue;
                    hazard = networkPrefab.Prefab;
                }
                if (hazard != null) break;
            }
            return hazard;
        }

        public void SpawnHazard(GameObject hazardPrefab, Vector3 position, EscapeHazard escapeHazard)
        {
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 587);

            var nearbyMapObjects = FindObjectsOfType<RandomMapObject>().Where(m => Vector3.Distance(position, m.transform.position) <= 5f).ToList();
            // Recherche de la meilleure position de spawn autour de la position donnée
            Vector3 finalPosition = position;
            if (nearbyMapObjects.Any())
            {
                // Prendre le premier objet proche, et placer le piège à proximité de cet objet
                RandomMapObject nearestMapObject = nearbyMapObjects.First();
                finalPosition = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position, 2f, default(NavMeshHit), random);
            }

            // Ajout de légers ajustements à la position pour placer le piège au sol
            if (Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 80f, 268437760, QueryTriggerInteraction.Ignore))
            {
                position = hit.point;
            }
            else
            {
                SawTapes.mls.LogWarning("No surface detected to place the trap.");
                return;
            }

            // Instancie le piège à la position calculée
            GameObject hazardInstance = Instantiate(hazardPrefab, position, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);

            if (escapeHazard.SpawnFacingAwayFromWall) gameObject.transform.eulerAngles = new Vector3(0f, RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(position + Vector3.up * 0.2f), 0f);
            else if (escapeHazard.SpawnFacingWall) gameObject.transform.eulerAngles = new Vector3(0f, RoundManager.Instance.YRotationThatFacesTheNearestFromPosition(position + Vector3.up * 0.2f), 0f);
            else gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, random.Next(0, 360), gameObject.transform.eulerAngles.z);

            if (escapeHazard.SpawnWithBackToWall && Physics.Raycast(gameObject.transform.position, -gameObject.transform.forward, out var hitInfo, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                gameObject.transform.position = hitInfo.point;
                if (escapeHazard.SpawnWithBackFlushAgainstWall)
                {
                    gameObject.transform.forward = hitInfo.normal;
                    gameObject.transform.eulerAngles = new Vector3(0f, gameObject.transform.eulerAngles.y, 0f);
                }
            }

            hazardInstance.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
            hazards.Add(hazardInstance);
        }

        public void Update()
        {
            if (!pathFound) return;

            float startDistance = Vector3.Distance(startingPosition, path.corners[0]);
            if (startDistance > 10f)
            {
                pathFound = false;
                SawTapes.mls.LogError($"Distance too large between the starting point and the path : {startDistance}");
                sawTape.EndGameForServer(true);
                return;
            }

            timer += Time.deltaTime;
            if (timer >= particleGenerationInterval)
            {
                foreach (PlayerControllerB player in players)
                {
                    if (Vector3.Distance(saw.transform.position, player.transform.position) > ConfigManager.escapeAuraDistance.Value) continue;
                    (sawTape as EscapeTape)?.PlayerEndPathGuideClientRpc((int)player.playerClientId, saw.GetComponent<NetworkObject>());
                }
                GenerateParticlesAlongPath();
                timer = 0f;
            }
        }

        public void GenerateParticlesAlongPath()
        {
            PlayerControllerB player = players.FirstOrDefault(p => !p.isPlayerDead);
            if (player == null) return;

            for (int i = 1; i < path.corners.Length; i++)
            {
                Vector3 start = path.corners[i - 1];
                Vector3 end = path.corners[i];
                float segmentLength = Vector3.Distance(start, end);

                Vector3 direction = (end - start).normalized;

                for (float distance = 0; distance < segmentLength; distance += new System.Random().Next(minParticleSpacing, maxParticleSpacing) )
                {
                    Vector3 position = start + direction * distance;
                    if (Vector3.Distance(position, player.transform.position) > 20f) continue;

                    // Calcul des positions gauche et droite
                    Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized * 1.25f;
                    Vector3 leftPosition = position - perpendicular;
                    Vector3 rightPosition = position + perpendicular;

                    if (players.Contains(GameNetworkManager.Instance.localPlayerController))
                    {
                        SawGameSTManager.SpawnPathParticle(leftPosition);
                        SawGameSTManager.SpawnPathParticle(rightPosition);
                    }
                    (sawTape as EscapeTape)?.SpawnPathParticleClientRpc(leftPosition, rightPosition, players.Select(p => (int)p.playerClientId).ToArray());
                }
            }
        }

        public void OnDestroy()
        {
            if (tempAgent != null) Destroy(tempAgent);

            foreach (GameObject hazard in hazards)
                SawGameSTManager.DespawnHazard(hazard);
        }
    }
}
