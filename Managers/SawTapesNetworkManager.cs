using GameNetcodeStuff;
using SawTapes.Behaviours;
using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.Tapes;
using SawTapes.Patches;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Managers
{
    public class SawTapesNetworkManager : NetworkBehaviour
    {
        public static SawTapesNetworkManager Instance;

        public void Awake()
            => Instance = this;

        [ClientRpc]
        public void SetScrapValueClientRpc(NetworkObjectReference obj, int value)
        {
            if (!obj.TryGet(out var networkObject)) return;

            GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
            grabbableObject.SetScrapValue(value);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeObjectPositionServerRpc(NetworkObjectReference obj, Vector3 position)
            => ChangeObjectPositionClientRpc(obj, position);

        [ClientRpc]
        public void ChangeObjectPositionClientRpc(NetworkObjectReference obj, Vector3 position)
        {
            if (!obj.TryGet(out var networkObject)) return;

            GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
            ObjectSTManager.ChangeObjectPosition(grabbableObject, position);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DestroyObjectServerRpc(NetworkObjectReference obj)
            => DestroyObjectClientRpc(obj);

        [ClientRpc]
        public void DestroyObjectClientRpc(NetworkObjectReference obj)
        {
            if (!obj.TryGet(out var networkObject)) return;

            GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
            grabbableObject.DestroyObjectInHand(grabbableObject.playerHeldBy);
        }

        [ClientRpc]
        public void KillPlayerClientRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player != GameNetworkManager.Instance.localPlayerController) return;
            
            player.KillPlayer(velocity, spawnBody, (CauseOfDeath)causeOfDeath);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPursuerEyeServerRpc(Vector3 position)
            => RoundManagerPatch.SpawnItem(SawTapes.pursuerEye.spawnPrefab, position);

        [ClientRpc]
        public void PlayDespawnParticleClientRpc(Vector3 position)
        {
            GameObject spawnObject = Instantiate(SawTapes.despawnParticle, position, Quaternion.identity);
            ParticleSystem despawnParticle = spawnObject.GetComponent<ParticleSystem>();
            Destroy(spawnObject, despawnParticle.main.duration + despawnParticle.main.startLifetime.constantMax);
        }

        [ClientRpc]
        public void PlayerEndPathGuideClientRpc(int playerId, NetworkObjectReference obj)
        {
            if (!obj.TryGet(out var networkObject)) return;

            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player != GameNetworkManager.Instance.localPlayerController) return;

            Saw saw = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as Saw;
            CustomPassManager.SetupCustomPassForObjects([saw.gameObject]);
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

        [ServerRpc(RequireOwnership = false)]
        public void TeleportSawToPlayerServerRpc(int playerId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            PlayerSTBehaviour playerBehaviour = PlayerSTManager.GetPlayerBehaviour(player);
            if (playerBehaviour == null) return;

            EscapeTape escapeTape = playerBehaviour.sawTape as EscapeTape;
            if (escapeTape == null || escapeTape.saw == null) return;
            if (Vector3.Distance(escapeTape.saw.transform.position, player.transform.position) > 15f) return;

            ChangeObjectPositionClientRpc(escapeTape.saw.GetComponent<NetworkObject>(), player.transform.position + Vector3.up * 0.5f);
        }
    }
}
