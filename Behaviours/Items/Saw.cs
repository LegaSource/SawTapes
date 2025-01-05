using SawTapes.Behaviours.Tapes;
using SawTapes.Managers;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items
{
    public class Saw : PhysicsProp
    {
        public AudioSource sawAudio;
        public AudioClip[] hitSFX;
        public AudioClip[] swingSFX;
        public GameObject particleEffect;
        public bool hasBeenUsedForEscapeGame = false;
        public int currentUsesLeft;

        public override void Start()
        {
            base.Start();
            currentUsesLeft = ConfigManager.sawMaxUse.Value;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            
            if (playerHeldBy != null && buttonDown)
            {
                if (!hasBeenUsedForEscapeGame)
                {
                    EscapeTape escapeTape = playerHeldBy.GetComponent<PlayerSTBehaviour>().escapeTape;
                    if (escapeTape != null)
                        ActivateForEscapeGameServerRpc(escapeTape.GetComponent<NetworkObject>());
                    else
                        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_IMPAC_SAW);
                }

                if (IsOwner)
                {
                    if (Physics.Raycast(new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward), out RaycastHit hit, 2f, 524288, QueryTriggerInteraction.Collide))
                    {
                        EnemyAICollisionDetect collisionDetect = hit.collider.GetComponent<EnemyAICollisionDetect>();
                        if (collisionDetect != null && !collisionDetect.mainScript.isEnemyDead)
                        {
                            RoundManager.PlayRandomClip(sawAudio, swingSFX);
                            if (playerHeldBy.IsOwner)
                                playerHeldBy.playerBodyAnimator.SetTrigger("UseHeldItem1");
                            RoundManager.PlayRandomClip(sawAudio, hitSFX);

                            EnemyAI enemy = collisionDetect.mainScript;
                            if (enemy.enemyType.canDie && !enemy.enemyType.destroyOnDeath)
                                KillEnemyServerRpc(enemy.NetworkObject);
                            else
                                KillEnemyServerRpc(enemy.NetworkObject, true);
                        }
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ActivateForEscapeGameServerRpc(NetworkObjectReference obj) => ActivateForEscapeGameClientRpc(obj);

        [ClientRpc]
        public void ActivateForEscapeGameClientRpc(NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
                EscapeTape escapeTape = networkObject.gameObject.GetComponentInChildren<GrabbableObject>() as EscapeTape;
                escapeTape.sawHasBeenUsed = true;
                hasBeenUsedForEscapeGame = true;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void KillEnemyServerRpc(NetworkObjectReference enemyObject, bool overrideDestroy = false)
        {
            if (enemyObject.TryGet(out NetworkObject networkObject))
                networkObject.gameObject.GetComponentInChildren<EnemyAI>().KillEnemyOnOwnerClient(overrideDestroy);

            UpdateUsesLeftClientRpc();
        }

        [ClientRpc]
        public void UpdateUsesLeftClientRpc()
        {
            currentUsesLeft--;
            if (playerHeldBy != null && playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                if (currentUsesLeft <= 0)
                {
                    SawTapesNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
                    return;
                }
                SetControlTipsForItem();
            }
        }

        public override void SetControlTipsForItem()
        {
            itemProperties.toolTips[1] = $"[Uses Left : {currentUsesLeft}]";
            HUDManager.Instance.ChangeControlTipMultiple(itemProperties.toolTips, holdingItem: true, itemProperties);
        }

        public override void GrabItem()
        {
            base.GrabItem();

            if (particleEffect != null)
                SawTapesNetworkManager.Instance.EnableBlackParticleServerRpc(GetComponent<NetworkObject>(), false);
        }

        public override void OnDestroy()
        {
            if (particleEffect != null)
                Destroy(particleEffect.gameObject);

            base.OnDestroy();
        }
    }
}
