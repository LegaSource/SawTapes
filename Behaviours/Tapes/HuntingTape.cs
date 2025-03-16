using GameNetcodeStuff;
using SawTapes.Behaviours.Items;
using SawTapes.Files;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Tapes
{
    public class HuntingTape : SawTape
    {
        public List<NetworkObject> spawnedEnemies = new List<NetworkObject>();

        public override void Start()
        {
            base.Start();

            InstantiateAndAttachAudio(SawTapes.sawRecordingHunting);
            subtitlesGame = SubtitleFile.huntingGameSubtitles;

            minPlayersAmount = ConfigManager.huntingMinPlayers.Value;
            maxPlayersAmount = ConfigManager.huntingMaxPlayers.Value;

            gameDuration = ConfigManager.huntingDuration.Value;
            billyValue = ConfigManager.huntingBillyValue.Value;
        }

        public override void ExecutePostGasActionsForClient(PlayerControllerB player)
        {
            base.ExecutePostGasActionsForClient(player);

            SpawnReverseBearTrapServerRpc((int)player.playerClientId);
            SpawnShovelServerRpc(player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnReverseBearTrapServerRpc(int playerId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            ReverseBearTrap reverseBearTrap = RoundManagerPatch.SpawnItem(SawTapes.reverseBearTrapObj, player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation) as ReverseBearTrap;
            reverseBearTrap.InitializeReverseBearTrapClientRpc((int)player.playerClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnShovelServerRpc(Vector3 position)
        {
            GameObject shovel = null;
            foreach (NetworkPrefabsList networkPrefabList in NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists ?? Enumerable.Empty<NetworkPrefabsList>())
            {
                foreach (NetworkPrefab networkPrefab in networkPrefabList.PrefabList ?? Enumerable.Empty<NetworkPrefab>())
                {
                    GrabbableObject grabbableObject = networkPrefab.Prefab.GetComponent<GrabbableObject>();
                    if (grabbableObject == null || grabbableObject.itemProperties == null) continue;
                    if (!grabbableObject.itemProperties.itemName.Equals(Constants.SHOVEL)) continue;

                    shovel = networkPrefab.Prefab;
                    if (shovel != null) break;
                }
            }
            if (shovel != null) RoundManagerPatch.SpawnItem(shovel, position + Vector3.up * 0.5f);
        }

        public override void ExecuteStartGameActionsForServer()
        {
            base.ExecuteStartGameActionsForServer();
            SpawnEnemies();
        }

        public void SpawnEnemies()
        {
            Vector3[] spawnPositions = STUtilities.GetFurthestPositions(transform.position, playersAmount);
            if (spawnPositions.Length < playersAmount)
            {
                SawTapes.mls.LogWarning("Not enough positions available");
                EndGameForServer(true);
                return;
            }

            int index = 0;
            foreach (PlayerControllerB player in players)
            {
                SpawnEnemy(spawnPositions[index]);
                index++;
            }
            ShowEnemiesClientRpc(spawnedEnemies.Select(e => e.NetworkObjectId).ToArray());
        }

        public void SpawnEnemy(Vector3 spawnPosition)
        {
            List<EnemyType> eligibleEnemies = SawTapes.allEnemies
                .Where(e => e.canDie && !e.isOutsideEnemy && !ConfigManager.huntingExclusions.Value.Contains(e.enemyName))
                .ToList();

            EnemyType enemyType = eligibleEnemies.Count > 0
                ? eligibleEnemies[Random.Range(0, eligibleEnemies.Count)]
                : null;

            NetworkObject networkObject = EnemySTManager.SpawnEnemy(enemyType, spawnPosition);
            spawnedEnemies.Add(networkObject);
        }

        [ClientRpc]
        public void ShowEnemiesClientRpc(ulong[] enemyIds)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (!players.Contains(player)) return;

            List<EnemyAI> enemies = new List<EnemyAI>();
            foreach (ulong enemyId in enemyIds)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyId, out NetworkObject networkObject))
                {
                    enemies.Add(networkObject.GetComponentInChildren<EnemyAI>());
                    if (player.IsHost || player.IsServer) continue;

                    spawnedEnemies.Add(networkObject);
                }
            }
            STUtilities.ShowAura(enemies);
        }

        public override bool DoGameForServer(int iterator)
            => !players.All(p => p.isPlayerDead || (p.TryGetComponent(out PlayerSTBehaviour b) && b.reverseBearTrap != null && b.reverseBearTrap.isReleased));

        public override bool ExecutePreEndGameActionForServer(bool isGameCancelled)
        {
            base.ExecutePreEndGameActionForServer(isGameCancelled);

            bool hasLivingPlayer = false;
            foreach (PlayerControllerB player in players)
            {
                PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(player);
                if (playerBehaviour?.reverseBearTrap == null
                    || (!player.isPlayerDead && playerBehaviour.reverseBearTrap.isReleased))
                {
                    hasLivingPlayer = true;
                    continue;
                }

                DestroyReverseBearTrap(playerBehaviour.reverseBearTrap);

                if (isGameCancelled || player.isPlayerDead) continue;
                SawTapesNetworkManager.Instance.KillPlayerClientRpc((int)player.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
            }
            DespawnEnemies();
            DestroySawKeys();

            return !hasLivingPlayer;
        }

        public void DestroyReverseBearTrap(ReverseBearTrap reverseBearTrap)
        {
            SawTapes.mls.LogError("DestroyReverseBearTrap");
            NetworkObject networkObject = reverseBearTrap.GetComponent<NetworkObject>();
            if (networkObject == null || !networkObject.IsSpawned) return;
            SawTapes.mls.LogError("ReverseBearTrap IsSpawned");

            SawTapesNetworkManager.Instance.DestroyObjectClientRpc(networkObject);
        }

        public void DespawnEnemies()
        {
            foreach (NetworkObject spawnedEnemy in spawnedEnemies)
            {
                if (spawnedEnemy == null) continue;

                EnemyAI enemy = spawnedEnemy.GetComponentInChildren<EnemyAI>();
                if (enemy?.thisNetworkObject == null || !enemy.thisNetworkObject.IsSpawned) continue;
                if (enemy.isEnemyDead) continue;

                EnemySTManager.DespawnEnemy(spawnedEnemy);
            }
        }

        public void DestroySawKeys()
        {
            foreach (SawKey sawKey in Resources.FindObjectsOfTypeAll<SawKey>())
            {
                if (sawKey == null) continue;

                NetworkObject networkObject = sawKey.GetComponent<NetworkObject>();
                if (networkObject == null || !networkObject.IsSpawned) continue;

                SawTapesNetworkManager.Instance.DestroyObjectClientRpc(sawKey.GetComponent<NetworkObject>());
            }
        }

        public override void EndGameForAllClients(bool isGameEnded)
        {
            base.EndGameForAllClients(isGameEnded);
            spawnedEnemies.Clear();
        }
    }
}
