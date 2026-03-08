#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace CountryGuessGame.Editor
{
    public class CountryGameFixLayout : MonoBehaviour
    {
        [MenuItem("Country Guess Game/Fix Answer Layout (2x2 Grid)")]
        public static void FixAnswerLayout()
        {
            GameObject answersContainer = GameObject.Find("AnswersContainer");
            if (answersContainer == null)
            {
                Debug.LogError("AnswersContainer not found!");
                return;
            }

            // Remove VerticalLayoutGroup if exists
            VerticalLayoutGroup vlg = answersContainer.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                Object.DestroyImmediate(vlg);
                Debug.Log("Removed VerticalLayoutGroup");
            }

            // Remove ContentSizeFitter if exists
            ContentSizeFitter csf = answersContainer.GetComponent<ContentSizeFitter>();
            if (csf != null)
            {
                Object.DestroyImmediate(csf);
            }

            // Add GridLayoutGroup
            GridLayoutGroup glg = answersContainer.GetComponent<GridLayoutGroup>();
            if (glg == null)
            {
                glg = answersContainer.AddComponent<GridLayoutGroup>();
            }

            // Configure grid: 2 columns x 2 rows
            glg.cellSize = new Vector2(220, 50);
            glg.spacing = new Vector2(15, 15);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.MiddleCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;
            glg.padding = new RectOffset(10, 10, 10, 10);

            // Fix RectTransform
            RectTransform rt = answersContainer.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 30);
            rt.sizeDelta = new Vector2(480, 140);

            EditorUtility.SetDirty(answersContainer);
            Debug.Log("Changed to 2x2 Grid Layout");

            // Save scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }

        [MenuItem("Country Guess Game/Fix Result Panel Buttons")]
        public static void FixResultPanelButtons()
        {
            GameObject resultPanel = GameObject.Find("ResultPanel");
            if (resultPanel == null)
            {
                Debug.LogError("ResultPanel not found!");
                return;
            }

            // Disable raycast on ResultPanel background
            Image panelImage = resultPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.raycastTarget = false;
                Debug.Log("Disabled raycast on ResultPanel background");
            }

            // Fix Retry button
            Transform retryTransform = resultPanel.transform.Find("RetryButton");
            if (retryTransform != null)
            {
                FixButton(retryTransform.gameObject, "Retry");
            }

            // Fix Next Level button
            Transform nextLevelTransform = resultPanel.transform.Find("NextLevelButton");
            if (nextLevelTransform != null)
            {
                FixButton(nextLevelTransform.gameObject, "Next Level");
            }

            // Save scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("Result panel buttons fixed!");
        }

        private static void FixButton(GameObject buttonGO, string buttonName)
        {
            // Ensure Image component with raycastTarget
            Image img = buttonGO.GetComponent<Image>();
            if (img == null)
            {
                img = buttonGO.AddComponent<Image>();
            }
            img.raycastTarget = true;
            img.color = new Color(0.2f, 0.6f, 0.2f, 1f);

            // Ensure Button component
            Button btn = buttonGO.GetComponent<Button>();
            if (btn == null)
            {
                btn = buttonGO.AddComponent<Button>();
            }
            btn.interactable = true;
            btn.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.2f, 0.6f, 0.2f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
            colors.pressedColor = new Color(0.1f, 0.5f, 0.1f, 1f);
            colors.selectedColor = new Color(0.2f, 0.6f, 0.2f, 1f);
            btn.colors = colors;

            // Fix RectTransform
            RectTransform rt = buttonGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 50);

            // Ensure text child
            TMPro.TextMeshProUGUI tmp = buttonGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.raycastTarget = false;
                tmp.text = buttonName;
            }

            EditorUtility.SetDirty(buttonGO);
            Debug.Log($"Fixed {buttonName} button");
        }
    }
}
#endif
