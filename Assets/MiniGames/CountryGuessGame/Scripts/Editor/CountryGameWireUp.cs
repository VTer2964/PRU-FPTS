#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CountryGuessGame.Data;
using CountryGuessGame.Core;
using CountryGuessGame.Managers;

namespace CountryGuessGame.Editor
{
    public class CountryGameWireUp : MonoBehaviour
    {
        [MenuItem("Country Guess Game/Wire Up All References")]
        public static void WireUpAllReferences()
        {
            // Load assets
            var database = AssetDatabase.LoadAssetAtPath<CountryDatabase>("Assets/CountryGuessGame/Data/CountryDatabase.asset");
            var level1 = AssetDatabase.LoadAssetAtPath<CountryLevelData>("Assets/CountryGuessGame/Data/Level1.asset");
            var level2 = AssetDatabase.LoadAssetAtPath<CountryLevelData>("Assets/CountryGuessGame/Data/Level2.asset");
            var level3 = AssetDatabase.LoadAssetAtPath<CountryLevelData>("Assets/CountryGuessGame/Data/Level3.asset");

            // Find managers
            var levelController = Object.FindFirstObjectByType<CountryLevelController>();
            var matchingController = Object.FindFirstObjectByType<CountryMatchingController>();
            var uiManager = Object.FindFirstObjectByType<CountryUIManager>();

            // Find UI elements
            var gamePanel = GameObject.Find("GamePanel");
            var resultPanel = GameObject.Find("ResultPanel");

            if (levelController != null)
            {
                SerializedObject so = new SerializedObject(levelController);

                var allLevels = so.FindProperty("allLevels");
                if (allLevels != null)
                {
                    allLevels.ClearArray();
                    allLevels.InsertArrayElementAtIndex(0);
                    allLevels.GetArrayElementAtIndex(0).objectReferenceValue = level1;
                    allLevels.InsertArrayElementAtIndex(1);
                    allLevels.GetArrayElementAtIndex(1).objectReferenceValue = level2;
                    allLevels.InsertArrayElementAtIndex(2);
                    allLevels.GetArrayElementAtIndex(2).objectReferenceValue = level3;
                }

                var dbProp = so.FindProperty("countryDatabase");
                if (dbProp != null) dbProp.objectReferenceValue = database;

                var mcProp = so.FindProperty("matchingController");
                if (mcProp != null) mcProp.objectReferenceValue = matchingController;

                so.ApplyModifiedProperties();
                Debug.Log("Wired up CountryLevelController");
            }

            if (matchingController != null && gamePanel != null)
            {
                SerializedObject so = new SerializedObject(matchingController);

                var dbProp = so.FindProperty("countryDatabase");
                if (dbProp != null) dbProp.objectReferenceValue = database;

                var levelProp = so.FindProperty("currentLevelData");
                if (levelProp != null) levelProp.objectReferenceValue = level1;

                // Find FlagDisplay
                var flagDisplay = gamePanel.GetComponentInChildren<FlagDisplay>();
                var flagProp = so.FindProperty("flagDisplay");
                if (flagProp != null && flagDisplay != null)
                    flagProp.objectReferenceValue = flagDisplay;

                // Find Answer Buttons
                var buttons = gamePanel.GetComponentsInChildren<AnswerButton>();
                var buttonsProp = so.FindProperty("answerButtons");
                if (buttonsProp != null && buttons.Length > 0)
                {
                    buttonsProp.ClearArray();
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        buttonsProp.InsertArrayElementAtIndex(i);
                        buttonsProp.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
                    }
                }

                so.ApplyModifiedProperties();
                Debug.Log($"Wired up CountryMatchingController with {buttons.Length} answer buttons");
            }

            if (uiManager != null && gamePanel != null && resultPanel != null)
            {
                SerializedObject so = new SerializedObject(uiManager);

                // Game Panel
                SetObjectRef(so, "gamePanel", gamePanel);
                SetObjectRef(so, "levelText", FindTMP(gamePanel, "LevelText"));
                SetObjectRef(so, "scoreText", FindTMP(gamePanel, "ScoreText"));
                SetObjectRef(so, "questionCounterText", FindTMP(gamePanel, "QuestionCounterText"));
                SetObjectRef(so, "timerText", FindTMP(gamePanel, "TimerText"));

                // Result Panel
                SetObjectRef(so, "resultPanel", resultPanel);
                SetObjectRef(so, "medalText", FindTMP(resultPanel, "MedalText"));
                SetObjectRef(so, "medalImage", resultPanel.transform.Find("MedalImage")?.GetComponent<Image>());
                SetObjectRef(so, "finalScoreText", FindTMP(resultPanel, "FinalScoreText"));
                SetObjectRef(so, "accuracyText", FindTMP(resultPanel, "AccuracyText"));
                SetObjectRef(so, "statsText", FindTMP(resultPanel, "StatsText"));
                SetObjectRef(so, "retryButton", resultPanel.transform.Find("RetryButton")?.GetComponent<Button>());
                SetObjectRef(so, "nextLevelButton", resultPanel.transform.Find("NextLevelButton")?.GetComponent<Button>());

                so.ApplyModifiedProperties();
                Debug.Log("Wired up CountryUIManager");
            }

            // Save scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("=== All references wired up successfully! ===");
        }

        private static void SetObjectRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop != null && value != null)
                prop.objectReferenceValue = value;
        }

        private static TextMeshProUGUI FindTMP(GameObject parent, string name)
        {
            var child = parent.transform.Find(name);
            return child?.GetComponent<TextMeshProUGUI>();
        }
    }
}
#endif
