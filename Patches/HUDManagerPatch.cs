using HarmonyLib;
using SawTapes.Managers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SawTapes.Patches;

internal class HUDManagerPatch
{
    public static TextMeshProUGUI chronoText;
    public static TextMeshProUGUI subtitleText;

    public static bool isChronoEnded = false;
    public static int remainedTime;
    public static bool isFlashFilterUsed = false;

    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Start))]
    [HarmonyPostfix]
    private static void StartHUDManager()
    {
        chronoText = CreateUIElement(
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
            subtitleText = CreateUIElement(
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

    public static TextMeshProUGUI CreateUIElement(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, TextAlignmentOptions alignment)
    {
        Transform parent = GameObject.Find("Systems/UI/Canvas/Panel/GameObject/PlayerScreen").transform;
        GameObject uiElement = new GameObject(name);
        uiElement.transform.localPosition = Vector3.zero;

        RectTransform rectTransform = uiElement.AddComponent<RectTransform>();
        rectTransform.SetParent(parent, worldPositionStays: false);
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        TextMeshProUGUI textMesh = uiElement.AddComponent<TextMeshProUGUI>();
        textMesh.alignment = alignment;
        textMesh.font = HUDManager.Instance.controlTipLines[0].font;
        textMesh.fontSize = 14f;

        return textMesh;
    }

    public static IEnumerator StartChronoCoroutine(int seconds)
    {
        remainedTime = seconds;
        while (!IsChronoEnded(remainedTime))
        {
            remainedTime--;
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

    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SetScreenFilters))]
    [HarmonyPrefix]
    private static bool UpdateScreenFilters()
        => !isFlashFilterUsed;
}
