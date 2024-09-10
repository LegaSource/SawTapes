using DunGen;
using SawTapes.Behaviours;
using SawTapes.Patches;
using System.Linq;
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
        public void AddTileInfosClientRpc(Vector3 tilePos, Vector3[] doorsPos, NetworkObjectReference[] entrancesObj)
        {
            Tile tile = FindObjectsOfType<Tile>().FirstOrDefault(t => t.transform.position == tilePos);
            if (tile != null)
            {
                TileSTBehaviour tileBehaviour = tile.GetComponent<TileSTBehaviour>() ?? tile.gameObject.AddComponent<TileSTBehaviour>();
                foreach (Vector3 doorPos in doorsPos)
                {
                    DoorLock doorLock = FindObjectsOfType<DoorLock>().FirstOrDefault(t => t.transform.position == doorPos);
                    if (doorLock != null)
                    {
                        tileBehaviour.doorLocks.Add(doorLock);
                    }
                    else
                    {
                        SawTapes.mls.LogWarning("DoorLock not found during the creation of the game room");
                    }
                }
                foreach (NetworkObjectReference entranceObj in entrancesObj)
                {
                    if (entranceObj.TryGet(out var networkObject))
                    {
                        EntranceTeleport entranceTeleport = networkObject.GetComponent<EntranceTeleport>();
                        if (entranceTeleport != null)
                        {
                            tileBehaviour.entranceTeleports.Add(entranceTeleport);
                        }
                        else
                        {
                            SawTapes.mls.LogWarning("EntranceTeleport not found during the creation of the game room");
                        }
                    }
                }
            }
            else
            {
                SawTapes.mls.LogWarning("Tile not found during the creation of the game room");
            }
        }
    }
}
