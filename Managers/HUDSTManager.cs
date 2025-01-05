using TMPro;
using UnityEngine;

namespace SawTapes.Managers
{
    public class HUDSTManager
    {
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
    }
}
