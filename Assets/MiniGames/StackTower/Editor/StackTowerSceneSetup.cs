#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace StackTower
{
    /// <summary>
    /// Editor utility to scaffold the StackTower scene with all GameObjects,
    /// Camera, and UI hierarchy. Run once via the Unity menu.
    /// </summary>
    public static class StackTowerSceneSetup
    {
        private const string SCENE_SETUP_MENU = "StackTower/Setup Scene";
        private const string CREATE_LEVEL_DATA_MENU = "StackTower/Create Level Data Assets";
        private const string CREATE_SETTINGS_MENU = "StackTower/Create Settings Asset";

        // ── Scene Setup ──────────────────────────────────────────────────────

        [MenuItem(SCENE_SETUP_MENU)]
        public static void SetupScene()
        {
            if (!EditorUtility.DisplayDialog("StackTower Scene Setup",
                "This will create all GameObjects for the StackTower scene in the current scene. Continue?",
                "Yes", "Cancel"))
                return;

            // ── Root game manager ──────────────────────────────────────────
            GameObject root = new GameObject("StackTower_Root");

            // ── Camera ────────────────────────────────────────────────────
            GameObject camGO = new GameObject("MainCamera");
            camGO.tag = "MainCamera";
            camGO.transform.SetParent(root.transform);

            Camera cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 20f, 0f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            TowerCameraController cameraCtrl = camGO.AddComponent<TowerCameraController>();

            // ── Game Logic Objects ────────────────────────────────────────
            GameObject spawnerGO = new GameObject("BlockSpawner");
            spawnerGO.transform.SetParent(root.transform);
            BlockSpawner spawner = spawnerGO.AddComponent<BlockSpawner>();

            GameObject builderGO = new GameObject("TowerBuilder");
            builderGO.transform.SetParent(root.transform);
            TowerBuilder builder = builderGO.AddComponent<TowerBuilder>();

            GameObject effectsGO = new GameObject("Effects");
            effectsGO.transform.SetParent(root.transform);
            PerfectEffect perfectFx = effectsGO.AddComponent<PerfectEffect>();

            // ── UI Canvas ─────────────────────────────────────────────────
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            canvasGO.AddComponent<GraphicRaycaster>();

            // Event system
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }

            // Build UI panels
            GameObject mainMenuPanel    = CreatePanel(canvasGO.transform, "MainMenuPanel",    new Color(0.1f, 0.1f, 0.2f, 0.95f));
            GameObject levelSelectPanel = CreatePanel(canvasGO.transform, "LevelSelectPanel", new Color(0.1f, 0.1f, 0.2f, 0.95f));
            GameObject hudPanel         = CreatePanel(canvasGO.transform, "HUDPanel",         Color.clear);
            GameObject pausePanel       = CreatePanel(canvasGO.transform, "PausePanel",       new Color(0f, 0f, 0f, 0.7f));
            GameObject gameOverPanel    = CreatePanel(canvasGO.transform, "GameOverPanel",    new Color(0.8f, 0.1f, 0.1f, 0.9f));
            GameObject victoryPanel     = CreatePanel(canvasGO.transform, "VictoryPanel",     new Color(0.1f, 0.6f, 0.2f, 0.95f));

            // ── Main Menu ─────────────────────────────────────────────────
            CreateLabel(mainMenuPanel.transform, "TitleText", "🏗️ STACK TOWER", 60, new Vector2(0, 200));
            Button playBtn = CreateButton(mainMenuPanel.transform, "PlayButton", "▶ CHƠI NGAY", new Vector2(0, 0));
            playBtn.onClick.AddListener(() =>
            {
                if (StackTowerGameManager.Instance != null)
                    StackTowerGameManager.Instance.StartGame(0);
            });

            // ── HUD ───────────────────────────────────────────────────────
            // Floor text (top center)
            TextMeshProUGUI floorTMP = CreateLabel(hudPanel.transform, "FloorText", "Tầng: 0 / 10", 36,
                new Vector2(0, 380), new Vector2(600, 60));

            // Progress bar
            Slider progressBar = CreateSlider(hudPanel.transform, "ProgressBar", new Vector2(0, 320));

            // Level name (top left)
            TextMeshProUGUI levelNameTMP = CreateLabel(hudPanel.transform, "LevelNameText", "Level 1", 28,
                new Vector2(-300, 420), new Vector2(300, 50));

            // Combo text (center)
            TextMeshProUGUI comboTMP = CreateLabel(hudPanel.transform, "ComboText", "", 40,
                new Vector2(0, 100), new Vector2(500, 80));
            comboTMP.color = new Color(1f, 0.9f, 0.2f);

            // Pause button
            Button pauseBtn = CreateButton(hudPanel.transform, "PauseButton", "⏸", new Vector2(440, 420),
                new Vector2(80, 80));

            // ── Pause Panel ───────────────────────────────────────────────
            CreateLabel(pausePanel.transform, "PauseTitle", "PAUSE", 60, new Vector2(0, 100));
            Button resumeBtn = CreateButton(pausePanel.transform, "ResumeButton", "▶ Tiếp tục", new Vector2(0, 0));
            Button restartPauseBtn = CreateButton(pausePanel.transform, "RestartButton", "🔄 Chơi lại", new Vector2(0, -100));
            Button menuPauseBtn = CreateButton(pausePanel.transform, "MenuButton", "🏠 Menu", new Vector2(0, -200));

            resumeBtn.onClick.AddListener(() => StackTowerGameManager.Instance?.ResumeGame());
            restartPauseBtn.onClick.AddListener(() => StackTowerGameManager.Instance?.RestartLevel());
            menuPauseBtn.onClick.AddListener(() => StackTowerGameManager.Instance?.ReturnToMenu());
            pauseBtn.onClick.AddListener(() => StackTowerGameManager.Instance?.PauseGame());

            // ── Game Over Panel ───────────────────────────────────────────
            CreateLabel(gameOverPanel.transform, "GameOverTitle", "💀 GAME OVER", 56, new Vector2(0, 150));
            TextMeshProUGUI goFloorTMP = CreateLabel(gameOverPanel.transform, "GoFloorText", "Reached floor 0 / 10", 32, new Vector2(0, 50));
            Button retryBtn  = CreateButton(gameOverPanel.transform, "RetryButton",  "🔄 Thử lại", new Vector2(0, -80));
            Button goMenuBtn = CreateButton(gameOverPanel.transform, "MenuButton2",  "🏠 Menu",    new Vector2(0, -180));

            // ── Victory Panel ─────────────────────────────────────────────
            CreateLabel(victoryPanel.transform, "VictoryTitle", "🎉 CHIẾN THẮNG!", 56, new Vector2(0, 250));

            // Result panel component
            GameObject resultPanelGO = new GameObject("ResultContent");
            resultPanelGO.transform.SetParent(victoryPanel.transform, false);
            var resultRT = resultPanelGO.AddComponent<RectTransform>();
            resultRT.anchoredPosition = new Vector2(0, -50);
            resultRT.sizeDelta = new Vector2(600, 400);

            ResultPanel resultPanelComp = resultPanelGO.AddComponent<ResultPanel>();

            // Stars
            Image[] starImgs = CreateStarRow(victoryPanel.transform, new Vector2(0, 150));

            TextMeshProUGUI vpFloorTMP  = CreateLabel(victoryPanel.transform, "VPFloorText",   "Đã xây: 0 tầng",         30, new Vector2(0, 60));
            TextMeshProUGUI vpPerfTMP   = CreateLabel(victoryPanel.transform, "VPPerfectText", "Perfect: 0 / 0 (0%)",    28, new Vector2(0, 10));
            TextMeshProUGUI vpStarTMP   = CreateLabel(victoryPanel.transform, "VPStarText",    "1 ⭐",                    40, new Vector2(0, -50));
            Button nextBtn   = CreateButton(victoryPanel.transform, "NextButton",   "▶ Level tiếp theo", new Vector2(0, -140));
            Button vpRetry   = CreateButton(victoryPanel.transform, "VPRetry",      "🔄 Thử lại",       new Vector2(0, -230));
            Button vpMenu    = CreateButton(victoryPanel.transform, "VPMenu",       "🏠 Menu",          new Vector2(0, -310));

            // Level select panel
            LevelSelectUI lsUI = levelSelectPanel.AddComponent<LevelSelectUI>();

            // ── Flash overlay (Perfect effect) ────────────────────────────
            GameObject flashGO = new GameObject("PerfectFlashOverlay");
            flashGO.transform.SetParent(canvasGO.transform, false);
            var flashRT = flashGO.AddComponent<RectTransform>();
            flashRT.anchorMin = Vector2.zero;
            flashRT.anchorMax = Vector2.one;
            flashRT.offsetMin = flashRT.offsetMax = Vector2.zero;
            Image flashImg = flashGO.AddComponent<Image>();
            flashImg.color = new Color(1f, 1f, 0.6f, 0f);
            flashImg.raycastTarget = false;
            flashGO.SetActive(false);

            // Perfect text
            GameObject perfTextGO = new GameObject("PerfectText");
            perfTextGO.transform.SetParent(canvasGO.transform, false);
            var perfTMP = perfTextGO.AddComponent<TextMeshProUGUI>();
            perfTMP.alignment = TextAlignmentOptions.Center;
            perfTMP.fontSize = 48;
            perfTMP.fontStyle = FontStyles.Bold;
            perfTextGO.SetActive(false);

            // ── Game Manager ──────────────────────────────────────────────
            GameObject gmGO = new GameObject("StackTowerGameManager");
            gmGO.transform.SetParent(root.transform);
            StackTowerGameManager gm = gmGO.AddComponent<StackTowerGameManager>();

            // ── UI Manager ────────────────────────────────────────────────
            GameObject uiManagerGO = new GameObject("StackTowerUIManager");
            uiManagerGO.transform.SetParent(root.transform);
            StackTowerUIManager uiMgr = uiManagerGO.AddComponent<StackTowerUIManager>();

            // ── Wire up serialized fields via SerializedObject ────────────
            WireUpUIManager(uiMgr, mainMenuPanel, hudPanel, pausePanel, gameOverPanel, victoryPanel,
                lsUI, null /* resultPanelComp set below */,
                floorTMP, progressBar, comboTMP, levelNameTMP, goFloorTMP, retryBtn, goMenuBtn);

            // Wire result panel
            SerializedObject soResult = new SerializedObject(resultPanelComp);
            soResult.FindProperty("starImages").arraySize = 3;
            for (int i = 0; i < 3 && i < starImgs.Length; i++)
                soResult.FindProperty("starImages").GetArrayElementAtIndex(i).objectReferenceValue = starImgs[i];
            soResult.FindProperty("retryButton").objectReferenceValue   = vpRetry;
            soResult.FindProperty("nextLevelButton").objectReferenceValue = nextBtn;
            soResult.FindProperty("menuButton").objectReferenceValue    = vpMenu;
            soResult.FindProperty("floorText").objectReferenceValue     = vpFloorTMP;
            soResult.FindProperty("perfectText").objectReferenceValue   = vpPerfTMP;
            soResult.FindProperty("starCountText").objectReferenceValue = vpStarTMP;
            soResult.ApplyModifiedProperties();

            // Wire result panel to victory panel show call
            SerializedObject soUI = new SerializedObject(uiMgr);
            soUI.FindProperty("resultPanel").objectReferenceValue = resultPanelComp;
            soUI.FindProperty("levelSelectUI").objectReferenceValue = lsUI;
            soUI.ApplyModifiedProperties();

            // Wire PerfectEffect
            SerializedObject soPerfect = new SerializedObject(perfectFx);
            soPerfect.FindProperty("perfectText").objectReferenceValue  = perfTMP;
            soPerfect.FindProperty("flashOverlay").objectReferenceValue = flashImg;
            soPerfect.ApplyModifiedProperties();

            // Wire GameManager
            SerializedObject soGM = new SerializedObject(gm);
            soGM.FindProperty("blockSpawner").objectReferenceValue   = spawner;
            soGM.FindProperty("towerBuilder").objectReferenceValue   = builder;
            soGM.FindProperty("cameraController").objectReferenceValue = cameraCtrl;
            soGM.FindProperty("uiManager").objectReferenceValue      = uiMgr;
            soGM.FindProperty("perfectEffect").objectReferenceValue  = perfectFx;
            soGM.ApplyModifiedProperties();

            // De-activate panels except main menu
            hudPanel.SetActive(false);
            pausePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            victoryPanel.SetActive(false);
            levelSelectPanel.SetActive(false);

            Debug.Log("[StackTower] Scene setup complete! " +
                      "Assign LevelData assets and StackTowerSettings in StackTowerGameManager inspector.");

            Selection.activeGameObject = gmGO;
            EditorUtility.DisplayDialog("Done",
                "Scene setup complete!\n\n" +
                "Next steps:\n" +
                "1. Run 'StackTower > Create Level Data Assets'\n" +
                "2. Run 'StackTower > Create Settings Asset'\n" +
                "3. Assign them to StackTowerGameManager in Inspector\n" +
                "4. Save the scene to Assets/StackTower/Scenes/StackTowerScene.unity",
                "OK");
        }

        // ── Level Data Asset Creation ────────────────────────────────────────

        [MenuItem(CREATE_LEVEL_DATA_MENU)]
        public static void CreateLevelDataAssets()
        {
            string folder = "Assets/StackTower/Settings";
            System.IO.Directory.CreateDirectory(folder);

            // Level 1 — Tutorial
            CreateLevelAsset(folder, 1, "Level 1: Tập làm quen",   10, 3.0f, 2.0f, 10f,  0.15f, 0.10f, 5);
            // Level 2 — Normal
            CreateLevelAsset(folder, 2, "Level 2: Vào nghề",       15, 2.5f, 2.5f, 12f,  0.13f, 0.15f, 5);
            // Level 3 — Challenge
            CreateLevelAsset(folder, 3, "Level 3: Thử thách",      20, 2.5f, 3.5f, 14f,  0.12f, 0.20f, 4);
            // Level 4 — Hard
            CreateLevelAsset(folder, 4, "Level 4: Chuyên gia",     25, 2.0f, 4.5f, 16f,  0.10f, 0.25f, 4);
            // Level 5 — Boss
            CreateLevelAsset(folder, 5, "Level 5: Huyền thoại",    30, 2.0f, 5.5f, 18f,  0.08f, 0.30f, 3);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done", "Level Data assets created in Assets/StackTower/Settings/", "OK");
        }

        [MenuItem(CREATE_SETTINGS_MENU)]
        public static void CreateSettingsAsset()
        {
            string folder = "Assets/StackTower/Settings";
            System.IO.Directory.CreateDirectory(folder);
            string path = $"{folder}/StackTowerSettings.asset";

            if (AssetDatabase.LoadAssetAtPath<StackTowerSettings>(path) != null)
            {
                EditorUtility.DisplayDialog("Already Exists", $"Settings asset already exists at:\n{path}", "OK");
                return;
            }

            var settings = ScriptableObject.CreateInstance<StackTowerSettings>();
            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = settings;
            EditorUtility.DisplayDialog("Done", $"Settings asset created at:\n{path}", "OK");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void CreateLevelAsset(
            string folder, int num, string name,
            int targetFloors, float size, float speed, float maxSpeed,
            float perfectThresh, float speedInc, int speedInterval)
        {
            string path = $"{folder}/Level{num}Data.asset";
            if (AssetDatabase.LoadAssetAtPath<LevelData>(path) != null)
            {
                Debug.Log($"[StackTower] {path} already exists — skipping.");
                return;
            }

            var ld = ScriptableObject.CreateInstance<LevelData>();
            ld.levelNumber         = num;
            ld.levelName           = name;
            ld.targetFloors        = targetFloors;
            ld.initialBlockSize    = size;
            ld.initialMoveSpeed    = speed;
            ld.maxMoveSpeed        = maxSpeed;
            ld.perfectThreshold    = perfectThresh;
            ld.speedIncreaseAmount = speedInc;
            ld.speedIncreaseInterval = speedInterval;

            // Assign default colors per level
            ld.blockColors = num switch
            {
                1 => new[] { new Color(0.3f, 0.7f, 1f), new Color(0.2f, 0.8f, 0.8f), new Color(0.4f, 0.9f, 0.6f) },
                2 => new[] { new Color(0.6f, 0.4f, 1f), new Color(0.8f, 0.3f, 0.9f), new Color(0.5f, 0.5f, 1f) },
                3 => new[] { new Color(1f, 0.7f, 0.2f), new Color(1f, 0.5f, 0.1f), new Color(0.9f, 0.9f, 0.1f) },
                4 => new[] { new Color(1f, 0.3f, 0.3f), new Color(0.9f, 0.2f, 0.5f), new Color(1f, 0.1f, 0.1f) },
                5 => new[] { new Color(0.9f, 0.1f, 0.5f), new Color(1f, 0.5f, 0f), new Color(1f, 0.8f, 0f) },
                _ => new[] { Color.white }
            };

            AssetDatabase.CreateAsset(ld, path);
            Debug.Log($"[StackTower] Created {path}");
        }

        private static GameObject CreatePanel(Transform parent, string name, Color bgColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            if (bgColor.a > 0f)
            {
                var img = go.AddComponent<Image>();
                img.color = bgColor;
            }

            go.SetActive(true);
            return go;
        }

        private static TextMeshProUGUI CreateLabel(
            Transform parent, string name, string text, int fontSize,
            Vector2 anchoredPos, Vector2? sizeDelta = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta ?? new Vector2(700, 80);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label,
            Vector2 anchoredPos, Vector2? size = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size ?? new Vector2(400, 80);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.5f, 0.9f);

            var btn = go.AddComponent<Button>();
            var cb = btn.colors;
            cb.highlightedColor = new Color(0.3f, 0.6f, 1f);
            cb.pressedColor = new Color(0.1f, 0.3f, 0.7f);
            btn.colors = cb;

            // Label
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var lRT = labelGO.AddComponent<RectTransform>();
            lRT.anchorMin = Vector2.zero;
            lRT.anchorMax = Vector2.one;
            lRT.offsetMin = lRT.offsetMax = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;

            return btn;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(600, 30);

            var slider = go.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0;
            slider.direction = Slider.Direction.LeftToRight;

            // Background
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(go.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f);

            // Fill area
            var fillAreaGO = new GameObject("Fill Area");
            fillAreaGO.transform.SetParent(go.transform, false);
            var faRT = fillAreaGO.AddComponent<RectTransform>();
            faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
            faRT.offsetMin = faRT.offsetMax = Vector2.zero;

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.8f, 0.4f);

            slider.fillRect = fillRT;
            return slider;
        }

        private static Image[] CreateStarRow(Transform parent, Vector2 pos)
        {
            GameObject row = new GameObject("StarRow");
            row.transform.SetParent(parent, false);
            var rowRT = row.AddComponent<RectTransform>();
            rowRT.anchoredPosition = pos;
            rowRT.sizeDelta = new Vector2(300, 80);

            var hg = row.AddComponent<HorizontalLayoutGroup>();
            hg.spacing = 20;
            hg.childForceExpandWidth = false;
            hg.childForceExpandHeight = false;
            hg.childAlignment = TextAnchor.MiddleCenter;

            Image[] stars = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject s = new GameObject($"Star{i + 1}");
                s.transform.SetParent(row.transform, false);
                var sRT = s.AddComponent<RectTransform>();
                sRT.sizeDelta = new Vector2(70, 70);
                var img = s.AddComponent<Image>();
                img.color = new Color(1f, 0.85f, 0.1f);
                stars[i] = img;
            }
            return stars;
        }

        private static void WireUpUIManager(
            StackTowerUIManager uiMgr,
            GameObject mainMenu, GameObject hud, GameObject pause,
            GameObject gameOver, GameObject victory,
            LevelSelectUI lsUI, ResultPanel result,
            TextMeshProUGUI floorText, Slider progressBar,
            TextMeshProUGUI comboText, TextMeshProUGUI levelName,
            TextMeshProUGUI goFloorText, Button retryBtn, Button menuBtn)
        {
            var so = new SerializedObject(uiMgr);
            so.FindProperty("mainMenuPanel").objectReferenceValue    = mainMenu;
            so.FindProperty("hudPanel").objectReferenceValue         = hud;
            so.FindProperty("pausePanel").objectReferenceValue       = pause;
            so.FindProperty("gameOverPanel").objectReferenceValue    = gameOver;
            so.FindProperty("victoryPanel").objectReferenceValue     = victory;
            so.FindProperty("levelSelectUI").objectReferenceValue    = lsUI;
            so.FindProperty("resultPanel").objectReferenceValue      = result;
            so.FindProperty("floorText").objectReferenceValue        = floorText;
            so.FindProperty("floorProgressBar").objectReferenceValue = progressBar;
            so.FindProperty("comboText").objectReferenceValue        = comboText;
            so.FindProperty("levelNameText").objectReferenceValue    = levelName;
            so.FindProperty("gameOverFloorText").objectReferenceValue = goFloorText;
            so.FindProperty("gameOverRetryBtn").objectReferenceValue  = retryBtn;
            so.FindProperty("gameOverMenuBtn").objectReferenceValue   = menuBtn;
            so.ApplyModifiedProperties();
        }
    }
}
#endif
