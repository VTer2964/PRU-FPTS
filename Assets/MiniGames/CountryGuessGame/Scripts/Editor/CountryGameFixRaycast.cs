#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CountryGuessGame.Editor
{
    public class CountryGameFixRaycast : MonoBehaviour
    {
        [MenuItem("Country Guess Game/Fix Raycast Issues")]
        public static void FixRaycastIssues()
        {
            // Ensure EventSystem exists
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystem = eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
                Debug.Log("Created EventSystem");
            }
            else
            {
                // Make sure it has StandaloneInputModule
                if (eventSystem.GetComponent<StandaloneInputModule>() == null)
                {
                    eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                    Debug.Log("Added StandaloneInputModule to EventSystem");
                }
            }

            // Fix GamePanel - disable raycast on the panel background
            GameObject gamePanel = GameObject.Find("GamePanel");
            if (gamePanel != null)
            {
                Image panelImage = gamePanel.GetComponent<Image>();
                if (panelImage != null)
                {
                    panelImage.raycastTarget = false;
                    Debug.Log("Disabled raycastTarget on GamePanel Image");
                }
            }

            // Fix AnswersContainer - disable raycast
            GameObject answersContainer = GameObject.Find("AnswersContainer");
            if (answersContainer != null)
            {
                Image containerImage = answersContainer.GetComponent<Image>();
                if (containerImage != null)
                {
                    containerImage.raycastTarget = false;
                    Debug.Log("Disabled raycastTarget on AnswersContainer Image");
                }
            }

            // Fix FlagImage - disable raycast so it doesn't block buttons below
            GameObject flagImage = GameObject.Find("FlagImage");
            if (flagImage != null)
            {
                Image img = flagImage.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = false;
                    Debug.Log("Disabled raycastTarget on FlagImage");
                }
            }

            // Fix all text elements - disable raycast
            var allTexts = Object.FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var text in allTexts)
            {
                if (text.raycastTarget)
                {
                    text.raycastTarget = false;
                }
            }
            Debug.Log($"Disabled raycastTarget on {allTexts.Length} text elements");

            // Ensure buttons have raycast enabled
            GameObject[] buttonGOs = new GameObject[]
            {
                GameObject.Find("AnswerButton1"),
                GameObject.Find("AnswerButton2"),
                GameObject.Find("AnswerButton3"),
                GameObject.Find("AnswerButton4")
            };

            foreach (var btnGO in buttonGOs)
            {
                if (btnGO != null)
                {
                    Image btnImage = btnGO.GetComponent<Image>();
                    Button btn = btnGO.GetComponent<Button>();

                    if (btnImage != null)
                    {
                        btnImage.raycastTarget = true;
                        EditorUtility.SetDirty(btnImage);
                    }

                    if (btn != null)
                    {
                        btn.interactable = true;
                        EditorUtility.SetDirty(btn);
                    }

                    Debug.Log($"Ensured {btnGO.name} has raycastTarget=true and interactable=true");
                }
            }

            // Check Canvas has GraphicRaycaster
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    canvas.gameObject.AddComponent<GraphicRaycaster>();
                    Debug.Log("Added GraphicRaycaster to Canvas");
                }
            }

            // Save
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("=== Raycast issues fixed! ===");
        }
    }
}
#endif
