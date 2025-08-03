using GameNetcodeStuff;
using LegaFusionCore.Utilities;
using SawTapes.Behaviours.Items.Addons;
using SawTapes.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items;

public class SawBC : PhysicsProp
{
    public AudioSource sawAudio;
    public List<RaycastHit> objectsHitBySawList = [];
    public PlayerControllerB previousPlayerHeldBy;
    public RaycastHit[] objectsHitBySaw;
    public int sawHitForce = 1;
    public AudioClip[] hitSFX;
    public AudioClip[] swingSFX;
    public int sawMask = 1084754248;
    public float timeAtLastDamageDealt;

    public override void Start()
    {
        base.Start();
        if (IsServer || IsHost)
        {
            InitializeSawForServer();
        }
    }

    public void InitializeSawForServer()
        => InitializeSawClientRpc();

    [ClientRpc]
    public void InitializeSawClientRpc()
    {
        SetScrapValue(ConfigManager.sawValue.Value);
        LFCUtilities.SetAddonComponent<BleedingChains>(this, Constants.BLEEDING_CHAINS);
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        if (!buttonDown || playerHeldBy == null || !IsOwner) return;

        _ = RoundManager.PlayRandomClip(sawAudio, swingSFX);
        if (playerHeldBy != null)
        {
            previousPlayerHeldBy = playerHeldBy;
            if (playerHeldBy.IsOwner) playerHeldBy.playerBodyAnimator.SetTrigger("UseHeldItem1");
        }
        if (IsOwner) HitSaw();
    }

    public void HitSaw(bool cancel = false)
    {
        if (previousPlayerHeldBy == null)
        {
            SawTapes.mls.LogError("previousPlayerHeldBy is null on this client when HitSaw is called");
            return;
        }
        previousPlayerHeldBy.activatingItem = false;
        bool hitDetected = false;
        bool hittableObjectHit = false;
        int footstepSurfaceIndex = -1;
        if (!cancel && Time.realtimeSinceStartup - timeAtLastDamageDealt > 0.43f)
        {
            previousPlayerHeldBy.twoHanded = false;
            objectsHitBySaw = Physics.SphereCastAll(previousPlayerHeldBy.gameplayCamera.transform.position + (previousPlayerHeldBy.gameplayCamera.transform.right * 0.1f), 0.5f, previousPlayerHeldBy.gameplayCamera.transform.forward, 0.75f, sawMask, QueryTriggerInteraction.Collide);
            objectsHitBySawList = objectsHitBySaw.OrderBy((RaycastHit x) => x.distance).ToList();

            foreach (RaycastHit daggerHit in objectsHitBySawList)
            {
                if (daggerHit.transform.gameObject.layer == 8 || daggerHit.transform.gameObject.layer == 11)
                {
                    hitDetected = true;
                    for (int i = 0; i < StartOfRound.Instance.footstepSurfaces.Length; i++)
                    {
                        if (StartOfRound.Instance.footstepSurfaces[i].surfaceTag == daggerHit.collider.gameObject.tag)
                        {
                            footstepSurfaceIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    if (!daggerHit.transform.TryGetComponent<IHittable>(out IHittable component) || daggerHit.transform == previousPlayerHeldBy.transform) continue;
                    if (!(daggerHit.point == Vector3.zero) && Physics.Linecast(previousPlayerHeldBy.gameplayCamera.transform.position, daggerHit.point, out RaycastHit hitInfo, StartOfRound.Instance.collidersAndRoomMaskAndDefault)) continue;

                    hitDetected = true;

                    try
                    {
                        if (Time.realtimeSinceStartup - timeAtLastDamageDealt > 0.43f)
                        {
                            timeAtLastDamageDealt = Time.realtimeSinceStartup;
                            _ = component.Hit(sawHitForce, previousPlayerHeldBy.gameplayCamera.transform.forward, previousPlayerHeldBy, playHitSFX: true, 5);
                        }
                        hittableObjectHit = true;
                    }
                    catch (Exception arg)
                    {
                        SawTapes.mls.LogError($"Exception caught when hitting object with saw from player #{previousPlayerHeldBy.playerClientId}: {arg}");
                    }
                }
            }
        }
        if (hitDetected)
        {
            _ = RoundManager.PlayRandomClip(sawAudio, hitSFX);
            FindObjectOfType<RoundManager>().PlayAudibleNoise(transform.position, 17f, 0.8f);
            if (!hittableObjectHit && footstepSurfaceIndex != -1)
            {
                sawAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[footstepSurfaceIndex].hitSurfaceSFX);
                WalkieTalkie.TransmitOneShotAudio(sawAudio, StartOfRound.Instance.footstepSurfaces[footstepSurfaceIndex].hitSurfaceSFX);
            }
            HitSawServerRpc(footstepSurfaceIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HitSawServerRpc(int hitSurfaceID)
        => HitSawClientRpc(hitSurfaceID);

    [ClientRpc]
    public void HitSawClientRpc(int hitSurfaceID)
    {
        if (IsOwner) return;

        _ = RoundManager.PlayRandomClip(sawAudio, hitSFX);
        if (hitSurfaceID != -1) HitSurfaceWithSaw(hitSurfaceID);
    }

    public void HitSurfaceWithSaw(int hitSurfaceID)
    {
        sawAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
        WalkieTalkie.TransmitOneShotAudio(sawAudio, StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
    }
}
