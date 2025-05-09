﻿using GameNetcodeStuff;
using SawTapes.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace SawTapes;

public class STUtilities
{
    public static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
        }
    }

    public static Transform FindMainEntrancePoint()
        => Object.FindObjectsOfType<EntranceTeleport>().FirstOrDefault(e => e.entranceId == 0 && !e.isEntranceToBuilding)?.entrancePoint;

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
        RandomScrapSpawn randomScrapSpawn = Object.FindObjectsOfType<RandomScrapSpawn>()
            .Where(s => Vector3.Distance(position, s.transform.position) >= minDistance)
            .OrderBy(s => Vector3.Distance(position, s.transform.position))
            .FirstOrDefault();

        if (!randomScrapSpawn.spawnedItemsCopyPosition) randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + (Vector3.up * itemToSpawn.verticalOffset);
        return randomScrapSpawn.transform.position + (Vector3.up * 0.5f);
    }

    public static Vector3 GetFurthestPositionScrapSpawn(Vector3 position, Item itemToSpawn)
    {
        RandomScrapSpawn randomScrapSpawn = Object.FindObjectsOfType<RandomScrapSpawn>()
            .Where(s => !s.spawnUsed)
            .OrderByDescending(s => Vector3.Distance(position, s.transform.position))
            .FirstOrDefault();

        if (randomScrapSpawn == null)
        {
            // Au cas où, mieux vaut prendre un spawn déjà utilisé que de le faire apparaître devant le joueur
            randomScrapSpawn = Object.FindObjectsOfType<RandomScrapSpawn>()
                .OrderByDescending(p => Vector3.Distance(position, p.transform.position))
                .FirstOrDefault();
        }

        if (randomScrapSpawn.spawnedItemsCopyPosition) randomScrapSpawn.spawnUsed = true;
        else randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + (Vector3.up * itemToSpawn.verticalOffset);

        return randomScrapSpawn.transform.position + (Vector3.up * 0.5f);
    }

    public static PlayerControllerB GetFurthestInGamePlayer(PlayerControllerB player)
        => StartOfRound.Instance.allPlayerScripts
            .Where(p => p != player && p.isPlayerControlled && !p.isPlayerDead && PlayerSTManager.GetPlayerBehaviour(p) is { } playerBehaviour && playerBehaviour.isInGame)
            .OrderByDescending(p => Vector3.Distance(player.transform.position, p.transform.position))
            .FirstOrDefault();
}
