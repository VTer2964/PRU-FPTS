using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using MemoryCardMatch;

namespace MemoryCardMatch.Editor
{
    public class MemoryMatchSetupWizard : EditorWindow
    {
        [MenuItem("Tools/MemoryCardMatch/Setup Full Game", false, 0)]
        public static void SetupFullGame()
        {
            EditorUtility.DisplayProgressBar("Memory Card Match Setup", "Starting...", 0f);
            try
            {
                // Step 1: Create sprites
                EditorUtility.DisplayProgressBar("Memory Card Match Setup", "Creating sprites...", 0.1f);
                CreateSprites();

                // Step 2: Create ScriptableObjects
                EditorUtility.DisplayProgressBar("Memory Card Match Setup", "Creating ScriptableObjects...", 0.3f);
                CreateScriptableObjects();

                // Step 3: Create Card Prefab
                EditorUtility.DisplayProgressBar("Memory Card Match Setup", "Creating Card Prefab...", 0.5f);
                CreateCardPrefab();

                // Step 4: Create Scene
                EditorUtility.DisplayProgressBar("Memory Card Match Setup", "Building Scene...", 0.7f);
                CreateScene();

                // Step 5: Add to build settings
                EditorUtility.DisplayProgressBar("Memory Card Match Setup", "Updating Build Settings...", 0.9f);
                AddSceneToBuildSettings();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayProgressBar("Memory Card Match Setup", "Done!", 1f);
                Debug.Log("[MemoryMatchSetup] ✅ Setup complete! Scene saved at Assets/MemoryCardMatch/Scenes/MemoryCardMatch.unity");
                EditorUtility.DisplayDialog("Setup Complete", "Memory Card Match setup completed!\n\nScene: Assets/MemoryCardMatch/Scenes/MemoryCardMatch.unity", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // -----------------------------------------------------------------------
        // STEP 1: Generate colored placeholder sprites for cards
        // -----------------------------------------------------------------------
        private static Dictionary<string, Sprite> createdSprites = new Dictionary<string, Sprite>();

        private static void CreateSprites()
        {
            string spritesPath = "Assets/MemoryCardMatch/Sprites";
            EnsureDirectory(spritesPath);

            // Card back – dark blue
            CreateColoredSprite("CardBack", new Color(0.12f, 0.18f, 0.35f), spritesPath);

            // Card face sprites – 12 distinct colors with emoji-like symbols
            var cardDefs = new (string name, Color color)[]
            {
                ("Card_Apple",     new Color(0.95f, 0.20f, 0.20f)),
                ("Card_Banana",    new Color(0.98f, 0.85f, 0.10f)),
                ("Card_Cherry",    new Color(0.80f, 0.05f, 0.30f)),
                ("Card_Grape",     new Color(0.55f, 0.10f, 0.80f)),
                ("Card_Orange",    new Color(0.98f, 0.55f, 0.10f)),
                ("Card_Watermelon",new Color(0.20f, 0.75f, 0.25f)),
                ("Card_Star",      new Color(0.99f, 0.85f, 0.00f)),
                ("Card_Heart",     new Color(0.95f, 0.20f, 0.45f)),
                ("Card_Diamond",   new Color(0.10f, 0.70f, 0.90f)),
                ("Card_Club",      new Color(0.15f, 0.55f, 0.20f)),
                ("Card_Sun",       new Color(0.99f, 0.75f, 0.10f)),
                ("Card_Moon",      new Color(0.70f, 0.80f, 0.95f)),
                ("Card_Fire",      new Color(0.99f, 0.40f, 0.05f)),
                ("Card_Ice",       new Color(0.60f, 0.85f, 0.99f)),
                ("Card_Lightning", new Color(0.99f, 0.95f, 0.00f)),
                ("Card_Flower",    new Color(0.99f, 0.50f, 0.70f)),
                ("Card_Tree",      new Color(0.10f, 0.65f, 0.15f)),
                ("Card_Crown",     new Color(0.99f, 0.80f, 0.10f)),
                ("Card_Shield",    new Color(0.30f, 0.45f, 0.80f)),
                ("Card_Sword",     new Color(0.65f, 0.65f, 0.70f)),
            };

            foreach (var (name, color) in cardDefs)
            {
                CreateColoredSprite(name, color, spritesPath);
            }

            Debug.Log($"[MemoryMatchSetup] Created {cardDefs.Length + 1} sprites.");
        }

        private static void CreateColoredSprite(string name, Color color, string path)
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size);

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Rounded rect mask
                    float cx = x - size * 0.5f;
                    float cy = y - size * 0.5f;
                    float radius = size * 0.42f;
                    float dist = Mathf.Sqrt(cx * cx + cy * cy);

                    if (dist < radius - 4)
                        pixels[y * size + x] = color;
                    else if (dist < radius)
                        pixels[y * size + x] = Color.Lerp(color, Color.clear, (dist - (radius - 4)) / 4f);
                    else
                        pixels[y * size + x] = Color.clear;
                }
            }

            // Draw a simple white border circle
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = x - size * 0.5f;
                    float cy = y - size * 0.5f;
                    float borderR = size * 0.40f;
                    float dist = Mathf.Sqrt(cx * cx + cy * cy);
                    if (dist > borderR - 3 && dist < borderR)
                    {
                        pixels[y * size + x] = new Color(1f, 1f, 1f, 0.5f);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            byte[] pngData = tex.EncodeToPNG();
            string filePath = $"{path}/{name}.png";
            File.WriteAllBytes(Application.dataPath.Replace("Assets", "") + filePath, pngData);
            Object.DestroyImmediate(tex);
        }

        // -----------------------------------------------------------------------
        // STEP 2: Create ScriptableObjects
        // -----------------------------------------------------------------------
        private static MemoryMatchSettings settingsAsset;
        private static CardDatabase dbAsset;

        private static void CreateScriptableObjects()
        {
            string soPath = "Assets/MemoryCardMatch/ScriptableObjects";
            EnsureDirectory(soPath);

            // ---- MemoryMatchSettings ----
            settingsAsset = AssetDatabase.LoadAssetAtPath<MemoryMatchSettings>($"{soPath}/MemoryMatchSettings.asset");
            if (settingsAsset == null)
            {
                settingsAsset = ScriptableObject.CreateInstance<MemoryMatchSettings>();
                AssetDatabase.CreateAsset(settingsAsset, $"{soPath}/MemoryMatchSettings.asset");
            }

            // ---- CardDatabase ----
            dbAsset = AssetDatabase.LoadAssetAtPath<CardDatabase>($"{soPath}/CardDatabase.asset");
            if (dbAsset == null)
            {
                dbAsset = ScriptableObject.CreateInstance<CardDatabase>();
                AssetDatabase.CreateAsset(dbAsset, $"{soPath}/CardDatabase.asset");
            }

            // Wait for asset import before loading sprites
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset("Assets/MemoryCardMatch/Sprites", ImportAssetOptions.ImportRecursive);
            AssetDatabase.Refresh();

            // ---- CardData instances ----
            string cardDataPath = $"{soPath}/Cards";
            EnsureDirectory(cardDataPath);

            var cardDefs = new (string name, string category, int id)[]
            {
                ("Apple",       "Fruits",   1),
                ("Banana",      "Fruits",   2),
                ("Cherry",      "Fruits",   3),
                ("Grape",       "Fruits",   4),
                ("Orange",      "Fruits",   5),
                ("Watermelon",  "Fruits",   6),
                ("Star",        "Shapes",   7),
                ("Heart",       "Shapes",   8),
                ("Diamond",     "Shapes",   9),
                ("Club",        "Shapes",  10),
                ("Sun",         "Nature",  11),
                ("Moon",        "Nature",  12),
                ("Fire",        "Elements",13),
                ("Ice",         "Elements",14),
                ("Lightning",   "Elements",15),
                ("Flower",      "Nature",  16),
                ("Tree",        "Nature",  17),
                ("Crown",       "RPG",     18),
                ("Shield",      "RPG",     19),
                ("Sword",       "RPG",     20),
            };

            dbAsset.cards = new List<CardData>();

            foreach (var (name, category, id) in cardDefs)
            {
                string assetFile = $"{cardDataPath}/CD_{name}.asset";
                CardData cd = AssetDatabase.LoadAssetAtPath<CardData>(assetFile);
                if (cd == null)
                {
                    cd = ScriptableObject.CreateInstance<CardData>();
                    AssetDatabase.CreateAsset(cd, assetFile);
                }

                cd.cardId = id;
                cd.cardName = name;
                cd.cardCategory = category;

                // Load sprite
                string spritePath = $"Assets/MemoryCardMatch/Sprites/Card_{name}.png";
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sp == null)
                {
                    // Try forcing texture importer to Sprite
                    TextureImporter ti = AssetImporter.GetAtPath(spritePath) as TextureImporter;
                    if (ti != null)
                    {
                        ti.textureType = TextureImporterType.Sprite;
                        ti.spriteImportMode = SpriteImportMode.Single;
                        ti.alphaIsTransparency = true;
                        AssetDatabase.ImportAsset(spritePath);
                        sp = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    }
                }
                cd.cardSprite = sp;

                EditorUtility.SetDirty(cd);
                dbAsset.cards.Add(cd);
            }

            EditorUtility.SetDirty(dbAsset);
            EditorUtility.SetDirty(settingsAsset);
            AssetDatabase.SaveAssets();
            Debug.Log($"[MemoryMatchSetup] Created CardDatabase with {dbAsset.cards.Count} cards.");
        }

        // -----------------------------------------------------------------------
        // STEP 3: Create Card Prefab
        // -----------------------------------------------------------------------
        private static GameObject cardPrefabGO;

        private static void CreateCardPrefab()
        {
            string prefabPath = "Assets/MemoryCardMatch/Prefabs/Card.prefab";
            EnsureDirectory("Assets/MemoryCardMatch/Prefabs");

            // Build the prefab hierarchy in memory
            GameObject root = new GameObject("Card");
            root.layer = LayerMask.NameToLayer("UI");

            // RectTransform on root
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(100, 100);

            // Button (for click detection)
            Button btn = root.AddComponent<Button>();
            btn.transition = Selectable.Transition.None; // we handle visuals ourselves

            // Image on root (optional background, transparent by default)
            Image rootImg = root.AddComponent<Image>();
            rootImg.color = Color.clear;
            rootImg.raycastTarget = true;

            // CardController
            CardController cc = root.AddComponent<CardController>();

            // ---- CardBack child ----
            GameObject cardBack = new GameObject("CardBack");
            cardBack.layer = LayerMask.NameToLayer("UI");
            cardBack.transform.SetParent(root.transform, false);
            RectTransform cbRT = cardBack.AddComponent<RectTransform>();
            cbRT.anchorMin = Vector2.zero;
            cbRT.anchorMax = Vector2.one;
            cbRT.offsetMin = Vector2.zero;
            cbRT.offsetMax = Vector2.zero;
            Image cbImg = cardBack.AddComponent<Image>();
            // Load CardBack sprite if available
            Sprite backSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/MemoryCardMatch/Sprites/CardBack.png");
            if (backSprite == null)
            {
                TextureImporter ti = AssetImporter.GetAtPath("Assets/MemoryCardMatch/Sprites/CardBack.png") as TextureImporter;
                if (ti != null) { ti.textureType = TextureImporterType.Sprite; ti.alphaIsTransparency = true; AssetDatabase.ImportAsset("Assets/MemoryCardMatch/Sprites/CardBack.png"); }
                backSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/MemoryCardMatch/Sprites/CardBack.png");
            }
            cbImg.sprite = backSprite;
            cbImg.color = Color.white;
            cbImg.preserveAspect = true;

            // ---- CardFront child ----
            GameObject cardFront = new GameObject("CardFront");
            cardFront.layer = LayerMask.NameToLayer("UI");
            cardFront.transform.SetParent(root.transform, false);
            RectTransform cfRT = cardFront.AddComponent<RectTransform>();
            cfRT.anchorMin = Vector2.zero;
            cfRT.anchorMax = Vector2.one;
            cfRT.offsetMin = new Vector2(4, 4);
            cfRT.offsetMax = new Vector2(-4, -4);
            Image cfImg = cardFront.AddComponent<Image>();
            cfImg.color = Color.white;
            cfImg.preserveAspect = true;
            cardFront.SetActive(false); // hidden at start

            // ---- MatchEffect child ----
            GameObject matchEffect = new GameObject("MatchEffect");
            matchEffect.layer = LayerMask.NameToLayer("UI");
            matchEffect.transform.SetParent(root.transform, false);
            RectTransform meRT = matchEffect.AddComponent<RectTransform>();
            meRT.anchorMin = Vector2.zero;
            meRT.anchorMax = Vector2.one;
            meRT.offsetMin = new Vector2(-4, -4);
            meRT.offsetMax = new Vector2(4, 4);
            Image meImg = matchEffect.AddComponent<Image>();
            meImg.color = new Color(1f, 0.9f, 0f, 0.3f); // golden glow
            meImg.raycastTarget = false;
            matchEffect.SetActive(false);

            // Wire CardController serialized fields via SerializedObject
            SerializedObject soCc = new SerializedObject(cc);
            soCc.FindProperty("cardBackImage").objectReferenceValue = cbImg;
            soCc.FindProperty("cardFrontImage").objectReferenceValue = cfImg;
            soCc.FindProperty("matchEffectObject").objectReferenceValue = matchEffect;
            soCc.ApplyModifiedPropertiesWithoutUndo();

            // Save prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            cardPrefabGO = savedPrefab;
            Debug.Log($"[MemoryMatchSetup] Card prefab created at {prefabPath}");
        }

        // -----------------------------------------------------------------------
        // STEP 4: Build Scene
        // -----------------------------------------------------------------------
        private static void CreateScene()
        {
            string scenePath = "Assets/MemoryCardMatch/Scenes/MemoryCardMatch.unity";
            EnsureDirectory("Assets/MemoryCardMatch/Scenes");

            // Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- Camera ----
            GameObject camGO = new GameObject("Main Camera");
            Camera cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.09f, 0.10f, 0.16f);
            cam.orthographic = true;
            camGO.tag = "MainCamera";

            // ---- Canvas ----
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // ---- EventSystem ----
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // ===== MAIN MENU PANEL =====
            GameObject mainMenuPanel = CreatePanel(canvasGO, "MainMenuPanel", new Color(0.09f, 0.10f, 0.16f, 0.98f));
            // Title
            CreateText(mainMenuPanel, "TitleText", "Memory Match", 72, FontStyles.Bold,
                new Color(0.98f, 0.85f, 0.20f), new Vector2(0, 250), new Vector2(900, 120));
            // Subtitle
            CreateText(mainMenuPanel, "SubtitleText", "Lật thẻ tìm cặp!", 36, FontStyles.Normal,
                new Color(0.75f, 0.85f, 1f), new Vector2(0, 160), new Vector2(700, 60));

            // Difficulty label
            CreateText(mainMenuPanel, "DifficultyLabel", "Chọn độ khó:", 32, FontStyles.Normal,
                Color.white, new Vector2(0, 60), new Vector2(600, 50));

            // Difficulty buttons row
            GameObject diffRow = CreateLayoutGroup(mainMenuPanel, "DifficultyButtons", new Vector2(0, -20), new Vector2(700, 90));
            HorizontalLayoutGroup hlg = diffRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;

            string[] diffNames = { "Dễ\n4×3", "Vừa\n4×4", "Khó\n5×4" };
            Color[] diffColors = { new Color(0.2f, 0.7f, 0.3f), new Color(0.2f, 0.5f, 0.9f), new Color(0.8f, 0.2f, 0.2f) };
            List<Button> diffButtons = new List<Button>();
            for (int i = 0; i < 3; i++)
            {
                Button db = CreateButton(diffRow, $"DiffBtn_{i}", diffNames[i], diffColors[i], 28);
                diffButtons.Add(db);
            }

            // Play Button
            Button playBtn = CreateButton(mainMenuPanel, "PlayButton", "CHƠI NGAY", new Color(0.20f, 0.75f, 0.35f), 48,
                new Vector2(0, -140), new Vector2(400, 110));

            // ===== GAME PANEL =====
            GameObject gamePanel = CreatePanel(canvasGO, "GamePanel", new Color(0.09f, 0.10f, 0.16f, 0.98f));

            // Top bar
            GameObject topBar = CreatePanel(gamePanel, "TopBar", new Color(0.12f, 0.14f, 0.22f, 0.95f),
                new Vector2(0, 880), new Vector2(1080, 110), TextAnchor.UpperCenter);
            SetAnchors(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -110), new Vector2(0, 0));

            HorizontalLayoutGroup topBarHlg = topBar.AddComponent<HorizontalLayoutGroup>();
            topBarHlg.padding = new RectOffset(20, 20, 10, 10);
            topBarHlg.spacing = 10;
            topBarHlg.childAlignment = TextAnchor.MiddleCenter;
            topBarHlg.childControlWidth = true;
            topBarHlg.childControlHeight = true;
            topBarHlg.childForceExpandWidth = true;

            TextMeshProUGUI scoreText = CreateTextInParent(topBar, "ScoreText", "Score: 0", 30, Color.white);
            TextMeshProUGUI timerText = CreateTextInParent(topBar, "TimerText", "02:00", 36, new Color(1f, 0.85f, 0.2f));
            TextMeshProUGUI movesText = CreateTextInParent(topBar, "MovesText", "Moves: 0", 30, Color.white);

            // Pause button (top right overlay)
            Button pauseBtn = CreateButton(gamePanel, "PauseButton", "⏸", new Color(0.3f, 0.3f, 0.4f), 36,
                new Vector2(480, -30), new Vector2(80, 80));
            SetAnchors(pauseBtn.gameObject, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-100, -100), new Vector2(-10, -10));

            // Combo text (center, hidden initially)
            TextMeshProUGUI comboText = CreateText(gamePanel, "ComboText", "Combo x2", 40, FontStyles.Bold,
                new Color(1f, 0.85f, 0.2f), new Vector2(0, 700), new Vector2(500, 70));
            comboText.gameObject.SetActive(false);

            // Card Grid container
            GameObject gridContainer = new GameObject("CardGridContainer");
            gridContainer.layer = LayerMask.NameToLayer("UI");
            gridContainer.transform.SetParent(gamePanel.transform, false);
            RectTransform gridContRT = gridContainer.AddComponent<RectTransform>();
            SetAnchors(gridContainer, new Vector2(0, 0), new Vector2(1, 1), new Vector2(20, 80), new Vector2(-20, -120));
            gridContainer.AddComponent<Image>().color = Color.clear;

            GameObject cardGrid = new GameObject("CardGrid");
            cardGrid.layer = LayerMask.NameToLayer("UI");
            cardGrid.transform.SetParent(gridContainer.transform, false);
            RectTransform cardGridRT = cardGrid.AddComponent<RectTransform>();
            cardGridRT.anchorMin = Vector2.zero;
            cardGridRT.anchorMax = Vector2.one;
            cardGridRT.offsetMin = Vector2.zero;
            cardGridRT.offsetMax = Vector2.zero;
            GridLayoutGroup glg = cardGrid.AddComponent<GridLayoutGroup>();
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4;
            glg.spacing = new Vector2(10, 10);
            glg.padding = new RectOffset(10, 10, 10, 10);
            glg.cellSize = new Vector2(120, 120);

            // Combo popup
            TextMeshProUGUI comboPopup = CreateText(gamePanel, "ComboPopup", "", 48, FontStyles.Bold,
                new Color(1f, 0.85f, 0.1f), Vector2.zero, new Vector2(500, 120));
            comboPopup.gameObject.SetActive(false);

            gamePanel.SetActive(false);

            // ===== PAUSE PANEL =====
            GameObject pausePanel = CreatePanel(canvasGO, "PausePanel", new Color(0, 0, 0, 0.85f));
            CreateText(pausePanel, "PauseTitleText", "TẠM DỪNG", 60, FontStyles.Bold,
                Color.white, new Vector2(0, 150), new Vector2(600, 100));
            Button resumeBtn = CreateButton(pausePanel, "ResumeButton", "Tiếp tục", new Color(0.2f, 0.7f, 0.3f), 40,
                new Vector2(0, 20), new Vector2(380, 90));
            Button restartBtnPause = CreateButton(pausePanel, "RestartButton", "Chơi lại", new Color(0.2f, 0.5f, 0.8f), 40,
                new Vector2(0, -100), new Vector2(380, 90));
            Button menuBtnPause = CreateButton(pausePanel, "MainMenuButton", "Menu chính", new Color(0.5f, 0.5f, 0.6f), 40,
                new Vector2(0, -210), new Vector2(380, 90));
            pausePanel.SetActive(false);

            // ===== GAME OVER PANEL =====
            GameObject gameOverPanel = CreatePanel(canvasGO, "GameOverPanel", new Color(0.12f, 0.05f, 0.05f, 0.96f));
            CreateText(gameOverPanel, "GameOverTitle", "HẾT GIỜ!", 72, FontStyles.Bold,
                new Color(1f, 0.3f, 0.3f), new Vector2(0, 280), new Vector2(700, 120));
            TextMeshProUGUI goScore = CreateText(gameOverPanel, "GOScoreText", "Score: 0", 40, FontStyles.Normal,
                Color.white, new Vector2(0, 140), new Vector2(600, 70));
            TextMeshProUGUI goMoves = CreateText(gameOverPanel, "GOMovesText", "Moves: 0", 36, FontStyles.Normal,
                new Color(0.8f, 0.8f, 0.8f), new Vector2(0, 60), new Vector2(600, 60));
            TextMeshProUGUI goMatches = CreateText(gameOverPanel, "GOMatchesText", "Pairs: 0 / 8", 36, FontStyles.Normal,
                new Color(0.8f, 0.8f, 0.8f), new Vector2(0, -10), new Vector2(600, 60));
            Button restartBtnGO = CreateButton(gameOverPanel, "RestartButton", "Chơi lại", new Color(0.2f, 0.6f, 0.9f), 42,
                new Vector2(0, -130), new Vector2(380, 90));
            Button menuBtnGO = CreateButton(gameOverPanel, "MainMenuButton", "Menu chính", new Color(0.5f, 0.5f, 0.6f), 42,
                new Vector2(0, -240), new Vector2(380, 90));
            gameOverPanel.SetActive(false);

            // ===== VICTORY PANEL =====
            GameObject victoryPanel = CreatePanel(canvasGO, "VictoryPanel", new Color(0.05f, 0.14f, 0.07f, 0.96f));
            CreateText(victoryPanel, "VictoryTitle", "CHIẾN THẮNG! 🎉", 64, FontStyles.Bold,
                new Color(0.98f, 0.85f, 0.1f), new Vector2(0, 300), new Vector2(800, 120));
            TextMeshProUGUI vicScore = CreateText(victoryPanel, "VicScoreText", "Score: 0", 40, FontStyles.Bold,
                new Color(0.98f, 0.85f, 0.1f), new Vector2(0, 160), new Vector2(600, 70));
            TextMeshProUGUI vicMoves = CreateText(victoryPanel, "VicMovesText", "Moves: 0", 36, FontStyles.Normal,
                Color.white, new Vector2(0, 80), new Vector2(600, 60));
            TextMeshProUGUI vicTime = CreateText(victoryPanel, "VicTimeText", "Time Left: 00:00", 36, FontStyles.Normal,
                new Color(0.6f, 0.9f, 1f), new Vector2(0, 10), new Vector2(600, 60));
            TextMeshProUGUI vicTimeBonus = CreateText(victoryPanel, "VicTimeBonusText", "Time Bonus: +0", 36, FontStyles.Normal,
                new Color(0.4f, 1f, 0.6f), new Vector2(0, -60), new Vector2(600, 60));
            Button restartBtnVic = CreateButton(victoryPanel, "RestartButton", "Chơi lại", new Color(0.2f, 0.7f, 0.3f), 42,
                new Vector2(0, -170), new Vector2(380, 90));
            Button menuBtnVic = CreateButton(victoryPanel, "MainMenuButton", "Menu chính", new Color(0.5f, 0.5f, 0.6f), 42,
                new Vector2(0, -280), new Vector2(380, 90));
            victoryPanel.SetActive(false);

            // ===== MANAGER GAMEOBJECTS =====
            // GameManager
            GameObject gmGO = new GameObject("GameManager");
            MemoryMatchGameManager gm = gmGO.AddComponent<MemoryMatchGameManager>();

            // UIManager
            GameObject uiGO = new GameObject("UIManager");
            MemoryMatchUIManager uiMgr = uiGO.AddComponent<MemoryMatchUIManager>();

            // GridSpawner
            GameObject spawnerGO = new GameObject("CardGridSpawner");
            CardGridSpawner spawner = spawnerGO.AddComponent<CardGridSpawner>();

            // ===== WIRE UP REFERENCES VIA SerializedObject =====

            // Load prefab reference
            CardController cardPrefabRef = AssetDatabase.LoadAssetAtPath<CardController>("Assets/MemoryCardMatch/Prefabs/Card.prefab");

            // Wire GridSpawner
            SerializedObject soSpawner = new SerializedObject(spawner);
            soSpawner.FindProperty("cardPrefab").objectReferenceValue = cardPrefabRef;
            soSpawner.FindProperty("gridLayoutGroup").objectReferenceValue = cardGrid.GetComponent<GridLayoutGroup>();
            soSpawner.FindProperty("cardDatabase").objectReferenceValue = dbAsset;
            soSpawner.ApplyModifiedPropertiesWithoutUndo();

            // Wire GameManager
            SerializedObject soGM = new SerializedObject(gm);
            soGM.FindProperty("settings").objectReferenceValue = settingsAsset;
            soGM.FindProperty("cardDatabase").objectReferenceValue = dbAsset;
            soGM.FindProperty("gridSpawner").objectReferenceValue = spawner;
            soGM.FindProperty("uiManager").objectReferenceValue = uiMgr;
            soGM.ApplyModifiedPropertiesWithoutUndo();

            // Wire UIManager
            SerializedObject soUI = new SerializedObject(uiMgr);
            soUI.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
            soUI.FindProperty("gamePanel").objectReferenceValue = gamePanel;
            soUI.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            soUI.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
            soUI.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;

            soUI.FindProperty("playButton").objectReferenceValue = playBtn;
            // Difficulty buttons array
            SerializedProperty diffBtnsArr = soUI.FindProperty("difficultyButtons");
            diffBtnsArr.arraySize = diffButtons.Count;
            for (int i = 0; i < diffButtons.Count; i++)
                diffBtnsArr.GetArrayElementAtIndex(i).objectReferenceValue = diffButtons[i];

            soUI.FindProperty("scoreText").objectReferenceValue = scoreText;
            soUI.FindProperty("timerText").objectReferenceValue = timerText;
            soUI.FindProperty("movesText").objectReferenceValue = movesText;
            soUI.FindProperty("comboText").objectReferenceValue = comboText;
            soUI.FindProperty("pauseButton").objectReferenceValue = pauseBtn;

            soUI.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
            soUI.FindProperty("restartButtonPause").objectReferenceValue = restartBtnPause;
            soUI.FindProperty("mainMenuButtonPause").objectReferenceValue = menuBtnPause;

            soUI.FindProperty("gameOverScoreText").objectReferenceValue = goScore;
            soUI.FindProperty("gameOverMovesText").objectReferenceValue = goMoves;
            soUI.FindProperty("gameOverMatchesText").objectReferenceValue = goMatches;
            soUI.FindProperty("restartButtonGameOver").objectReferenceValue = restartBtnGO;
            soUI.FindProperty("mainMenuButtonGameOver").objectReferenceValue = menuBtnGO;

            soUI.FindProperty("victoryScoreText").objectReferenceValue = vicScore;
            soUI.FindProperty("victoryMovesText").objectReferenceValue = vicMoves;
            soUI.FindProperty("victoryTimeText").objectReferenceValue = vicTime;
            soUI.FindProperty("victoryTimeBonusText").objectReferenceValue = vicTimeBonus;
            soUI.FindProperty("restartButtonVictory").objectReferenceValue = restartBtnVic;
            soUI.FindProperty("mainMenuButtonVictory").objectReferenceValue = menuBtnVic;

            soUI.FindProperty("comboPopupText").objectReferenceValue = comboPopup;
            soUI.ApplyModifiedPropertiesWithoutUndo();

            // Save scene
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[MemoryMatchSetup] Scene created and saved: {scenePath}");
        }

        // -----------------------------------------------------------------------
        // STEP 5: Add to Build Settings
        // -----------------------------------------------------------------------
        private static void AddSceneToBuildSettings()
        {
            string scenePath = "Assets/MemoryCardMatch/Scenes/MemoryCardMatch.unity";
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // Check if already exists
            foreach (var s in scenes)
            {
                if (s.path == scenePath)
                {
                    Debug.Log("[MemoryMatchSetup] Scene already in Build Settings.");
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[MemoryMatchSetup] Added scene to Build Settings at index {scenes.Count - 1}.");
        }

        // -----------------------------------------------------------------------
        // UI Helper Methods
        // -----------------------------------------------------------------------

        private static GameObject CreatePanel(GameObject parent, string name, Color bgColor,
            Vector2? anchoredPos = null, Vector2? size = null, TextAnchor? anchor = null)
        {
            GameObject go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            if (anchoredPos.HasValue) rt.anchoredPosition = anchoredPos.Value;
            if (size.HasValue) rt.sizeDelta = size.Value;
            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;
            return go;
        }

        private static TextMeshProUGUI CreateText(GameObject parent, string name, string text, int fontSize,
            FontStyles style, Color color, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;
            return tmp;
        }

        private static TextMeshProUGUI CreateTextInParent(GameObject parent, string name, string text, int fontSize, Color color)
        {
            GameObject go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            return tmp;
        }

        private static Button CreateButton(GameObject parent, string name, string label,
            Color bgColor, int fontSize, Vector2? pos = null, Vector2? size = null)
        {
            GameObject go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            if (pos.HasValue) rt.anchoredPosition = pos.Value;
            rt.sizeDelta = size ?? new Vector2(300, 80);
            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            Button btn = go.AddComponent<Button>();

            ColorBlock cb = btn.colors;
            cb.highlightedColor = bgColor * 1.2f;
            cb.pressedColor = bgColor * 0.8f;
            cb.normalColor = bgColor;
            btn.colors = cb;

            // Text child
            GameObject txtGO = new GameObject("Text");
            txtGO.layer = LayerMask.NameToLayer("UI");
            txtGO.transform.SetParent(go.transform, false);
            RectTransform txtRT = txtGO.AddComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;

            return btn;
        }

        private static GameObject CreateLayoutGroup(GameObject parent, string name, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go;
        }

        private static void SetAnchors(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }
    }
}
