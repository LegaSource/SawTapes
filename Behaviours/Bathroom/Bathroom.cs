using BepInEx;
using GameNetcodeStuff;
using LegaFusionCore.Managers;
using LegaFusionCore.Managers.NetworkManagers;
using SawTapes.Behaviours.Bathroom.Enemies;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Bathroom;

public class Bathroom : NetworkBehaviour
{
    public PlayerControllerB player;
    public BillyBathroom billy;
    public NavMeshSurface navMeshSurface;
    public AudioSource sawTheme;

    public Vector3 savedPosition;
    public Transform spawnPosition;
    public Transform billyPosition;
    public Transform sawPosition;

    public bool hasUsedSaw = false;
    public bool hasUsedKey = false;

    public int currentSpotIndex = 0;
    public bool isRightSpot = false;

    [ClientRpc]
    public void InitializeBathroomClientRpc(int playerId, NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        SawTapes.bathroom = networkObject.gameObject.GetComponentInChildren<Bathroom>();
        player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();

        savedPosition = player.transform.position;
        GetComponentsInChildren<InteractTrigger>().ToList().ForEach(t => t.interactable = false);

        if (player == GameNetworkManager.Instance.localPlayerController)
        {
            LFCNetworkManager.Instance.TeleportPlayerServerRpc((int)player.playerClientId, spawnPosition.position, false, false, true, withSpawnAnimation: true);
            SpawnBillyServerRpc();
            SpawnSawServerRpc();
        }

        SetUpChainClientRpc();
    }

    [ClientRpc]
    public void SetUpChainClientRpc()
    {
        ChainBathroom bathroomChain = GetComponentInChildren<ChainBathroom>();
        bathroomChain.player = player;

        bathroomChain.ConfigureAttach();
        bathroomChain.SetupCollisionIgnore();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBillyServerRpc()
    {
        NetworkObject networkObject = EnemySTManager.SpawnEnemyForServer(SawTapes.billyBathroomEnemy, billyPosition.position);
        SpawnBillyClientRpc(networkObject);
    }

    [ClientRpc]
    public void SpawnBillyClientRpc(NetworkObjectReference enemyObject)
    {
        if (!enemyObject.TryGet(out NetworkObject networkObject)) return;

        billy = networkObject.gameObject.GetComponentInChildren<BillyBathroom>();
        if (billy != null)
        {
            billy.targetPlayer = player;
            if (IsServer) billy.StartFollowingPlayer();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnSawServerRpc()
        => LFCObjectsManager.SpawnObjectForServer(SawTapes.sawBathroom.spawnPrefab, sawPosition.position);

    public void KeySpotInteraction()
    {
        int index = int.Parse(player.hoveringOverTrigger.name["KeySpot".Length..]);
        isRightSpot = currentSpotIndex == index;
        StartSlidingPuzzle();
    }

    public void StartSlidingPuzzle()
    {
        if (SawTapes.puzzleBoardInterface != null) Destroy(SawTapes.puzzleBoardInterface.gameObject);

        SawTapes.puzzleBoardInterface = Instantiate(SawTapes.puzzleBoardPrefab, Vector3.zero, Quaternion.identity, HUDManager.Instance.HUDContainer.transform.parent);
        SawTapes.puzzleBoardInterface.transform.localPosition = Vector3.zero;

        if (SawTapes.puzzleBoardInterface == null)
        {
            SawTapes.mls.LogError($"puzzleBoardInterface not initialized");
            return;
        }

        if (SawTapes.puzzleBoardInterface == null) return;
        if (player == null || player.twoHanded || player.inSpecialInteractAnimation) return;

        OpenPuzzleBoard(true);
    }

    public void OpenPuzzleBoard(bool enable)
    {
        player.inSpecialMenu = enable;

        Cursor.lockState = enable ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = enable;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnSawKeyServerRpc()
    {
        GrabbableObject grabbableObject = LFCObjectsManager.SpawnObjectForServer(SawTapes.sawKeyBathroom.spawnPrefab, player.transform.position);
        LFCNetworkManager.Instance.ForceGrabObjectClientRpc(grabbableObject.GetComponent<NetworkObject>(), (int)player.playerClientId);
    }

    public void EndOfGame()
    {
        if (hasUsedKey || hasUsedSaw)
        {
            //if (hasUsedSaw) Debuff vitesse
            LFCNetworkManager.Instance.TeleportPlayerServerRpc((int)player.playerClientId, savedPosition, false, false, true);
        }
        else
        {
            LFCNetworkManager.Instance.KillPlayerServerRpc((int)player.playerClientId, Vector3.zero, false, (int)CauseOfDeath.Unknown);
        }
        if (!HUDManagerPatch.chronoText.text.IsNullOrWhiteSpace()) HUDManagerPatch.isChronoEnded = true;
        LFCNetworkManager.Instance.DestroyObjectServerRpc(billy.gameObject);
        Destroy(gameObject);
    }
}
