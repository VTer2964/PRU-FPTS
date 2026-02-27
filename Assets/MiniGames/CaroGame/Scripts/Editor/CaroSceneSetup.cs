using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace CaroGame.Editor
{
    public static class CaroSceneSetup
    {
        [MenuItem("CaroGame/Setup Complete Scene")]
        public static void SetupCompleteScene()
        {
            // Create root
            GameObject root = new GameObject("CaroGame");
            Undo.RegisterCreatedObjectUndo(root, "Create CaroGame");

            // ========== Canvas ==========
            GameObject canvasObj = CreateCanvas(root.transform);
            Canvas canvas = canvasObj.GetComponent<Canvas>();

            // ========== Main Menu Panel ==========
            GameObject mainMenuPanel = CreateMainMenuPanel(canvasObj.transform);

            // ========== Game Panel ==========
            GameObject gamePanel = CreateGamePanel(canvasObj.transform);
            gamePanel.SetActive(false);

            // ========== Results Panel ==========
            GameObject resultsPanel = CreateResultsPanel(canvasObj.transform);

            // ========== Cell Prefab ==========
            GameObject cellPrefab = CreateCellPrefab();

            // ========== Manager Objects ==========
            // GameManager
            GameObject managerObj = new GameObject("GameManager");
            managerObj.transform.SetParent(root.transform);
            var gameManager = managerObj.AddComponent<CaroGameManager>();

            // BoardController
            GameObject boardObj = new GameObject("BoardController");
            boardObj.transform.SetParent(root.transform);
            var boardController = boardObj.AddComponent<CaroBoardController>();

            // AI Controller
            GameObject aiObj = new GameObject("AIController");
            aiObj.transform.SetParent(root.transform);
            var caroAI = aiObj.AddComponent<CaroAI>();

            // Timer System
            GameObject timerObj = new GameObject("TimerSystem");
            timerObj.transform.SetParent(root.transform);
            var timerSystem = timerObj.AddComponent<CaroTimerSystem>();

            // ========== Create ScriptableObject Config ==========
            CaroGameConfig config = CreateConfig();

            // ========== Wire References ==========
            // Get UI components
            var uiManager = canvasObj.GetComponent<CaroUIManager>();
            if (uiManager == null)
                uiManager = canvasObj.AddComponent<CaroUIManager>();

            // Wire GameManager
            SerializedObject gmSO = new SerializedObject(gameManager);
            gmSO.FindProperty("config").objectReferenceValue = config;
            gmSO.FindProperty("boardController").objectReferenceValue = boardController;
            gmSO.FindProperty("caroAI").objectReferenceValue = caroAI;
            gmSO.FindProperty("timerSystem").objectReferenceValue = timerSystem;
            gmSO.FindProperty("uiManager").objectReferenceValue = uiManager;
            gmSO.ApplyModifiedProperties();

            // Wire BoardController
            SerializedObject bcSO = new SerializedObject(boardController);
            bcSO.FindProperty("config").objectReferenceValue = config;
            bcSO.FindProperty("cellPrefab").objectReferenceValue = cellPrefab;

            // Find BoardContainer (GridLayout inside GamePanel)
            Transform boardContainer = gamePanel.transform.Find("BoardContainer");
            bcSO.FindProperty("boardContainer").objectReferenceValue = boardContainer;
            GridLayoutGroup gridLayout = boardContainer?.GetComponent<GridLayoutGroup>();
            bcSO.FindProperty("gridLayout").objectReferenceValue = gridLayout;
            bcSO.ApplyModifiedProperties();

            // Wire AI
            SerializedObject aiSO = new SerializedObject(caroAI);
            aiSO.FindProperty("config").objectReferenceValue = config;
            aiSO.ApplyModifiedProperties();

            // Wire UIManager
            SerializedObject uiSO = new SerializedObject(uiManager);
            uiSO.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
            uiSO.FindProperty("gamePanel").objectReferenceValue = gamePanel;

            var resultsPanelComp = resultsPanel.GetComponent<CaroResultsPanel>();
            uiSO.FindProperty("resultsPanel").objectReferenceValue = resultsPanelComp;

            // Find UI elements
            Transform timerDisplay = gamePanel.transform.Find("TopBar/TimerDisplay");
            if (timerDisplay != null)
                uiSO.FindProperty("timerDisplay").objectReferenceValue = timerDisplay.GetComponent<CaroTimerDisplay>();

            Transform moveCounterT = gamePanel.transform.Find("TopBar/MoveCounter");
            if (moveCounterT != null)
                uiSO.FindProperty("moveCounter").objectReferenceValue = moveCounterT.GetComponent<CaroMoveCounter>();

            Transform turnIndicatorT = gamePanel.transform.Find("TopBar/TurnIndicator");
            if (turnIndicatorT != null)
                uiSO.FindProperty("turnIndicator").objectReferenceValue = turnIndicatorT.GetComponent<TextMeshProUGUI>();

            Transform hintBtn = gamePanel.transform.Find("TopBar/HintToggleButton");
            if (hintBtn != null)
            {
                uiSO.FindProperty("hintToggleButton").objectReferenceValue = hintBtn.GetComponent<Button>();
                Transform hintText = hintBtn.Find("Text");
                if (hintText != null)
                    uiSO.FindProperty("hintButtonText").objectReferenceValue = hintText.GetComponent<TextMeshProUGUI>();
            }

            // Wire Play button
            Transform playBtn = mainMenuPanel.transform.Find("PlayButton");
            if (playBtn != null)
                uiSO.FindProperty("playButton").objectReferenceValue = playBtn.GetComponent<Button>();

            uiSO.ApplyModifiedProperties();

            // Wire TimerDisplay
            if (timerDisplay != null)
            {
                var timerDisplayComp = timerDisplay.GetComponent<CaroTimerDisplay>();
                if (timerDisplayComp != null)
                {
                    SerializedObject tdSO = new SerializedObject(timerDisplayComp);
                    tdSO.FindProperty("timerSystem").objectReferenceValue = timerSystem;

                    Transform timerText = timerDisplay.Find("TimerText");
                    if (timerText != null)
                        tdSO.FindProperty("timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();

                    Transform timerBar = timerDisplay.Find("TimerBar");
                    if (timerBar != null)
                        tdSO.FindProperty("timerBar").objectReferenceValue = timerBar.GetComponent<Image>();

                    tdSO.ApplyModifiedProperties();
                }
            }

            // Wire ResultsPanel
            if (resultsPanelComp != null)
            {
                SerializedObject rpSO = new SerializedObject(resultsPanelComp);

                Transform resultTitle = resultsPanel.transform.Find("Background/ResultTitle");
                if (resultTitle != null)
                    rpSO.FindProperty("resultTitleText").objectReferenceValue = resultTitle.GetComponent<TextMeshProUGUI>();

                Transform resultDesc = resultsPanel.transform.Find("Background/ResultDescription");
                if (resultDesc != null)
                    rpSO.FindProperty("resultDescriptionText").objectReferenceValue = resultDesc.GetComponent<TextMeshProUGUI>();

                Transform moveCount = resultsPanel.transform.Find("Background/MoveCount");
                if (moveCount != null)
                    rpSO.FindProperty("moveCountText").objectReferenceValue = moveCount.GetComponent<TextMeshProUGUI>();

                Transform medalIcon = resultsPanel.transform.Find("Background/MedalIcon");
                if (medalIcon != null)
                    rpSO.FindProperty("medalIcon").objectReferenceValue = medalIcon.GetComponent<Image>();

                Transform medalText = resultsPanel.transform.Find("Background/MedalText");
                if (medalText != null)
                    rpSO.FindProperty("medalText").objectReferenceValue = medalText.GetComponent<TextMeshProUGUI>();

                Transform restartBtn = resultsPanel.transform.Find("Background/ButtonContainer/RestartButton");
                if (restartBtn != null)
                    rpSO.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();

                Transform menuBtn = resultsPanel.transform.Find("Background/ButtonContainer/MenuButton");
                if (menuBtn != null)
                    rpSO.FindProperty("menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();

                rpSO.ApplyModifiedProperties();
            }

            // EventSystem
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                eventSystem.transform.SetParent(root.transform);
            }

            // Camera setup
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            }

            Selection.activeGameObject = root;
            Debug.Log("[CaroGame] Scene setup complete! All references wired.");
        }

        private static GameObject CreateCanvas(Transform parent)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(parent);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            canvasObj.AddComponent<CaroUIManager>();

            return canvasObj;
        }

        private static GameObject CreateMainMenuPanel(Transform parent)
        {
            // Main Menu Panel - full screen
            GameObject panel = CreatePanel(parent, "MainMenuPanel", new Color(0.12f, 0.12f, 0.18f, 0.95f));
            SetFullStretch(panel.GetComponent<RectTransform>());

            // Title
            GameObject title = CreateTextObject(panel.transform, "TitleText", "CARO GAME",
                64, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.2f, 0.6f);
            titleRect.anchorMax = new Vector2(0.8f, 0.8f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            title.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.85f, 0.4f);

            // Subtitle
            GameObject subtitle = CreateTextObject(panel.transform, "SubtitleText", "Fast Gomoku - 5 in a Row",
                28, FontStyles.Normal, TextAlignmentOptions.Center);
            RectTransform subRect = subtitle.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.2f, 0.52f);
            subRect.anchorMax = new Vector2(0.8f, 0.6f);
            subRect.offsetMin = Vector2.zero;
            subRect.offsetMax = Vector2.zero;
            subtitle.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.8f);

            // Play Button
            GameObject playBtn = CreateButton(panel.transform, "PlayButton", "PLAY",
                new Color(0.2f, 0.7f, 0.3f), new Vector2(250, 60));
            RectTransform playRect = playBtn.GetComponent<RectTransform>();
            playRect.anchorMin = new Vector2(0.5f, 0.35f);
            playRect.anchorMax = new Vector2(0.5f, 0.35f);
            playRect.anchoredPosition = Vector2.zero;

            // Instructions
            GameObject instructions = CreateTextObject(panel.transform, "InstructionsText",
                "Get 5 in a row to win!\n60 seconds per turn\nBlock the AI and strike fast!",
                20, FontStyles.Normal, TextAlignmentOptions.Center);
            RectTransform instrRect = instructions.GetComponent<RectTransform>();
            instrRect.anchorMin = new Vector2(0.15f, 0.1f);
            instrRect.anchorMax = new Vector2(0.85f, 0.3f);
            instrRect.offsetMin = Vector2.zero;
            instrRect.offsetMax = Vector2.zero;
            instructions.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.6f, 0.65f);

            return panel;
        }

        private static GameObject CreateGamePanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "GamePanel", new Color(0.1f, 0.1f, 0.15f, 0.95f));
            SetFullStretch(panel.GetComponent<RectTransform>());

            // Top Bar
            GameObject topBar = new GameObject("TopBar");
            topBar.transform.SetParent(panel.transform, false);
            RectTransform topBarRect = topBar.AddComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0, 0.88f);
            topBarRect.anchorMax = new Vector2(1, 1);
            topBarRect.offsetMin = new Vector2(10, 0);
            topBarRect.offsetMax = new Vector2(-10, -5);

            HorizontalLayoutGroup hlg = topBar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(20, 20, 5, 5);

            // Turn Indicator
            GameObject turnIndicator = CreateTextObject(topBar.transform, "TurnIndicator", "Your Turn (X)",
                28, FontStyles.Bold, TextAlignmentOptions.Left);
            var turnLE = turnIndicator.AddComponent<LayoutElement>();
            turnLE.preferredWidth = 250;
            turnLE.preferredHeight = 40;

            // Timer Display
            GameObject timerDisplayObj = new GameObject("TimerDisplay");
            timerDisplayObj.transform.SetParent(topBar.transform, false);
            timerDisplayObj.AddComponent<RectTransform>();
            var timerDisplayComp = timerDisplayObj.AddComponent<CaroTimerDisplay>();
            var timerLE = timerDisplayObj.AddComponent<LayoutElement>();
            timerLE.preferredWidth = 200;
            timerLE.preferredHeight = 40;

            // Timer Bar (background)
            GameObject timerBarBg = new GameObject("TimerBarBg");
            timerBarBg.transform.SetParent(timerDisplayObj.transform, false);
            RectTransform timerBarBgRect = timerBarBg.AddComponent<RectTransform>();
            SetFullStretch(timerBarBgRect);
            timerBarBgRect.offsetMin = new Vector2(0, 8);
            timerBarBgRect.offsetMax = new Vector2(0, -24);
            Image timerBarBgImg = timerBarBg.AddComponent<Image>();
            timerBarBgImg.color = new Color(0.2f, 0.2f, 0.25f);

            // Timer Bar (fill)
            GameObject timerBar = new GameObject("TimerBar");
            timerBar.transform.SetParent(timerDisplayObj.transform, false);
            RectTransform timerBarRect = timerBar.AddComponent<RectTransform>();
            SetFullStretch(timerBarRect);
            timerBarRect.offsetMin = new Vector2(0, 8);
            timerBarRect.offsetMax = new Vector2(0, -24);
            Image timerBarImg = timerBar.AddComponent<Image>();
            timerBarImg.color = new Color(0.2f, 0.8f, 0.2f);
            timerBarImg.type = Image.Type.Filled;
            timerBarImg.fillMethod = Image.FillMethod.Horizontal;

            // Timer Text
            GameObject timerText = CreateTextObject(timerDisplayObj.transform, "TimerText", "01:00",
                24, FontStyles.Bold, TextAlignmentOptions.Center);
            SetFullStretch(timerText.GetComponent<RectTransform>());

            // Move Counter
            GameObject moveCounterObj = new GameObject("MoveCounter");
            moveCounterObj.transform.SetParent(topBar.transform, false);
            moveCounterObj.AddComponent<RectTransform>();
            var mcComp = moveCounterObj.AddComponent<CaroMoveCounter>();
            var mcLE = moveCounterObj.AddComponent<LayoutElement>();
            mcLE.preferredWidth = 140;
            mcLE.preferredHeight = 40;

            GameObject mcText = CreateTextObject(moveCounterObj.transform, "MoveText", "Moves: 0",
                22, FontStyles.Normal, TextAlignmentOptions.Center);
            SetFullStretch(mcText.GetComponent<RectTransform>());

            // Wire MoveCounter text
            SerializedObject mcSO = new SerializedObject(mcComp);
            mcSO.FindProperty("moveText").objectReferenceValue = mcText.GetComponent<TextMeshProUGUI>();
            mcSO.ApplyModifiedProperties();

            // Hint Toggle Button
            GameObject hintBtn = CreateButton(topBar.transform, "HintToggleButton", "Hints: ON",
                new Color(0.4f, 0.4f, 0.6f), new Vector2(130, 35));
            var hintLE = hintBtn.AddComponent<LayoutElement>();
            hintLE.preferredWidth = 130;
            hintLE.preferredHeight = 35;

            // Board Container with GridLayout
            GameObject boardContainer = new GameObject("BoardContainer");
            boardContainer.transform.SetParent(panel.transform, false);
            RectTransform boardRect = boardContainer.AddComponent<RectTransform>();
            boardRect.anchorMin = new Vector2(0.5f, 0.44f);
            boardRect.anchorMax = new Vector2(0.5f, 0.44f);
            boardRect.sizeDelta = new Vector2(590, 590); // 10*(55+2) + padding
            boardRect.anchoredPosition = Vector2.zero;

            Image boardBg = boardContainer.AddComponent<Image>();
            boardBg.color = new Color(0.18f, 0.18f, 0.22f);

            GridLayoutGroup grid = boardContainer.AddComponent<GridLayoutGroup>();
            grid.constraintCount = 10;
            grid.cellSize = new Vector2(55, 55);
            grid.spacing = new Vector2(2, 2);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.padding = new RectOffset(5, 5, 5, 5);

            ContentSizeFitter csf = boardContainer.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return panel;
        }

        private static GameObject CreateResultsPanel(Transform parent)
        {
            // Semi-transparent overlay
            GameObject panel = new GameObject("ResultsPanel");
            panel.transform.SetParent(parent, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            SetFullStretch(panelRect);

            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0, 0, 0, 0.7f);

            panel.AddComponent<CaroResultsPanel>();

            // Background card
            GameObject bg = CreatePanel(panel.transform, "Background", new Color(0.15f, 0.15f, 0.2f, 0.98f));
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(500, 450);
            bgRect.anchoredPosition = Vector2.zero;

            // Vertical layout
            VerticalLayoutGroup vlg = bg.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(30, 30, 30, 30);

            // Result Title
            GameObject resultTitle = CreateTextObject(bg.transform, "ResultTitle", "YOU WIN!",
                48, FontStyles.Bold, TextAlignmentOptions.Center);
            var rtLE = resultTitle.AddComponent<LayoutElement>();
            rtLE.preferredHeight = 60;
            resultTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.85f, 0.4f);

            // Result Description
            GameObject resultDesc = CreateTextObject(bg.transform, "ResultDescription", "Congratulations!",
                22, FontStyles.Normal, TextAlignmentOptions.Center);
            var rdLE = resultDesc.AddComponent<LayoutElement>();
            rdLE.preferredHeight = 35;

            // Medal Icon (placeholder)
            GameObject medalIcon = new GameObject("MedalIcon");
            medalIcon.transform.SetParent(bg.transform, false);
            RectTransform medalRect = medalIcon.AddComponent<RectTransform>();
            Image medalImg = medalIcon.AddComponent<Image>();
            medalImg.color = new Color(1f, 0.84f, 0f);
            var miLE = medalIcon.AddComponent<LayoutElement>();
            miLE.preferredWidth = 80;
            miLE.preferredHeight = 80;

            // Medal Text
            GameObject medalText = CreateTextObject(bg.transform, "MedalText", "GOLD MEDAL",
                28, FontStyles.Bold, TextAlignmentOptions.Center);
            var mtLE = medalText.AddComponent<LayoutElement>();
            mtLE.preferredHeight = 40;
            medalText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.84f, 0f);

            // Move Count
            GameObject moveCount = CreateTextObject(bg.transform, "MoveCount", "Total Moves: 0",
                22, FontStyles.Normal, TextAlignmentOptions.Center);
            var mcLE = moveCount.AddComponent<LayoutElement>();
            mcLE.preferredHeight = 30;

            // Button container
            GameObject btnContainer = new GameObject("ButtonContainer");
            btnContainer.transform.SetParent(bg.transform, false);
            btnContainer.AddComponent<RectTransform>();
            HorizontalLayoutGroup btnHLG = btnContainer.AddComponent<HorizontalLayoutGroup>();
            btnHLG.spacing = 20;
            btnHLG.childAlignment = TextAnchor.MiddleCenter;
            btnHLG.childControlWidth = false;
            btnHLG.childControlHeight = false;
            var bcLE = btnContainer.AddComponent<LayoutElement>();
            bcLE.preferredHeight = 55;

            // Restart Button
            CreateButton(btnContainer.transform, "RestartButton", "RESTART",
                new Color(0.2f, 0.7f, 0.3f), new Vector2(180, 50));

            // Menu Button
            CreateButton(btnContainer.transform, "MenuButton", "MENU",
                new Color(0.5f, 0.5f, 0.6f), new Vector2(180, 50));

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateCellPrefab()
        {
            string prefabDir = "Assets/CaroGame/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/CaroGame"))
                    AssetDatabase.CreateFolder("Assets", "CaroGame");
                AssetDatabase.CreateFolder("Assets/CaroGame", "Prefabs");
            }

            string prefabPath = prefabDir + "/CaroCell.prefab";

            // Create cell game object
            GameObject cellObj = new GameObject("CaroCell");

            // Background
            RectTransform cellRect = cellObj.AddComponent<RectTransform>();
            cellRect.sizeDelta = new Vector2(55, 55);

            Image bgImage = cellObj.AddComponent<Image>();
            bgImage.color = new Color(0.95f, 0.95f, 0.95f);

            Button btn = cellObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.85f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.8f, 0.95f);
            colors.disabledColor = new Color(0.9f, 0.9f, 0.9f);
            btn.colors = colors;

            // Piece text (X or O)
            GameObject pieceTextObj = new GameObject("PieceText");
            pieceTextObj.transform.SetParent(cellObj.transform, false);
            RectTransform ptRect = pieceTextObj.AddComponent<RectTransform>();
            SetFullStretch(ptRect);
            ptRect.offsetMin = new Vector2(2, 2);
            ptRect.offsetMax = new Vector2(-2, -2);
            TextMeshProUGUI pieceText = pieceTextObj.AddComponent<TextMeshProUGUI>();
            pieceText.text = "";
            pieceText.fontSize = 32;
            pieceText.fontStyle = FontStyles.Bold;
            pieceText.alignment = TextAlignmentOptions.Center;
            pieceText.color = Color.blue;

            // Highlight border
            GameObject highlightObj = new GameObject("HighlightBorder");
            highlightObj.transform.SetParent(cellObj.transform, false);
            RectTransform hlRect = highlightObj.AddComponent<RectTransform>();
            SetFullStretch(hlRect);
            hlRect.offsetMin = new Vector2(-2, -2);
            hlRect.offsetMax = new Vector2(2, 2);
            Image highlightImg = highlightObj.AddComponent<Image>();
            highlightImg.color = new Color(1f, 0.85f, 0.2f, 0.8f);
            highlightImg.enabled = false;
            // Make it outline-like by using a sprite or just covering
            highlightImg.raycastTarget = false;

            // Add CaroCell component
            CaroCell cell = cellObj.AddComponent<CaroCell>();

            // Wire references via SerializedObject
            SerializedObject so = new SerializedObject(cell);
            so.FindProperty("cellBackground").objectReferenceValue = bgImage;
            so.FindProperty("pieceText").objectReferenceValue = pieceText;
            so.FindProperty("highlightBorder").objectReferenceValue = highlightImg;
            so.FindProperty("cellButton").objectReferenceValue = btn;
            so.ApplyModifiedProperties();

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cellObj, prefabPath);
            Object.DestroyImmediate(cellObj);

            Debug.Log($"[CaroGame] Cell prefab created at {prefabPath}");
            return prefab;
        }

        private static CaroGameConfig CreateConfig()
        {
            string configDir = "Assets/CaroGame/Config";
            if (!AssetDatabase.IsValidFolder(configDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/CaroGame"))
                    AssetDatabase.CreateFolder("Assets", "CaroGame");
                AssetDatabase.CreateFolder("Assets/CaroGame", "Config");
            }

            string path = configDir + "/CaroGameConfig.asset";
            CaroGameConfig existing = AssetDatabase.LoadAssetAtPath<CaroGameConfig>(path);
            if (existing != null)
                return existing;

            CaroGameConfig config = ScriptableObject.CreateInstance<CaroGameConfig>();
            config.boardSize = 10;
            config.winCondition = 5;
            config.turnTimeLimit = 60f;
            config.aiThinkDelay = 0.5f;
            config.aiDepth = 2;
            config.goldMedalMoves = 10;
            config.silverMedalMoves = 20;
            config.showHints = true;
            config.cellSize = 55f;
            config.cellSpacing = 2f;

            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[CaroGame] Config created at {path}");
            return config;
        }

        // ========== Helper Methods ==========

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            Image img = panel.AddComponent<Image>();
            img.color = color;
            return panel;
        }

        private static GameObject CreateTextObject(Transform parent, string name, string text,
            float fontSize, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return obj;
        }

        private static GameObject CreateButton(Transform parent, string name, string label,
            Color bgColor, Vector2 size)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = size;

            Image img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = colors;

            // Button text
            GameObject textObj = CreateTextObject(btnObj.transform, "Text", label,
                24, FontStyles.Bold, TextAlignmentOptions.Center);
            SetFullStretch(textObj.GetComponent<RectTransform>());

            return btnObj;
        }

        private static void SetFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
