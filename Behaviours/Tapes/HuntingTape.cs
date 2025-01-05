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
    public class HuntingTape : SawTapeGassing
    {
        public ReverseBearTrap reverseBearTrap;
        public EnemyAI assignedEnemy;

        public override void Start()
        {
            base.Start();

            InstantiateAndAttachAudio(SawTapes.sawRecordingHunting);
            subtitlesGame = SubtitleFile.huntingGameSubtitles;
        }

        public override void ExecutePlayerFlindedActionForAllClients() => mainPlayer.GetComponent<PlayerSTBehaviour>().huntingTape = this;

        public override void ExecutePostGassedSetUpActionForClient()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            SpawnReverseBearTrapServerRpc((int)localPlayer.playerClientId);
            Vector3 position = localPlayer.gameplayCamera.transform.position + localPlayer.gameplayCamera.transform.forward;
            SpawnShovelServerRpc(position);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnReverseBearTrapServerRpc(int playerId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            GrabbableObject reverseBearTrap = RoundManagerPatch.SpawnItem(SawTapes.reverseBearTrapObj, player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation);
            InitializeReverseBearTrapClientRpc(reverseBearTrap.GetComponent<NetworkObject>(), (int)player.playerClientId);
        }

        [ClientRpc]
        public void InitializeReverseBearTrapClientRpc(NetworkObjectReference obj, int playerId)
        {
            if (obj.TryGet(out var networkObject))
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                ReverseBearTrap reverseBearTrap = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as ReverseBearTrap;
                reverseBearTrap.grabbable = false;
                reverseBearTrap.grabbableToEnemies = false;
                reverseBearTrap.hasHitGround = false;
                reverseBearTrap.EnablePhysics(enable: false);
                reverseBearTrap.SetScrapValue(ConfigManager.huntingReverseBearTrapValue.Value);

                if (GameNetworkManager.Instance.localPlayerController == player)
                    reverseBearTrap.parentObject = player.gameplayCamera.transform;
                else
                    reverseBearTrap.parentObject = player.playerGlobalHead.transform;

                PlayerSTBehaviour playerBehaviour = player.GetComponent<PlayerSTBehaviour>();
                playerBehaviour.isInGame = true;
                this.reverseBearTrap = reverseBearTrap;
            }
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
                    if (grabbableObject != null && grabbableObject.itemProperties.itemName.Equals(Constants.SHOVEL))
                    {
                        shovel = networkPrefab.Prefab;
                        if (shovel != null) break;
                    }
                }
            }
            if (shovel != null)
                RoundManagerPatch.SpawnItem(shovel, position + Vector3.up * 0.5f);
        }

        public override void ExecutePreGameActionForServer(PlayerSTBehaviour playerBehaviour)
        {
            SpawnEnemy(playerBehaviour.playerProperties);
            gameDuration = ConfigManager.huntingDuration.Value;
            billyValue = ConfigManager.huntingBillyValue.Value;
        }

        public void SpawnEnemy(PlayerControllerB player)
        {
            List<EnemyType> killableEnemies = SawTapes.allEnemies.Where(e => e.canDie && !e.isOutsideEnemy && !ConfigManager.huntingExclusions.Value.Contains(e.enemyName)).ToList();
            EnemyType enemyType = killableEnemies[new System.Random().Next(killableEnemies.Count)];
            Vector3 spawnPosition = EnemySTManager.GetFurthestPositionFromPlayer(player);
            NetworkObject networkObject = EnemySTManager.SpawnEnemy(enemyType, spawnPosition);
            SetAssignedEnemyClientRpc(networkObject);
        }

        [ClientRpc]
        public void SetAssignedEnemyClientRpc(NetworkObjectReference enemyObject)
        {
            if (enemyObject.TryGet(out NetworkObject networkObject))
                assignedEnemy = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
        }

        public override void ExecuteStartGameActionForAllClients(int gameDuration)
        {
            base.ExecuteStartGameActionForAllClients(gameDuration);
            if (GameNetworkManager.Instance.localPlayerController == mainPlayer)
                StartCoroutine(STUtilities.ShowEnemyCoroutine(assignedEnemy));
        }

        public override bool DoGameForServer(int iterator)
            => !(mainPlayer.isPlayerDead || reverseBearTrap.isReleased);

        public override bool ExecutePreEndGameActionForServer(bool isGameCancelled)
        {
            if (mainPlayer.isPlayerDead || !reverseBearTrap.isReleased)
            {
                DestroyReverseBearTrap();
                DespawnAssignedEnemy();
                DestroySawKey();
                if (!isGameCancelled && !mainPlayer.isPlayerDead)
                    SawTapesNetworkManager.Instance.KillPlayerClientRpc((int)mainPlayer.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
                return true;
            }
            return false;
        }

        public void DestroyReverseBearTrap()
        {
            if (reverseBearTrap != null)
                SawTapesNetworkManager.Instance.DestroyObjectClientRpc(reverseBearTrap.GetComponent<NetworkObject>());
        }

        public void DespawnAssignedEnemy()
        {
            if (assignedEnemy != null
                && !assignedEnemy.isEnemyDead
                && assignedEnemy.NetworkObject != null
                && assignedEnemy.NetworkObject.IsSpawned)
            {
                EnemySTManager.DespawnEnemy(assignedEnemy.NetworkObject);
            }
        }

        public void DestroySawKey()
        {
            SawKey sawKey = FindFirstObjectByType<SawKey>();
            if (sawKey != null)
                SawTapesNetworkManager.Instance.DestroyObjectClientRpc(sawKey.GetComponent<NetworkObject>());
        }

        public override void EndGameResetsForAllClients(bool isGameOver, bool isGameCancelled)
        {
            base.EndGameResetsForAllClients(isGameOver, isGameCancelled);
            assignedEnemy = null;
            reverseBearTrap = null;
        }
    }
}
