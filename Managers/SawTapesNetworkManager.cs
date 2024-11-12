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
        public void SpawnTapeParticleClientRpc(NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                TapeSTManager.SpawnTapeParticle(ref grabbableObject);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TapeSearchServerRpc(int playerId)
        {
            if (TapeSTManager.tapeSearchCoroutine != null)
            {
                StopCoroutine(TapeSTManager.tapeSearchCoroutine);
            }
            TapeSTManager.tapeSearchCoroutine = StartCoroutine(TapeSTManager.TapeSearchCoroutine(playerId));
        }

        [ClientRpc]
        public void UnlockDoorsClientRpc(int playerId)
        {
            TileSTBehaviour tileSTBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerSTBehaviour>().tileGame?.GetComponent<TileSTBehaviour>();
            TileSTManager.UnlockDoors(ref tileSTBehaviour);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeTapePositionServerRpc(NetworkObjectReference obj, Vector3 position) => ChangeTapePositionClientRpc(obj, position);

        [ClientRpc]
        public void ChangeTapePositionClientRpc(NetworkObjectReference obj, Vector3 position)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                TapeSTManager.ChangeTapePosition(ref grabbableObject, position);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void EnableParticleServerRpc(NetworkObjectReference obj, bool enable) => EnableParticleClientRpc(obj, enable);

        [ClientRpc]
        public void EnableParticleClientRpc(NetworkObjectReference obj, bool enable)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                TapeSTManager.EnableParticle(grabbableObject, enable);
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
            {
                player.KillPlayer(velocity, spawnBody, (CauseOfDeath)causeOfDeath);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnSawKeyServerRpc(Vector3 position) => RoundManagerPatch.SpawnItem(ref SawTapes.sawKeyObj, position);

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPursuerEyeServerRpc(Vector3 position) => RoundManagerPatch.SpawnItem(ref SawTapes.pursuerEyeObj, position);

        [ClientRpc]
        public void PlayDespawnParticleClientRpc(Vector3 position)
        {
            GameObject spawnObject = Instantiate(SawTapes.despawnParticle, position, Quaternion.identity);
            ParticleSystem despawnParticle = spawnObject.GetComponent<ParticleSystem>();
            Destroy(spawnObject, despawnParticle.main.duration + despawnParticle.main.startLifetime.constantMax);
        }
    }
}
