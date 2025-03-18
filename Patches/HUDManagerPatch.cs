using DunGen;
using HarmonyLib;
using SawTapes.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SawTapes.Patches
{
    internal class HUDManagerPatch
    {
        public static TextMeshProUGUI chronoText;
        public static TextMeshProUGUI subtitleText;
        public static bool isChronoEnded = false;
        public static Dictionary<EntranceTeleport, Tile> blockedEntrances = new Dictionary<EntranceTeleport, Tile>();
        public static bool isFlashFilterUsed = false;

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Start))]
        [HarmonyPostfix]
        private static void StartHUDManager()
        {
            chronoText = HUDSTManager.CreateUIElement(
                name: "ChronoUI",
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                pivot: new Vector2(0f, 1f),
                anchoredPosition: new Vector2(ConfigManager.chronoPosX.Value, ConfigManager.chronoPosY.Value),
                sizeDelta: new Vector2(300f, 300f),
                alignment: TextAlignmentOptions.TopLeft
            );

            if (ConfigManager.isSubtitles.Value)
            {
                subtitleText = HUDSTManager.CreateUIElement(
                    name: "SubtitleUI",
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    pivot: new Vector2(0.5f, 0.5f),
                    anchoredPosition: new Vector2(0f, -125f),
                    sizeDelta: new Vector2(600f, 200f),
                    alignment: TextAlignmentOptions.Center
                );
            }
        }

        public static IEnumerator StartChronoCoroutine(int seconds)
        {
            while (!IsChronoEnded(seconds))
            {
                seconds--;
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        private static bool IsChronoEnded(int totalSeconds)
        {
            int minutes = (int)Math.Floor(totalSeconds / 60.0);
            int seconds = (int)Math.Floor(totalSeconds % 60.0);

            chronoText.text = $"{minutes:D2}:{seconds:D2}";

            if (GameNetworkManager.Instance.localPlayerController.isPlayerDead || isChronoEnded || (minutes == 0 && seconds == 0))
            {
                chronoText.text = "";
                isChronoEnded = false;
                return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.HoldInteractionFill))]
        [HarmonyPostfix]
        private static void HoldInteraction(ref bool __result)
        {
            if (!__result) return;

            InteractTrigger interactTrigger = GameNetworkManager.Instance.localPlayerController.hoveringOverTrigger;
            if (interactTrigger == null) return;

            EntranceTeleport entranceTeleport = interactTrigger.GetComponent<EntranceTeleport>();
            if (entranceTeleport == null || !IsEntranceBlocked(entranceTeleport)) return;

            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_INFO_IMP_ACTION);
            __result = false;
        }

        public static bool IsEntranceBlocked(EntranceTeleport entranceTeleport)
            => blockedEntrances.ContainsKey(entranceTeleport);

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SetScreenFilters))]
        [HarmonyPrefix]
        private static bool UpdateScreenFilters()
            => !isFlashFilterUsed;
    }
}
