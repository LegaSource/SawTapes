using SawTapes.Behaviours;
using SawTapes.Behaviours.Tapes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    public class TapeSTManager
    {
        public static Coroutine tapeSearchCoroutine;

        public static IEnumerator TapeSearchCoroutine(int playerId)
        {
            PlayerSTBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>();
            SawTape sawTape = playerBehaviour.tileGame?.GetComponent<TileSTBehaviour>()?.sawTape;
            if (sawTape != null)
            {
                List<RandomScrapSpawn> listRandomScrapSpawn = Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => playerBehaviour.tileGame.Bounds.Contains(s.transform.position)).ToList();
                
                yield return new WaitForSeconds(30f);

                while (sawTape != null && !sawTape.sawRecording.isPlaying)
                {
                    if (sawTape.playerHeldBy == null)
                    {
                        if (playerBehaviour.playerProperties.isPlayerDead) break;

                        Vector3 position = playerBehaviour.playerProperties.transform.position + Vector3.up * 0.5f;
                        if (listRandomScrapSpawn.Count > 0)
                        {
                            List<RandomScrapSpawn> distantRandomScrapSpawns = listRandomScrapSpawn.Where(s => Vector3.Distance(s.transform.position, sawTape.transform.position) > 1f).ToList();
                            if (distantRandomScrapSpawns.Count > 0)
                            {
                                RandomScrapSpawn randomScrapSpawn = distantRandomScrapSpawns[new System.Random().Next(distantRandomScrapSpawns.Count)];
                                if (randomScrapSpawn != null)
                                {
                                    if (!randomScrapSpawn.spawnedItemsCopyPosition)
                                        randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + Vector3.up * sawTape.itemProperties.verticalOffset;
                                    position = randomScrapSpawn.transform.position + Vector3.up * 0.5f;
                                }
                            }
                        }
                        SawTapesNetworkManager.Instance.ChangeObjectPositionClientRpc(sawTape.GetComponent<NetworkObject>(), position);
                    }

                    yield return new WaitForSeconds(30f);
                }
            }
            else
            {
                SawTapesNetworkManager.Instance.OpenTileDoorsClientRpc((int)playerBehaviour.playerProperties.playerClientId);
                SawTapes.mls.LogWarning("Tape not found, doors unlocked.");
            }
        }
    }
}
