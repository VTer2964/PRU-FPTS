#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CountryGuessGame.Data;
using CountryGuessGame.Core;
using CountryGuessGame.Managers;
using System.IO;
using System.Collections.Generic;

namespace CountryGuessGame.Editor
{
    /// <summary>
    /// Automatic setup for Country Guess Game - runs all setup steps in sequence
    /// </summary>
    public class CountryGameAutoSetup : EditorWindow
    {
        [MenuItem("Country Guess Game/Auto Setup (All Steps)")]
        public static void RunAutoSetup()
        {
            // Step 1: Import Sprites
            ImportFlagSprites();

            // Step 2: Create Country Database
            var database = CreateCountryDatabase();

            // Step 3: Populate Countries
            if (database != null)
            {
                PopulateCountries(database);
            }

            // Step 4: Create Sample Levels
            var levels = CreateSampleLevels();

            // Step 5-8: Setup Scene
            SetupGameScene(database, levels);

            Debug.Log("=== Country Guess Game Auto Setup Complete! ===");
            EditorUtility.DisplayDialog("Setup Complete",
                "Country Guess Game has been set up successfully!\n\n" +
                "- 17 flag sprites imported\n" +
                "- Country Database created\n" +
                "- 17 countries populated\n" +
                "- 3 levels created\n" +
                "- Game scene configured\n\n" +
                "Press Play to test the game!",
                "OK");
        }

        [MenuItem("Country Guess Game/Step 1 - Import Flag Sprites")]
        public static void ImportFlagSprites()
        {
            string flagsPath = "Assets/CountryGuessGame/Sprites/Flags";
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { flagsPath });

            int count = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    bool needsReimport = false;

                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        needsReimport = true;
                    }

                    if (importer.spriteImportMode != SpriteImportMode.Single)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
                        needsReimport = true;
                    }

                    if (needsReimport)
                    {
                        importer.SaveAndReimport();
                        count++;
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Imported {count} flag sprites as Sprite (2D and UI)");
        }

        [MenuItem("Country Guess Game/Step 2 - Create Country Database")]
        public static CountryDatabase CreateCountryDatabase()
        {
            string path = "Assets/CountryGuessGame/Data";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/CountryGuessGame", "Data");
            }

            string assetPath = $"{path}/CountryDatabase.asset";

            // Check if already exists
            CountryDatabase existing = AssetDatabase.LoadAssetAtPath<CountryDatabase>(assetPath);
            if (existing != null)
            {
                Debug.Log("CountryDatabase already exists, using existing one.");
                Selection.activeObject = existing;
                return existing;
            }

            CountryDatabase database = ScriptableObject.CreateInstance<CountryDatabase>();
            AssetDatabase.CreateAsset(database, assetPath);
            AssetDatabase.SaveAssets();

            Selection.activeObject = database;
            Debug.Log("Created CountryDatabase.asset");

            return database;
        }

        public static void PopulateCountries(CountryDatabase database)
        {
            if (database == null) return;

            // Clear existing countries
            database.AllCountries.Clear();

            var countriesData = new[]
            {
                new { NameEn = "Vietnam", NameVi = "Việt Nam", Diff = 1, Region = "Asia", Capital = "Hanoi", FlagFile = "vietnam_flag" },
                new { NameEn = "United States", NameVi = "Hoa Kỳ", Diff = 1, Region = "Americas", Capital = "Washington D.C.", FlagFile = "usa_flag" },
                new { NameEn = "Japan", NameVi = "Nhật Bản", Diff = 1, Region = "Asia", Capital = "Tokyo", FlagFile = "japan_flag" },
                new { NameEn = "China", NameVi = "Trung Quốc", Diff = 1, Region = "Asia", Capital = "Beijing", FlagFile = "china_flag" },
                new { NameEn = "South Korea", NameVi = "Hàn Quốc", Diff = 2, Region = "Asia", Capital = "Seoul", FlagFile = "south_korea_flag" },
                new { NameEn = "Thailand", NameVi = "Thái Lan", Diff = 1, Region = "Asia", Capital = "Bangkok", FlagFile = "thailand_flag" },
                new { NameEn = "Singapore", NameVi = "Singapore", Diff = 2, Region = "Asia", Capital = "Singapore", FlagFile = "singapore_flag" },
                new { NameEn = "Malaysia", NameVi = "Malaysia", Diff = 2, Region = "Asia", Capital = "Kuala Lumpur", FlagFile = "malaysia_flag" },
                new { NameEn = "Indonesia", NameVi = "Indonesia", Diff = 1, Region = "Asia", Capital = "Jakarta", FlagFile = "indonesia_flag" },
                new { NameEn = "Philippines", NameVi = "Philippines", Diff = 2, Region = "Asia", Capital = "Manila", FlagFile = "philippines_flag" },
                new { NameEn = "United Kingdom", NameVi = "Vương quốc Anh", Diff = 1, Region = "Europe", Capital = "London", FlagFile = "uk_flag" },
                new { NameEn = "France", NameVi = "Pháp", Diff = 1, Region = "Europe", Capital = "Paris", FlagFile = "france_flag" },
                new { NameEn = "Germany", NameVi = "Đức", Diff = 1, Region = "Europe", Capital = "Berlin", FlagFile = "germany_flag" },
                new { NameEn = "Italy", NameVi = "Ý", Diff = 1, Region = "Europe", Capital = "Rome", FlagFile = "italy_flag" },
                new { NameEn = "Spain", NameVi = "Tây Ban Nha", Diff = 1, Region = "Europe", Capital = "Madrid", FlagFile = "spain_flag" },
                new { NameEn = "Brazil", NameVi = "Brazil", Diff = 1, Region = "Americas", Capital = "Brasilia", FlagFile = "brazil_flag" },
                new { NameEn = "Argentina", NameVi = "Argentina", Diff = 2, Region = "Americas", Capital = "Buenos Aires", FlagFile = "argentina_flag" },
            };

            string basePath = "Assets/CountryGuessGame/Data/Countries";
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                AssetDatabase.CreateFolder("Assets/CountryGuessGame/Data", "Countries");
            }

            foreach (var countryInfo in countriesData)
            {
                CountryData country = ScriptableObject.CreateInstance<CountryData>();
                country.Setup(
                    countryInfo.NameEn,
                    countryInfo.NameVi,
                    countryInfo.Diff,
                    countryInfo.Region,
                    countryInfo.Capital
                );

                // Find and assign flag sprite
                string[] guids = AssetDatabase.FindAssets($"{countryInfo.FlagFile} t:Texture2D", new[] { "Assets/CountryGuessGame/Sprites/Flags" });
                if (guids.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    foreach (var obj in assets)
                    {
                        if (obj is Sprite sprite)
                        {
                            country.SetFlagSprite(sprite);
                            break;
                        }
                    }
                }

                string countryAssetPath = $"{basePath}/{countryInfo.NameEn.Replace(" ", "")}.asset";

                // Delete existing if present
                if (File.Exists(countryAssetPath))
                {
                    AssetDatabase.DeleteAsset(countryAssetPath);
                }

                AssetDatabase.CreateAsset(country, countryAssetPath);
                database.AddCountry(country);
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Populated {countriesData.Length} countries with flags");
        }

        [MenuItem("Country Guess Game/Step 4 - Create Sample Levels")]
        public static CountryLevelData[] CreateSampleLevels()
        {
            string path = "Assets/CountryGuessGame/Data";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/CountryGuessGame", "Data");
            }

            var levels = new CountryLevelData[3];

            // Level 1 - Easy
            levels[0] = CreateOrGetLevel(path, "Level1", "Easy", 1, 5, 30f, 100, 50, 10);

            // Level 2 - Medium
            levels[1] = CreateOrGetLevel(path, "Level2", "Medium", 2, 7, 25f, 150, 75, 15);

            // Level 3 - Hard
            levels[2] = CreateOrGetLevel(path, "Level3", "Hard", 3, 10, 20f, 200, 100, 20);

            AssetDatabase.SaveAssets();
            Debug.Log("Created 3 level assets (Easy, Medium, Hard)");

            return levels;
        }

        private static CountryLevelData CreateOrGetLevel(string basePath, string fileName, string displayName,
            int levelNumber, int questionCount, float timePerQuestion, int baseScore, int timeBonus, int streakBonus)
        {
            string assetPath = $"{basePath}/{fileName}.asset";

            CountryLevelData level = AssetDatabase.LoadAssetAtPath<CountryLevelData>(assetPath);
            if (level != null)
            {
                return level;
            }

            level = ScriptableObject.CreateInstance<CountryLevelData>();

            // Use SerializedObject to set private fields
            AssetDatabase.CreateAsset(level, assetPath);

            SerializedObject so = new SerializedObject(level);
            so.FindProperty("levelName").stringValue = displayName;
            so.FindProperty("levelNumber").intValue = levelNumber;
            so.FindProperty("questionCount").intValue = questionCount;
            so.FindProperty("timePerQuestion").floatValue = timePerQuestion;
            so.FindProperty("pointsPerCorrect").intValue = baseScore;
            so.FindProperty("timeBonusMultiplier").floatValue = timeBonus;
            so.FindProperty("maxDifficulty").intValue = levelNumber; // Difficulty matches level number
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(level);

            return level;
        }

        public static void SetupGameScene(CountryDatabase database, CountryLevelData[] levels)
        {
            // Find or create Canvas
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // Create managers
            var gameManager = CreateManager<CountryGameManager>("CountryGameManager");
            var scoreManager = CreateManager<CountryScoreManager>("CountryScoreManager");
            var levelController = CreateManager<CountryLevelController>("CountryLevelController");
            var matchingController = CreateManager<CountryMatchingController>("CountryMatchingController");
            var uiManager = CreateManager<CountryUIManager>("CountryUIManager");

            // Create UI Panels
            GameObject gamePanel = CreateGamePanel(canvas.transform);
            GameObject resultPanel = CreateResultPanel(canvas.transform);

            // Wire up components using SerializedObject for proper editor integration
            WireUpLevelController(levelController, database, levels, matchingController);
            WireUpMatchingController(matchingController, database, levels.Length > 0 ? levels[0] : null, gamePanel);
            WireUpUIManager(uiManager, gamePanel, resultPanel);

            Debug.Log("Game scene setup complete with all managers and UI");
        }

        private static T CreateManager<T>(string name) where T : MonoBehaviour
        {
            T existing = Object.FindFirstObjectByType<T>();
            if (existing != null)
            {
                return existing;
            }

            GameObject go = new GameObject(name);
            return go.AddComponent<T>();
        }

        private static GameObject CreateGamePanel(Transform parent)
        {
            // Check if already exists
            Transform existing = parent.Find("GamePanel");
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject panel = CreatePanel(parent, "GamePanel");
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Create UI elements
            CreateTMPText(panel.transform, "LevelText", new Vector2(0, 200), "Level 1");
            CreateTMPText(panel.transform, "ScoreText", new Vector2(0, 160), "Score: 0");
            CreateTMPText(panel.transform, "QuestionCounterText", new Vector2(0, 120), "1/5");
            CreateTMPText(panel.transform, "TimerText", new Vector2(0, 80), "30");

            // Flag Image
            GameObject flagGO = new GameObject("FlagImage");
            flagGO.transform.SetParent(panel.transform, false);
            Image flagImage = flagGO.AddComponent<Image>();
            flagGO.AddComponent<FlagDisplay>();
            RectTransform flagRT = flagGO.GetComponent<RectTransform>();
            flagRT.sizeDelta = new Vector2(400, 300);
            flagRT.anchoredPosition = new Vector2(0, -50);

            // Answers Container
            GameObject answersContainer = new GameObject("AnswersContainer");
            answersContainer.transform.SetParent(panel.transform, false);
            RectTransform acRT = answersContainer.AddComponent<RectTransform>();
            acRT.sizeDelta = new Vector2(600, 200);
            acRT.anchoredPosition = new Vector2(0, -280);

            UnityEngine.UI.VerticalLayoutGroup vlg = answersContainer.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Create 4 answer buttons
            for (int i = 0; i < 4; i++)
            {
                CreateAnswerButton(answersContainer.transform, $"AnswerButton{i + 1}");
            }

            return panel;
        }

        private static GameObject CreateResultPanel(Transform parent)
        {
            Transform existing = parent.Find("ResultPanel");
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject panel = CreatePanel(parent, "ResultPanel");
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Set background color darker
            Image img = panel.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.9f);

            CreateTMPText(panel.transform, "MedalText", new Vector2(0, 150), "GOLD MEDAL!");

            GameObject medalImageGO = new GameObject("MedalImage");
            medalImageGO.transform.SetParent(panel.transform, false);
            Image medalImg = medalImageGO.AddComponent<Image>();
            RectTransform medalRT = medalImageGO.GetComponent<RectTransform>();
            medalRT.sizeDelta = new Vector2(100, 100);
            medalRT.anchoredPosition = new Vector2(0, 50);

            CreateTMPText(panel.transform, "FinalScoreText", new Vector2(0, -50), "Final Score: 500");
            CreateTMPText(panel.transform, "AccuracyText", new Vector2(0, -100), "Accuracy: 80%");
            CreateTMPText(panel.transform, "StatsText", new Vector2(0, -150), "4/5 Correct");

            CreateUIButton(panel.transform, "RetryButton", new Vector2(-100, -250), "Retry");
            CreateUIButton(panel.transform, "NextLevelButton", new Vector2(100, -250), "Next Level");

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);

            return panel;
        }

        private static TextMeshProUGUI CreateTMPText(Transform parent, string name, Vector2 position, string text)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 50);
            rt.anchoredPosition = position;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return tmp;
        }

        private static void CreateAnswerButton(Transform parent, string name)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            RectTransform rt = buttonGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 50);

            Image img = buttonGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.4f, 0.8f, 1f);

            Button button = buttonGO.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.4f, 0.8f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f, 1f);
            colors.pressedColor = new Color(0.1f, 0.3f, 0.7f, 1f);
            button.colors = colors;

            // Add AnswerButton component
            buttonGO.AddComponent<AnswerButton>();

            // Create text child
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "Answer";
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        private static void CreateUIButton(Transform parent, string name, Vector2 position, string text)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            RectTransform rt = buttonGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 50);
            rt.anchoredPosition = position;

            Image img = buttonGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 0.2f, 1f);

            Button button = buttonGO.AddComponent<Button>();

            // Create text child
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        private static void WireUpLevelController(CountryLevelController controller, CountryDatabase database,
            CountryLevelData[] levels, CountryMatchingController matchingController)
        {
            SerializedObject so = new SerializedObject(controller);

            SerializedProperty allLevelsProp = so.FindProperty("allLevels");
            if (allLevelsProp != null)
            {
                allLevelsProp.ClearArray();
                for (int i = 0; i < levels.Length; i++)
                {
                    allLevelsProp.InsertArrayElementAtIndex(i);
                    allLevelsProp.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
                }
            }

            SerializedProperty dbProp = so.FindProperty("countryDatabase");
            if (dbProp != null)
            {
                dbProp.objectReferenceValue = database;
            }

            SerializedProperty mcProp = so.FindProperty("matchingController");
            if (mcProp != null)
            {
                mcProp.objectReferenceValue = matchingController;
            }

            so.ApplyModifiedProperties();
        }

        private static void WireUpMatchingController(CountryMatchingController controller, CountryDatabase database,
            CountryLevelData level, GameObject gamePanel)
        {
            SerializedObject so = new SerializedObject(controller);

            SerializedProperty dbProp = so.FindProperty("countryDatabase");
            if (dbProp != null)
            {
                dbProp.objectReferenceValue = database;
            }

            SerializedProperty levelProp = so.FindProperty("currentLevelData");
            if (levelProp != null)
            {
                levelProp.objectReferenceValue = level;
            }

            // Find FlagDisplay
            FlagDisplay flagDisplay = gamePanel.GetComponentInChildren<FlagDisplay>();
            SerializedProperty flagProp = so.FindProperty("flagDisplay");
            if (flagProp != null && flagDisplay != null)
            {
                flagProp.objectReferenceValue = flagDisplay;
            }

            // Find Answer Buttons
            AnswerButton[] buttons = gamePanel.GetComponentsInChildren<AnswerButton>();
            SerializedProperty buttonsProp = so.FindProperty("answerButtons");
            if (buttonsProp != null)
            {
                buttonsProp.ClearArray();
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttonsProp.InsertArrayElementAtIndex(i);
                    buttonsProp.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
                }
            }

            so.ApplyModifiedProperties();
        }

        private static void WireUpUIManager(CountryUIManager uiManager, GameObject gamePanel, GameObject resultPanel)
        {
            SerializedObject so = new SerializedObject(uiManager);

            // Game Panel references
            SetProperty(so, "gamePanel", gamePanel);
            SetProperty(so, "levelText", FindTMPChild(gamePanel, "LevelText"));
            SetProperty(so, "scoreText", FindTMPChild(gamePanel, "ScoreText"));
            SetProperty(so, "questionCounterText", FindTMPChild(gamePanel, "QuestionCounterText"));
            SetProperty(so, "timerText", FindTMPChild(gamePanel, "TimerText"));

            // Result Panel references
            SetProperty(so, "resultPanel", resultPanel);
            SetProperty(so, "medalText", FindTMPChild(resultPanel, "MedalText"));
            SetProperty(so, "medalImage", resultPanel.transform.Find("MedalImage")?.GetComponent<Image>());
            SetProperty(so, "finalScoreText", FindTMPChild(resultPanel, "FinalScoreText"));
            SetProperty(so, "accuracyText", FindTMPChild(resultPanel, "AccuracyText"));
            SetProperty(so, "statsText", FindTMPChild(resultPanel, "StatsText"));
            SetProperty(so, "retryButton", resultPanel.transform.Find("RetryButton")?.GetComponent<Button>());
            SetProperty(so, "nextLevelButton", resultPanel.transform.Find("NextLevelButton")?.GetComponent<Button>());

            so.ApplyModifiedProperties();
        }

        private static void SetProperty(SerializedObject so, string propName, Object value)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop != null && value != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        private static TextMeshProUGUI FindTMPChild(GameObject parent, string name)
        {
            Transform child = parent.transform.Find(name);
            return child?.GetComponent<TextMeshProUGUI>();
        }
    }
}
#endif
