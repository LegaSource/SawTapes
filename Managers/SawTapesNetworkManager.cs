using SawTapes.Behaviours.Bathroom;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers;

public class SawTapesNetworkManager : NetworkBehaviour
{
    public static SawTapesNetworkManager Instance;

    public void Awake()
        => Instance = this;

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBathroomServerRpc(int playerId)
    {
        GameObject bathroomObj = Instantiate(SawTapes.bathroomObj, (Vector3.up * ConfigManager.bathroomPosY.Value) + (Vector3.right * ConfigManager.bathroomPosX.Value), Quaternion.identity);
        bathroomObj.GetComponent<NetworkObject>().Spawn();

        SawTapes.bathroom = bathroomObj.GetComponent<Bathroom>();
        SawTapes.bathroom.InitializeBathroomClientRpc(playerId, bathroomObj.GetComponent<NetworkObject>());
    }
}
