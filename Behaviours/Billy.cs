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
            if (bikeSqueek == null)
            {
                bikeSqueek = GetComponent<AudioSource>();
            }
            if (bikeSqueek == null)
            {
                SawTapes.mls.LogError("bikeSqueek is not assigned and could not be found.");
            }
            GameObject audioObject = Instantiate(SawTapes.billyRecordingSurvival, transform.position, Quaternion.identity);
            billyRecording = audioObject.GetComponent<AudioSource>();
            audioObject.transform.SetParent(transform);

            syncMovementSpeed = 0;
            updatePositionThreshold = 99999;
        }

        public void StartFollowingPlayer() => isMoving = true;

        public override void Update()
        {
            base.Update();
            if (IsServer && targetPlayer != null && isMoving)
            {
                MoveTowardPlayer();
            }
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
        private void StartMovingClientRpc()
        {
            PlayMovementSound();
            creatureAnimator?.SetBool("isMoving", true);
            SetMovingTowardsTargetPlayer(targetPlayer);
        }

        [ClientRpc]
        private void StopMovingClientRpc()
        {
            moveTowardsDestination = false;
            movingTowardsTargetPlayer = false;
            targetPlayer = null;
            agent.speed = 0f;
            bikeSqueek.Stop();
            creatureAnimator?.SetBool("isMoving", false);
        }

        private void PlayMovementSound()
        {
            if (bikeSqueek != null && !bikeSqueek.isPlaying)
            {
                RoundManager.PlayRandomClip(bikeSqueek, [bikeSqueek.clip], randomize: true);
            }
        }

        public IEnumerator BillyAnnouncementCoroutine()
        {
            BillyAnnouncementClientRpc();

            yield return new WaitUntil(() => billyRecording.isPlaying);
            yield return new WaitWhile(() => billyRecording.isPlaying);

            if (billyPuppet == null)
            {
                Vector3 position = transform.position;
                billyPuppet = RoundManagerPatch.SpawnItem(ref SawTapes.billyPuppetObj, position) as BillyPuppet;
                SpawnBillyClientRpc(billyPuppet.GetComponent<NetworkObject>());
            }
        }

        [ClientRpc]
        private void BillyAnnouncementClientRpc()
        {
            billyRecording.Play();
            if (ConfigManager.isSubtitles.Value) StartCoroutine(ShowSubtitlesCoroutine());
        }

        [ClientRpc]
        private void SpawnBillyClientRpc(NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
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
        }

        public IEnumerator ShowSubtitlesCoroutine()
        {
            while (billyRecording.isPlaying)
            {
                string subtitleText = SubtitleFile.billySubtitles.Where(s => s.Timestamp <= billyRecording.time).OrderByDescending(s => s.Timestamp).FirstOrDefault()?.Text;
                if (!string.IsNullOrEmpty(subtitleText))
                {
                    if (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, transform.position) <= 25)
                    {
                        HUDManagerPatch.subtitleText.text = subtitleText;
                    }
                    else
                    {
                        HUDManagerPatch.subtitleText.text = "";
                    }
                }
                yield return null;
            }
            HUDManagerPatch.subtitleText.text = "";
        }
    }
}
