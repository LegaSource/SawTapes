﻿using SawTapes.Behaviours.Items;
using SawTapes.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SawTapes
{
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

        public static IEnumerator ShowEnemyCoroutine(EnemyAI enemy)
        {
            if (enemy == null || enemy.isEnemyDead)
            {
                SawKey sawKey = Object.FindFirstObjectByType<SawKey>();
                if (sawKey != null)
                    CustomPassManager.SetupCustomPassForObject(sawKey);
            }
            else
            {
                CustomPassManager.SetupCustomPassForEnemy(enemy);
            }

            yield return new WaitForSeconds(ConfigManager.huntingAura.Value);

            CustomPassManager.RemoveAura();
        }

        public static Transform FindMainEntrancePoint()
            => Object.FindObjectsOfType<EntranceTeleport>().FirstOrDefault(e => e.entranceId == 0 && !e.isEntranceToBuilding).entrancePoint;

        public static Vector3 GetFurthestPositionScrapSpawn(Vector3 position, Item itemToSpawn)
        {
            RandomScrapSpawn randomScrapSpawn = Object.FindObjectsOfType<RandomScrapSpawn>()
                .Where(p => !p.spawnUsed)
                .OrderByDescending(p => Vector3.Distance(position, p.transform.position))
                .FirstOrDefault();

            if (randomScrapSpawn == null)
            {
                // Au cas où, mieux vaut prendre un spawn déjà utilisé que de le faire apparaître devant le joueur
                randomScrapSpawn = Object.FindObjectsOfType<RandomScrapSpawn>()
                    .OrderByDescending(p => Vector3.Distance(position, p.transform.position))
                    .FirstOrDefault();
            }

            if (randomScrapSpawn.spawnedItemsCopyPosition)
                randomScrapSpawn.spawnUsed = true;
            else
                randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + Vector3.up * itemToSpawn.verticalOffset;

            return randomScrapSpawn.transform.position + Vector3.up * 0.5f;
        }
    }
}
