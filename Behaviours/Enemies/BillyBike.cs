using SawTapes.Files.Values;
using SawTapes.Managers;
using SawTapes.Patches;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Enemies;

public class BillyBike : EnemyAI
{
    public HashSet<SubtitleMapping> subtitlesBilly = [];
    public AudioSource billyRecording;

    public void PlayMovementSound()
    {
        if (creatureSFX == null || creatureSFX.isPlaying) return;
        _ = RoundManager.PlayRandomClip(creatureSFX, [creatureSFX.clip], randomize: true);
    }

    public IEnumerator BillyDialogueCoroutine()
    {
        BillyAnnouncementClientRpc();

        yield return new WaitUntil(() => billyRecording.isPlaying);
        yield return new WaitWhile(() => billyRecording.isPlaying);

        ExecutePostDialogueForServer();
    }

    [ClientRpc]
    public void BillyAnnouncementClientRpc()
    {
        billyRecording.Play();
        if (!ConfigManager.isSubtitles.Value) return;
        _ = StartCoroutine(ShowSubtitlesCoroutine());
    }

    public IEnumerator ShowSubtitlesCoroutine()
    {
        while (billyRecording.isPlaying)
        {
            string subtitleText = subtitlesBilly.Where(s => s.Timestamp <= billyRecording.time).OrderByDescending(s => s.Timestamp).FirstOrDefault()?.Text;
            if (!string.IsNullOrEmpty(subtitleText))
            {
                HUDManagerPatch.subtitleText.text = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, transform.position) <= 25f
                    ? subtitleText
                    : "";
            }
            yield return null;
        }
        HUDManagerPatch.subtitleText.text = "";
    }

    public virtual void ExecutePostDialogueForServer() { }
}
