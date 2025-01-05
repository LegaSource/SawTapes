using GameNetcodeStuff;
using SawTapes.Behaviours;
using SawTapes.Patches;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    public class SawTapesNetworkManager : NetworkBehaviour
    {
        public static SawTapesNetworkManager Instance;

        public void Awake() => Instance = this;

        [ClientRpc]
        public void SetGenerateGameTileClientRpc(bool enable) => DungeonPatch.isGenerateTileGame = enable;

        [ClientRpc]
        public void SetScrapValueClientRpc(NetworkObjectReference obj, int value)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                grabbableObject.SetScrapValue(value);
            }
        }

        [ClientRpc]
        public void SpawnBlackParticleClientRpc(NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                ObjectSTManager.SpawnBlackParticle(grabbableObject);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TapeSearchServerRpc(int playerId)
        {
            if (TapeSTManager.tapeSearchCoroutine != null)
                StopCoroutine(TapeSTManager.tapeSearchCoroutine);
            TapeSTManager.tapeSearchCoroutine = StartCoroutine(TapeSTManager.TapeSearchCoroutine(playerId));
        }

        [ClientRpc]
        public void OpenTileDoorsClientRpc(int playerId)
        {
            PlayerSTBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>();
            TileSTManager.OpenTileDoors(playerBehaviour);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeObjectPositionServerRpc(NetworkObjectReference obj, Vector3 position) => ChangeObjectPositionClientRpc(obj, position);

        [ClientRpc]
        public void ChangeObjectPositionClientRpc(NetworkObjectReference obj, Vector3 position)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                ObjectSTManager.ChangeObjectPosition(grabbableObject, position);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void EnableBlackParticleServerRpc(NetworkObjectReference obj, bool enable) => EnableBlackParticleClientRpc(obj, enable);

        [ClientRpc]
        public void EnableBlackParticleClientRpc(NetworkObjectReference obj, bool enable)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                ObjectSTManager.EnableBlackParticle(grabbableObject, enable);
            }
        }

        [ClientRpc]
        public void AddTileInfosClientRpc(Vector3 tilePos, Vector3[] doorsPos, NetworkObjectReference[] entrancesObj, NetworkObjectReference obj)
            => TileSTManager.AddTileInfos(tilePos, doorsPos, entrancesObj, obj);

        [ServerRpc(RequireOwnership = false)]
        public void DestroyObjectServerRpc(NetworkObjectReference obj) => DestroyObjectClientRpc(obj);

        [ClientRpc]
        public void DestroyObjectClientRpc(NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                grabbableObject.DestroyObjectInHand(grabbableObject.playerHeldBy);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void KillPlayerServerRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath) => KillPlayerClientRpc(playerId, velocity, spawnBody, causeOfDeath);

        [ClientRpc]
        public void KillPlayerClientRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player == GameNetworkManager.Instance.localPlayerController)
                player.KillPlayer(velocity, spawnBody, (CauseOfDeath)causeOfDeath);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnSawKeyServerRpc(Vector3 position) => RoundManagerPatch.SpawnItem(SawTapes.sawKeyObj, position);

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPursuerEyeServerRpc(Vector3 position) => RoundManagerPatch.SpawnItem(SawTapes.pursuerEyeObj, position);

        [ClientRpc]
        public void PlayDespawnParticleClientRpc(Vector3 position)
        {
            GameObject spawnObject = Instantiate(SawTapes.despawnParticle, position, Quaternion.identity);
            ParticleSystem despawnParticle = spawnObject.GetComponent<ParticleSystem>();
            Destroy(spawnObject, despawnParticle.main.duration + despawnParticle.main.startLifetime.constantMax);
        }

        [ClientRpc]
        public void PlayerEndPathGuideClientRpc(int playerId)
        {
            PlayerSTBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>();
            SawGameSTManager.PlayerEndPathGuide(playerBehaviour);
        }

        [ClientRpc]
        public void SpawnPathParticleClientRpc(Vector3 leftPosition, Vector3 rightPosition, int[] playerIds)
        {
            if (GameNetworkManager.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.localPlayerController.IsServer) return;

            foreach (int playerId in playerIds)
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                if (GameNetworkManager.Instance.localPlayerController == player)
                {
                    SawGameSTManager.SpawnPathParticle(leftPosition);
                    SawGameSTManager.SpawnPathParticle(rightPosition);
                }
            }
        }
    }
}
