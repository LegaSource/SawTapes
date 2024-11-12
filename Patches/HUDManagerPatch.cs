using DunGen;
using HarmonyLib;
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
        private static void StartHUDManager(ref HUDManager __instance)
        {
            GameObject chrono = new GameObject("ChronoUI");
            chrono.transform.localPosition = new Vector3(0f, 0f, 0f);
            chrono.AddComponent<RectTransform>();

            TextMeshProUGUI textMeshChrono = chrono.AddComponent<TextMeshProUGUI>();
            RectTransform rectTransformChrono = textMeshChrono.rectTransform;
            rectTransformChrono.SetParent(GameObject.Find("Systems/UI/Canvas/Panel/GameObject/PlayerScreen").transform, worldPositionStays: false);
            rectTransformChrono.anchorMin = new Vector2(0f, 1f);
            rectTransformChrono.anchorMax = new Vector2(0f, 1f);
            rectTransformChrono.pivot = new Vector2(0f, 1f);
            rectTransformChrono.anchoredPosition = new Vector2(ConfigManager.chronoPosX.Value, ConfigManager.chronoPosY.Value);
            rectTransformChrono.sizeDelta = new Vector2(300f, 300f);
            textMeshChrono.alignment = TextAlignmentOptions.TopLeft;
            textMeshChrono.font = __instance.controlTipLines[0].font;
            textMeshChrono.fontSize = 14f;
            chronoText = textMeshChrono;

            if (ConfigManager.isSubtitles.Value)
            {
                GameObject subtitle = new GameObject("SubtitleUI");
                subtitle.transform.localPosition = new Vector3(0f, -125f, 0f);
                subtitle.AddComponent<RectTransform>();

                TextMeshProUGUI textMeshProUGUI = subtitle.AddComponent<TextMeshProUGUI>();
                RectTransform rectTransform = textMeshProUGUI.rectTransform;
                rectTransform.SetParent(GameObject.Find("Systems/UI/Canvas/Panel/GameObject/PlayerScreen").transform, worldPositionStays: false);
                rectTransform.sizeDelta = new Vector2(600f, 200f);
                rectTransform.anchoredPosition = new Vector2(0f, -125f);
                textMeshProUGUI.alignment = TextAlignmentOptions.Center;
                textMeshProUGUI.font = __instance.controlTipLines[0].font;
                textMeshProUGUI.fontSize = 14f;
                subtitleText = textMeshProUGUI;
            }
        }

        public static IEnumerator StartChronoCoroutine(int seconds)
        {
            while (!IsChronoEnded(seconds))
            {
                seconds--;
                yield return new WaitForSeconds(1f);
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
            if (!__result)
            {
                return;
            }
            InteractTrigger interactTrigger = GameNetworkManager.Instance.localPlayerController.hoveringOverTrigger;
            if (interactTrigger == null)
            {
                return;
            }
            EntranceTeleport entranceTeleport = interactTrigger.GetComponent<EntranceTeleport>();
            if (entranceTeleport != null && IsEntranceBlocked(ref entranceTeleport))
            {
                HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, Constants.MESSAGE_IMPAC_LOCKED_ENTRANCE);
                __result = false;
            }
        }

        public static bool IsEntranceBlocked(ref EntranceTeleport entranceTeleport)
        {
            if (blockedEntrances.ContainsKey(entranceTeleport))
            {
                return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SetScreenFilters))]
        [HarmonyPrefix]
        private static bool UpdateScreenFilters() => !isFlashFilterUsed;
    }
}
