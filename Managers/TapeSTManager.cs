using SawTapes.Behaviours;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    internal class TapeSTManager
    {
        public static Coroutine tapeSearchCoroutine;

        public static void SpawnTapeParticle(ref GrabbableObject grabbableObject)
        {
            if (grabbableObject is SawTape sawTape)
            {
                GameObject curseParticleEffect = Object.Instantiate(SawTapes.tapeParticle, sawTape.transform.position, Quaternion.identity);
                curseParticleEffect.transform.SetParent(sawTape.transform);
                sawTape.particleEffect = curseParticleEffect;
            }
        }

        public static IEnumerator TapeSearchCoroutine(int playerId)
        {
            PlayerSTBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>();
            SawTape sawTape = playerBehaviour.tileGame.GetComponent<TileSTBehaviour>()?.sawTape;
            if (sawTape != null)
            {
                List<RandomScrapSpawn> listRandomScrapSpawn = Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => playerBehaviour.tileGame.Bounds.Contains(s.transform.position)).ToList();
                
                yield return new WaitForSeconds(30f);

                while (sawTape != null && !sawTape.sawRecording.isPlaying)
                {
                    if (sawTape.playerHeldBy == null)
                    {
                        if (playerBehaviour.playerProperties.isPlayerDead)
                        {
                            SawTapesNetworkManager.Instance.UnlockDoorsClientRpc((int)playerBehaviour.playerProperties.playerClientId);
                            break;
                        }

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
                                    {
                                        randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + Vector3.up * sawTape.itemProperties.verticalOffset;
                                    }
                                    position = randomScrapSpawn.transform.position + Vector3.up * 0.5f;
                                }
                            }
                        }
                        SawTapesNetworkManager.Instance.ChangeTapePositionClientRpc(sawTape.GetComponent<NetworkObject>(), position);
                    }

                    yield return new WaitForSeconds(30f);
                }
            }
            else
            {
                SawTapesNetworkManager.Instance.UnlockDoorsClientRpc((int)playerBehaviour.playerProperties.playerClientId);
                SawTapes.mls.LogWarning("Tape not found, doors unlocked.");
            }
        }

        public static void ChangeTapePosition(ref GrabbableObject grabbableObject, Vector3 position)
        {
            grabbableObject.EnableItemMeshes(false);
            grabbableObject.transform.localPosition = position;
            grabbableObject.transform.position = position;
            grabbableObject.startFallingPosition = position;
            grabbableObject.FallToGround();
            grabbableObject.EnableItemMeshes(true);
            EnableParticle(grabbableObject, true);
        }

        public static void EnableParticle(GrabbableObject grabbableObject, bool enable)
        {
            if (grabbableObject != null && grabbableObject is SawTape sawTape && sawTape.particleEffect != null)
            {
                sawTape.particleEffect.SetActive(enable);
            }
        }
    }
}
