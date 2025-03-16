using SawTapes.Behaviours.Items;
using SawTapes.Files;
using SawTapes.Patches;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours
{
    public class Billy : EnemyAI
    {
        public bool isMoving = false;
        public BillyPuppet billyPuppet;
        public int billyValue = 0;
        public AudioSource bikeSqueek;
        public AudioSource billyRecording;

        public override void Start()
        {
            enemyType = SawTapes.billyEnemy;
            base.Start();

            if (bikeSqueek == null) bikeSqueek = GetComponent<AudioSource>();
            if (bikeSqueek == null) SawTapes.mls.LogError("bikeSqueek is not assigned and could not be found.");

            GameObject audioObject = Instantiate(SawTapes.billyRecordingSurvival, transform.position, Quaternion.identity);
            billyRecording = audioObject.GetComponent<AudioSource>();
            audioObject.transform.SetParent(transform);

            syncMovementSpeed = 0;
            updatePositionThreshold = 99999;
        }

        public void StartFollowingPlayer()
            => isMoving = true;

        public override void Update()
        {
            base.Update();
            if (IsServer && isMoving && targetPlayer != null) MoveTowardPlayer();
        }

        public void MoveTowardPlayer()
        {
            StartMovingClientRpc();
            if (Vector3.Distance(targetPlayer.transform.position, transform.position) <= 4f)
            {
                isMoving = false;

                StopMovingClientRpc();
                StartCoroutine(BillyAnnouncementCoroutine());
            }
        }

        [ClientRpc]
        public void StartMovingClientRpc()
        {
            PlayMovementSound();
            creatureAnimator?.SetBool("isMoving", true);
            SetMovingTowardsTargetPlayer(targetPlayer);
        }

        [ClientRpc]
        public void StopMovingClientRpc()
        {
            moveTowardsDestination = false;
            movingTowardsTargetPlayer = false;
            targetPlayer = null;
            agent.speed = 0f;
            bikeSqueek.Stop();
            creatureAnimator?.SetBool("isMoving", false);
        }

        public void PlayMovementSound()
        {
            if (bikeSqueek == null || bikeSqueek.isPlaying) return;
            RoundManager.PlayRandomClip(bikeSqueek, [bikeSqueek.clip], randomize: true);
        }

        public IEnumerator BillyAnnouncementCoroutine()
        {
            BillyAnnouncementClientRpc();

            yield return new WaitUntil(() => billyRecording.isPlaying);
            yield return new WaitWhile(() => billyRecording.isPlaying);

            if (billyPuppet == null)
            {
                Vector3 position = transform.position;
                billyPuppet = RoundManagerPatch.SpawnItem(SawTapes.billyPuppetObj, position) as BillyPuppet;
                SpawnBillyClientRpc(billyPuppet.GetComponent<NetworkObject>());
            }
        }

        [ClientRpc]
        public void BillyAnnouncementClientRpc()
        {
            billyRecording.Play();
            if (!ConfigManager.isSubtitles.Value) return;
            StartCoroutine(ShowSubtitlesCoroutine());
        }

        [ClientRpc]
        public void SpawnBillyClientRpc(NetworkObjectReference obj)
        {
            if (!obj.TryGet(out var networkObject)) return;
            
            billyPuppet = networkObject.gameObject.GetComponentInChildren<BillyPuppet>();
            if (billyPuppet != null)
            {
                billyPuppet.EnableItemMeshes(false);
                billyPuppet.SetScrapValue(billyValue);
                billyPuppet.billy = this;
            }
            else
            {
                SawTapes.mls.LogError("billyPuppet could not be found.");
            }
        }

        public IEnumerator ShowSubtitlesCoroutine()
        {
            while (billyRecording.isPlaying)
            {
                string subtitleText = SubtitleFile.billySubtitles.Where(s => s.Timestamp <= billyRecording.time).OrderByDescending(s => s.Timestamp).FirstOrDefault()?.Text;
                if (!string.IsNullOrEmpty(subtitleText))
                {
                    if (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, transform.position) <= 25)
                        HUDManagerPatch.subtitleText.text = subtitleText;
                    else
                        HUDManagerPatch.subtitleText.text = "";
                }
                yield return null;
            }
            HUDManagerPatch.subtitleText.text = "";
        }
    }
}
