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
            if (enemy != null)
            {
                CustomPassManager.SetupCustomPassForEnemy(enemy);
                yield return new WaitForSeconds(ConfigManager.huntingAura.Value);
            }
            CustomPassManager.RemoveAura();
        }

        public static Transform FindMainEntrancePoint()
        {
            return Object.FindObjectsOfType<EntranceTeleport>().FirstOrDefault(e => e.entranceId == 0 && !e.isEntranceToBuilding).entrancePoint;
        }
    }
}
