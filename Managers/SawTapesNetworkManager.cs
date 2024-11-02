using SawTapes.Behaviours;
using SawTapes.Patches;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    internal class SawTapesNetworkManager : NetworkBehaviour
    {
        public static SawTapesNetworkManager Instance;

        public void Awake()
        {
            Instance = this;
        }

        [ClientRpc]
        public void SetGenerateGameTileClientRpc(bool enable)
        {
            DungeonPatch.isGenerateTileGame = enable;
        }

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
        public void EnableParticleServerRpc(NetworkObjectReference obj, bool enable)
        {
            EnableParticleClientRpc(obj, enable);
        }

        [ClientRpc]
        private void EnableParticleClientRpc(NetworkObjectReference obj, bool enable)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                TapeSTManager.EnableParticle(grabbableObject, enable);
            }
        }

        [ClientRpc]
        public void AddTileInfosClientRpc(Vector3 tilePos, Vector3[] doorsPos, NetworkObjectReference[] entrancesObj, NetworkObjectReference obj)
        {
            TileSTManager.AddTileInfos(tilePos, doorsPos, entrancesObj, obj);
        }
    }
}
