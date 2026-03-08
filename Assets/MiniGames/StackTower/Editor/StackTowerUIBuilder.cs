#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace StackTower
{
    /// <summary>
    /// Editor utility: destroys the old Canvas and rebuilds the complete Stack Tower UI from scratch.
    /// Run via menu: StackTower > Rebuild UI
    /// </summary>
    public static class StackTowerUIBuilder
    {
        // ── Colors ───────────────────────────────────────────────────────────
        static readonly Color BgDark       = new Color(0.05f, 0.05f, 0.10f, 0.92f);
        static readonly Color BgPanel      = new Color(0.08f, 0.08f, 0.14f, 0.95f);
        static readonly Color AccentYellow = new Color(1.00f, 0.85f, 0.20f, 1f);
        static readonly Color AccentRed    = new Color(0.90f, 0.25f, 0.25f, 1f);
        static readonly Color AccentGreen  = new Color(0.20f, 0.80f, 0.40f, 1f);
        static readonly Color AccentBlue   = new Color(0.25f, 0.55f, 0.95f, 1f);
        static readonly Color White        = Color.white;
        static readonly Color Transparent  = new Color(0, 0, 0, 0);

        // ── Entry Point ──────────────────────────────────────────────────────
        [MenuItem("StackTower/Rebuild UI")]
        public static void RebuildUI()
        {
            // 1. Delete old Canvas
            var oldCanvas = GameObject.Find("Canvas");
            if (oldCanvas != null) Undo.DestroyObjectImmediate(oldCanvas);

            // 2. Ensure EventSystem
            EnsureEventSystem();

            // 3. Build new Canvas
            var canvasGO = BuildCanvas();
            var canvas = canvasGO.GetComponent<Canvas>();

            // 4. Build panels (all inactive except MainMenuPanel)
            var mainMenu   = BuildMainMenuPanel(canvasGO.transform);
            var levelSel   = BuildLevelSelectPanel(canvasGO.transform);
            var hud        = BuildHUDPanel(canvasGO.transform);
            var pause      = BuildPausePanel(canvasGO.transform);
            var gameOver   = BuildGameOverPanel(canvasGO.transform);
            var victory    = BuildVictoryPanel(canvasGO.transform);

            // 5. Effects overlay (full-screen, inactive)
            var flashOverlay = BuildFlashOverlay(canvasGO.transform);
            var perfectText  = BuildPerfectText(canvasGO.transform);

            // Set default visibility
            levelSel.SetActive(false);
            hud.SetActive(false);
            pause.SetActive(false);
            gameOver.SetActive(false);
            victory.SetActive(false);
            flashOverlay.SetActive(false);
            perfectText.SetActive(false);

            // 6. Wire up StackTowerUIManager
            WireUIManager(canvasGO, mainMenu, levelSel, hud, pause, gameOver, victory);

            // 7. Wire up PerfectEffect
            WirePerfectEffect(canvasGO, flashOverlay, perfectText);

            Undo.RegisterCreatedObjectUndo(canvasGO, "Rebuild Stack Tower UI");
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[StackTower] UI rebuilt successfully.");
        }

        // ── Canvas ───────────────────────────────────────────────────────────
        static GameObject BuildCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        // ── Panels ───────────────────────────────────────────────────────────

        static GameObject BuildMainMenuPanel(Transform parent)
        {
            var panel = MakeFullPanel("MainMenuPanel", parent, BgDark);

            // Title
            var title = MakeText("TitleText", panel.transform,
                "STACK TOWER", 80, FontStyles.Bold, AccentYellow);
            SetRect(title, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.85f));

            // Subtitle
            var sub = MakeText("SubtitleText", panel.transform,
                "Xây tháp càng cao càng tốt!", 32, FontStyles.Normal, White);
            SetRect(sub, new Vector2(0.15f, 0.52f), new Vector2(0.85f, 0.60f));

            // Play button
            var play = MakeButton("PlayButton", panel.transform, "CHƠI NGAY", AccentGreen, White, 48);
            SetRect(play, new Vector2(0.2f, 0.28f), new Vector2(0.8f, 0.42f));

            return panel;
        }

        static GameObject BuildLevelSelectPanel(Transform parent)
        {
            var panel = MakeFullPanel("LevelSelectPanel", parent, BgDark);

            var title = MakeText("Title", panel.transform, "CHỌN LEVEL", 64, FontStyles.Bold, AccentYellow);
            SetRect(title, new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.95f));

            // Container for level buttons
            var container = new GameObject("LevelButtonContainer");
            container.transform.SetParent(panel.transform, false);
            var gridRT = container.AddComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(0.05f, 0.15f);
            gridRT.anchorMax = new Vector2(0.95f, 0.80f);
            gridRT.offsetMin = gridRT.offsetMax = Vector2.zero;
            var grid = container.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200, 200);
            grid.spacing = new Vector2(20, 20);
            grid.childAlignment = TextAnchor.UpperCenter;

            var back = MakeButton("BackButton", panel.transform, "QUAY LAI", AccentRed, White, 36);
            SetRect(back, new Vector2(0.1f, 0.02f), new Vector2(0.45f, 0.12f));

            // LevelSelectUI on the PANEL (not container) so gameObject.SetActive works on whole panel
            var lsUI = panel.AddComponent<LevelSelectUI>();

            // Wire internal refs via SerializedObject right here
            var lsSO = new SerializedObject(lsUI);
            lsSO.FindProperty("levelButtonContainer").objectReferenceValue = container.transform;
            lsSO.FindProperty("backButton").objectReferenceValue = back.GetComponent<Button>();
            lsSO.ApplyModifiedProperties();

            return panel;
        }

        static GameObject BuildHUDPanel(Transform parent)
        {
            var panel = new GameObject("HUDPanel");
            panel.transform.SetParent(parent, false);
            StretchFull(panel.AddComponent<RectTransform>());
            // Transparent background (HUD overlays gameplay)
            var img = panel.AddComponent<Image>();
            img.color = Transparent;
            img.raycastTarget = false;

            // Floor text (top-center)
            var floorTxt = MakeText("FloorText", panel.transform, "Tầng: 0 / 10", 40, FontStyles.Bold, White);
            SetRect(floorTxt, new Vector2(0.25f, 0.88f), new Vector2(0.75f, 0.96f));

            // Level name (below floor)
            var lvlName = MakeText("LevelNameText", panel.transform, "Level 1", 28, FontStyles.Normal,
                new Color(0.8f, 0.8f, 0.8f));
            SetRect(lvlName, new Vector2(0.2f, 0.82f), new Vector2(0.8f, 0.89f));

            // Progress bar
            var pb = BuildProgressBar("ProgressBar", panel.transform);
            SetRect(pb, new Vector2(0.05f, 0.93f), new Vector2(0.95f, 0.965f));

            // Combo text (bottom-center)
            var combo = MakeText("ComboText", panel.transform, "", 44, FontStyles.Bold, AccentYellow);
            SetRect(combo, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.18f));

            // Pause button (top-right)
            var pauseBtn = MakeButton("PauseButton", panel.transform, "II", new Color(0.2f, 0.2f, 0.2f, 0.8f), White, 40);
            SetRect(pauseBtn, new Vector2(0.82f, 0.88f), new Vector2(0.97f, 0.97f));

            return panel;
        }

        static GameObject BuildPausePanel(Transform parent)
        {
            var panel = MakeFullPanel("PausePanel", parent, new Color(0, 0, 0, 0.75f));

            var title = MakeText("PauseTitle", panel.transform, "TẠM DỪNG", 72, FontStyles.Bold, White);
            SetRect(title, new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.82f));

            var resume  = MakeButton("ResumeButton",  panel.transform, "TIEP TUC",   AccentGreen,  White, 44);
            SetRect(resume, new Vector2(0.2f, 0.52f), new Vector2(0.8f, 0.63f));

            var restart = MakeButton("RestartButton", panel.transform, "CHOI LAI",   AccentYellow, White, 40);
            SetRect(restart, new Vector2(0.2f, 0.39f), new Vector2(0.8f, 0.50f));

            var menu    = MakeButton("MenuButton",    panel.transform, "MENU CHINH", AccentBlue,   White, 40);
            SetRect(menu, new Vector2(0.2f, 0.26f), new Vector2(0.8f, 0.37f));

            return panel;
        }

        static GameObject BuildGameOverPanel(Transform parent)
        {
            var panel = MakeFullPanel("GameOverPanel", parent, BgDark);

            var title = MakeText("GameOverTitle", panel.transform, "GAME OVER", 80, FontStyles.Bold, AccentRed);
            SetRect(title, new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.82f));

            var floor = MakeText("GoFloorText", panel.transform, "Bạn đã xây được X tầng", 40, FontStyles.Normal, White);
            SetRect(floor, new Vector2(0.1f, 0.50f), new Vector2(0.9f, 0.62f));

            var retry = MakeButton("RetryButton", panel.transform, "THU LAI",    AccentGreen,  White, 44);
            SetRect(retry, new Vector2(0.15f, 0.33f), new Vector2(0.85f, 0.46f));

            var menu  = MakeButton("MenuButton",  panel.transform, "MENU CHINH", AccentBlue,   White, 40);
            SetRect(menu, new Vector2(0.2f, 0.19f), new Vector2(0.8f, 0.31f));

            return panel;
        }

        static GameObject BuildVictoryPanel(Transform parent)
        {
            var panel = MakeFullPanel("VictoryPanel", parent, new Color(0.04f, 0.10f, 0.04f, 0.95f));

            var title = MakeText("VictoryTitle", panel.transform, "CHIEN THANG!", 72, FontStyles.Bold, AccentGreen);
            SetRect(title, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.95f));

            var floor  = MakeText("VPFloorText",  panel.transform, "Da xay: X tang",      36, FontStyles.Normal, White);
            SetRect(floor, new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.77f));

            var perf   = MakeText("VPPerfectText", panel.transform, "Perfect: 0 / 0 (0%)", 34, FontStyles.Normal, AccentYellow);
            SetRect(perf, new Vector2(0.1f, 0.60f), new Vector2(0.9f, 0.68f));

            var starTxt = MakeText("VPStarText",   panel.transform, "★★★",                60, FontStyles.Bold,   AccentYellow);
            SetRect(starTxt, new Vector2(0.2f, 0.49f), new Vector2(0.8f, 0.60f));

            var next   = MakeButton("NextButton",    panel.transform, "LEVEL TIEP",  AccentGreen,  White, 40);
            SetRect(next,   new Vector2(0.15f, 0.32f), new Vector2(0.85f, 0.44f));

            var retry2 = MakeButton("VPRetryButton", panel.transform, "CHOI LAI",    AccentYellow, White, 38);
            SetRect(retry2, new Vector2(0.15f, 0.19f), new Vector2(0.85f, 0.31f));

            var menu2  = MakeButton("VPMenuButton",  panel.transform, "MENU CHINH",  AccentBlue,   White, 36);
            SetRect(menu2,  new Vector2(0.2f, 0.07f), new Vector2(0.8f, 0.18f));

            // ResultPanel component directly on VictoryPanel — wire all refs here
            var rp   = panel.AddComponent<ResultPanel>();
            var rpSO = new SerializedObject(rp);
            rpSO.FindProperty("floorText").objectReferenceValue      = floor.GetComponent<TextMeshProUGUI>();
            rpSO.FindProperty("perfectText").objectReferenceValue    = perf.GetComponent<TextMeshProUGUI>();
            rpSO.FindProperty("starCountText").objectReferenceValue  = starTxt.GetComponent<TextMeshProUGUI>();
            rpSO.FindProperty("retryButton").objectReferenceValue    = retry2.GetComponent<Button>();
            rpSO.FindProperty("nextLevelButton").objectReferenceValue = next.GetComponent<Button>();
            rpSO.FindProperty("menuButton").objectReferenceValue     = menu2.GetComponent<Button>();
            rpSO.ApplyModifiedProperties();

            return panel;
        }

        // ── Effects ──────────────────────────────────────────────────────────

        static GameObject BuildFlashOverlay(Transform parent)
        {
            var go = new GameObject("PerfectFlashOverlay");
            go.transform.SetParent(parent, false);
            StretchFull(go.AddComponent<RectTransform>());
            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 0.6f, 0f);
            img.raycastTarget = false;
            return go;
        }

        static GameObject BuildPerfectText(Transform parent)
        {
            var go = new GameObject("PerfectText");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(600, 120);
            rt.anchoredPosition = Vector2.zero;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "PERFECT!";
            tmp.fontSize = 72;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 1f, 0.6f);
            tmp.raycastTarget = false;
            return go;
        }

        // ── Wiring ───────────────────────────────────────────────────────────

        static void WireUIManager(GameObject canvas,
            GameObject mainMenu, GameObject levelSel, GameObject hud,
            GameObject pause, GameObject gameOver, GameObject victory)
        {
            var uiMgrGO = GameObject.Find("StackTowerUIManager");
            if (uiMgrGO == null)
            {
                Debug.LogWarning("[StackTower] StackTowerUIManager GameObject not found in scene.");
                return;
            }

            var ui = uiMgrGO.GetComponent<StackTowerUIManager>();
            if (ui == null) return;

            var so = new SerializedObject(ui);
            SetRef(so, "mainMenuPanel",  mainMenu);
            SetRef(so, "hudPanel",       hud);
            SetRef(so, "pausePanel",     pause);
            SetRef(so, "gameOverPanel",  gameOver);
            SetRef(so, "victoryPanel",   victory);

            // LevelSelectUI is on the panel itself
            var lsUI = levelSel.GetComponent<LevelSelectUI>();
            if (lsUI != null) so.FindProperty("levelSelectUI").objectReferenceValue = lsUI;

            // ResultPanel is on VictoryPanel itself
            var rp = victory.GetComponent<ResultPanel>();
            if (rp != null) so.FindProperty("resultPanel").objectReferenceValue = rp;

            // Buttons
            SetRef(so, "playButton",       mainMenu.GetComponentInChildren<Button>(true));
            SetRef(so, "gameOverRetryBtn", FindButtonInChildren(gameOver, "RetryButton"));
            SetRef(so, "gameOverMenuBtn",  FindButtonInChildren(gameOver, "MenuButton"));

            // Text fields
            SetRef(so, "floorText",       FindTMP(hud, "FloorText"));
            SetRef(so, "comboText",       FindTMP(hud, "ComboText"));
            SetRef(so, "levelNameText",   FindTMP(hud, "LevelNameText"));
            SetRef(so, "gameOverFloorText", FindTMP(gameOver, "GoFloorText"));
            SetRef(so, "floorProgressBar",  hud.GetComponentInChildren<Slider>(true));

            so.ApplyModifiedProperties();
        }

        static void WirePerfectEffect(GameObject canvas, GameObject flashOverlay, GameObject perfectText)
        {
            var effectGO = GameObject.Find("Effects");
            if (effectGO == null) return;

            var pe = effectGO.GetComponentInChildren<PerfectEffect>(true);
            if (pe == null) return;

            var so = new SerializedObject(pe);
            SetRef(so, "perfectText",   perfectText.GetComponent<TextMeshProUGUI>());
            SetRef(so, "flashOverlay",  flashOverlay.GetComponent<Image>());
            so.ApplyModifiedProperties();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        static GameObject MakeFullPanel(string name, Transform parent, Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            StretchFull(go.AddComponent<RectTransform>());
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            return go;
        }

        static GameObject MakeText(string name, Transform parent,
            string text, float size, FontStyles style, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            return go;
        }

        static GameObject MakeButton(string name, Transform parent,
            string label, Color bgColor, Color textColor, float fontSize)
        {
            // Container
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            var cs = btn.colors;
            cs.highlightedColor = Color.Lerp(bgColor, Color.white, 0.2f);
            cs.pressedColor     = Color.Lerp(bgColor, Color.black, 0.2f);
            btn.colors = cs;

            // Label
            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(go.transform, false);
            StretchFull(lblGO.AddComponent<RectTransform>());
            var tmp = lblGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return go;
        }

        static GameObject BuildProgressBar(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var slider = go.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 10;
            slider.value = 0;
            slider.wholeNumbers = true;
            slider.direction = Slider.Direction.LeftToRight;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            StretchFull(bg.AddComponent<RectTransform>());
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            slider.targetGraphic = bgImg;

            // Fill area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var faRT = fillArea.AddComponent<RectTransform>();
            faRT.anchorMin = new Vector2(0, 0);
            faRT.anchorMax = new Vector2(1, 1);
            faRT.offsetMin = faRT.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            StretchFull(fill.AddComponent<RectTransform>());
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = AccentYellow;
            slider.fillRect = fill.GetComponent<RectTransform>();

            return go;
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void SetRef(SerializedObject so, string propName, Object obj)
        {
            var prop = so.FindProperty(propName);
            if (prop != null) prop.objectReferenceValue = obj;
        }

        static Button FindButtonInChildren(GameObject root, string childName)
        {
            var t = root.transform.Find(childName);
            return t != null ? t.GetComponent<Button>() : null;
        }

        static TextMeshProUGUI FindTMP(GameObject root, string childName)
        {
            var t = root.transform.Find(childName);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
    }
}
#endif
