#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CountryGuessGame.Core;

namespace CountryGuessGame.Editor
{
    public class CountryGameFixUI : MonoBehaviour
    {
        [MenuItem("Country Guess Game/Fix UI Layout")]
        public static void FixUILayout()
        {
            // Find AnswersContainer
            GameObject answersContainer = GameObject.Find("AnswersContainer");
            if (answersContainer == null)
            {
                Debug.LogError("AnswersContainer not found!");
                return;
            }

            // Fix AnswersContainer RectTransform
            RectTransform containerRT = answersContainer.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0);
            containerRT.anchorMax = new Vector2(0.5f, 0);
            containerRT.pivot = new Vector2(0.5f, 0);
            containerRT.anchoredPosition = new Vector2(0, 50);
            containerRT.sizeDelta = new Vector2(500, 250);

            // Fix or add VerticalLayoutGroup
            VerticalLayoutGroup vlg = answersContainer.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = answersContainer.AddComponent<VerticalLayoutGroup>();
            }
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            // Add ContentSizeFitter if not present
            ContentSizeFitter csf = answersContainer.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = answersContainer.AddComponent<ContentSizeFitter>();
            }
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Fix each answer button
            AnswerButton[] buttons = answersContainer.GetComponentsInChildren<AnswerButton>();
            foreach (var btn in buttons)
            {
                FixAnswerButton(btn.gameObject);
            }

            EditorUtility.SetDirty(answersContainer);

            // Save scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log($"Fixed UI Layout - {buttons.Length} buttons updated");
        }

        private static void FixAnswerButton(GameObject buttonGO)
        {
            // Fix RectTransform
            RectTransform rt = buttonGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(480, 50);

            // Add LayoutElement
            LayoutElement le = buttonGO.GetComponent<LayoutElement>();
            if (le == null)
            {
                le = buttonGO.AddComponent<LayoutElement>();
            }
            le.minHeight = 50;
            le.preferredHeight = 50;
            le.flexibleWidth = 1;

            // Ensure Button component is set up correctly
            Button button = buttonGO.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = true;

                // Set up button colors
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(0.2f, 0.4f, 0.8f, 1f);
                colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f, 1f);
                colors.pressedColor = new Color(0.1f, 0.3f, 0.7f, 1f);
                colors.selectedColor = new Color(0.2f, 0.4f, 0.8f, 1f);
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                button.colors = colors;

                // Make sure transition is ColorTint
                button.transition = Selectable.Transition.ColorTint;
            }

            // Fix Image component
            Image img = buttonGO.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
                img.color = new Color(0.2f, 0.4f, 0.8f, 1f);
            }

            // Fix text child
            TextMeshProUGUI tmp = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.raycastTarget = false; // Text should not block raycasts
                tmp.fontSize = 24;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;

                // Fix text RectTransform
                RectTransform textRT = tmp.GetComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = new Vector2(10, 5);
                textRT.offsetMax = new Vector2(-10, -5);
            }

            EditorUtility.SetDirty(buttonGO);
        }
    }
}
#endif
