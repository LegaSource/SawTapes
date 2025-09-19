using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Registries;
using SawTapes.Managers;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace SawTapes;

public class STUtilities
{
    public static bool IsEligiblePlayer(PlayerControllerB player)
        => player.isPlayerControlled
            && !player.isPlayerDead
            && (SawTapes.bathroom == null || (SawTapes.bathroom != null && SawTapes.bathroom.player != player))
            && (string.IsNullOrEmpty(ConfigManager.excludedPlayers.Value) || !ConfigManager.excludedPlayers.Value.Contains(player.playerUsername));

    public static Transform FindMainEntrancePoint() => LFCSpawnRegistry.GetAllAs<EntranceTeleport>().FirstOrDefault(e => e.entranceId == 0 && !e.isEntranceToBuilding)?.entrancePoint;

    public static Vector3[] GetFurthestPositions(Vector3 position, int? amount = null)
    {
        IOrderedEnumerable<Vector3> allPositions = RoundManager.Instance.insideAINodes
            .Select(n => n.transform.position)
            .OrderByDescending(p => Vector3.Distance(position, p));

        return amount.HasValue ? allPositions.Take(amount.Value).ToArray() : allPositions.ToArray();
    }

    public static Vector3? GetFurthestValidPosition(Vector3 position, NavMeshAgent agent, GameObject[] nodes)
    {
        Vector3? bestPosition = null;
        float maxDistance = -1f;

        foreach (GameObject node in nodes)
        {
            Vector3 nodePosition = node.transform.position;
            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(nodePosition, path)) continue;
            if (Vector3.Distance(path.corners[path.corners.Length - 1], nodePosition) > 1.55f) continue;

            float distance = Vector3.Distance(position, nodePosition);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                bestPosition = nodePosition;
            }
        }

        return bestPosition;
    }

    public static Vector3? GetClosestPositionScrapSpawn(Vector3 position, float minDistance, Item itemToSpawn)
    {
        RandomScrapSpawn randomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
            .Where(s => Vector3.Distance(position, s.transform.position) >= minDistance)
            .OrderBy(s => Vector3.Distance(position, s.transform.position))
            .FirstOrDefault();

        if (!randomScrapSpawn.spawnedItemsCopyPosition) randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + (Vector3.up * itemToSpawn.verticalOffset);
        return randomScrapSpawn.transform.position + (Vector3.up * 0.5f);
    }

    public static Vector3 GetFurthestPositionScrapSpawn(Vector3 position, Item itemToSpawn)
    {
        RandomScrapSpawn randomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
            .Where(s => !s.spawnUsed)
            .OrderByDescending(s => Vector3.Distance(position, s.transform.position))
            .FirstOrDefault();

        if (randomScrapSpawn == null)
        {
            // Au cas où, mieux vaut prendre un spawn déjà utilisé que de le faire apparaître devant le joueur
            randomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
                .OrderByDescending(p => Vector3.Distance(position, p.transform.position))
                .FirstOrDefault();
        }

        if (randomScrapSpawn.spawnedItemsCopyPosition) randomScrapSpawn.spawnUsed = true;
        else randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + (Vector3.up * itemToSpawn.verticalOffset);

        return randomScrapSpawn.transform.position + (Vector3.up * 0.5f);
    }

    public static void ShowAuraAimedEnemy(PlayerControllerB player, string tag, float distance, ref EnemyAI aimedEnemy)
    {
        if (Physics.Raycast(new Ray(player.gameplayCamera.transform.position, player.gameplayCamera.transform.forward), out RaycastHit hit, distance, 524288, QueryTriggerInteraction.Collide))
        {
            EnemyAICollisionDetect enemyCollision = hit.collider.GetComponent<EnemyAICollisionDetect>();
            if (enemyCollision == null || enemyCollision.mainScript == null) return;

            if (aimedEnemy != null && aimedEnemy != enemyCollision.mainScript) RemoveAuraFromEnemy(tag, ref aimedEnemy);
            aimedEnemy = enemyCollision.mainScript;
            CustomPassManager.SetupAuraForObjects([enemyCollision.mainScript.gameObject], LegaFusionCore.LegaFusionCore.transparentShader, tag, Color.red);
            return;
        }
        RemoveAuraFromEnemy(tag, ref aimedEnemy);
    }

    public static void RemoveAuraFromEnemy(string tag, ref EnemyAI aimedEnemy)
    {
        if (aimedEnemy == null) return;

        CustomPassManager.RemoveAuraFromObjects([aimedEnemy.gameObject], tag);
        aimedEnemy = null;
    }

    public static void ShowAuraAimedPlayer(PlayerControllerB player, string tag, float distance, ref PlayerControllerB aimedPlayer)
    {
        RaycastHit[] hits = Physics.RaycastAll(new Ray(player.gameplayCamera.transform.position, player.gameplayCamera.transform.forward), distance);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            PlayerControllerB targetedPlayer = hit.collider.GetComponent<PlayerControllerB>();
            if (targetedPlayer == null || targetedPlayer == player) continue;

            if (aimedPlayer != null && aimedPlayer != targetedPlayer) RemoveAuraFromPlayer(tag, ref aimedPlayer);
            aimedPlayer = targetedPlayer;
            CustomPassManager.SetupAuraForObjects([targetedPlayer.gameObject], LegaFusionCore.LegaFusionCore.transparentShader, tag, Color.yellow);
            return;
        }
        RemoveAuraFromPlayer(tag, ref aimedPlayer);
    }

    public static void RemoveAuraFromPlayer(string tag, ref PlayerControllerB aimedPlayer)
    {
        if (aimedPlayer == null) return;

        CustomPassManager.RemoveAuraFromObjects([aimedPlayer.gameObject], tag);
        aimedPlayer = null;
    }
}
