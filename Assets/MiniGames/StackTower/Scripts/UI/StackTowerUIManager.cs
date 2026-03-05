using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StackTower
{
    /// <summary>
    /// Manages all UI panels for the Stack Tower game.
    /// Listens to GameManager state and updates displays accordingly.
    /// </summary>
    public class StackTowerUIManager : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private LevelSelectUI levelSelectUI;
        [SerializeField] private ResultPanel resultPanel;

        [Header("Main Menu Elements")]
        [SerializeField] private Button playButton;

        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI floorText;          // "8 / 15"
        [SerializeField] private Slider floorProgressBar;
        [SerializeField] private TextMeshProUGUI comboText;          // "Perfect x3 🔥"
        [SerializeField] private TextMeshProUGUI levelNameText;

        [Header("Game Over Elements")]
        [SerializeField] private TextMeshProUGUI gameOverFloorText;
        [SerializeField] private Button gameOverRetryBtn;
        [SerializeField] private Button gameOverMenuBtn;

        // ── Unity ────────────────────────────────────────────────────────────
        private void Start()
        {
            AutoFindPanels();

            // Wire up Play button to start the game
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (gameOverRetryBtn != null)
                gameOverRetryBtn.onClick.AddListener(() => StackTowerGameManager.Instance?.RestartLevel());
            if (gameOverMenuBtn != null)
                gameOverMenuBtn.onClick.AddListener(() => StackTowerGameManager.Instance?.ReturnToMenu());

            ShowMainMenu();
        }

        private void OnPlayClicked()
        {
            var gm = StackTowerGameManager.Instance;
            if (gm == null) return;

            // If there are multiple levels, show level select; otherwise start level 0 directly
            if (gm.LevelCount > 1)
            {
                gm.ReturnToMenu(); // This calls ShowLevelSelect
                ShowLevelSelect();
            }
            else
            {
                gm.StartGame(0);
            }
        }

        private void AutoFindPanels()
        {
            // Use the Canvas transform so Transform.Find works on inactive children
            Canvas canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
            Transform ct = canvas != null ? canvas.transform : null;

            if (ct != null)
            {
                if (mainMenuPanel == null)  { var t = ct.Find("MainMenuPanel");   if (t) mainMenuPanel  = t.gameObject; }
                if (hudPanel == null)       { var t = ct.Find("HUDPanel");        if (t) hudPanel       = t.gameObject; }
                if (pausePanel == null)     { var t = ct.Find("PausePanel");      if (t) pausePanel     = t.gameObject; }
                if (gameOverPanel == null)  { var t = ct.Find("GameOverPanel");   if (t) gameOverPanel  = t.gameObject; }
                if (victoryPanel == null)   { var t = ct.Find("VictoryPanel");    if (t) victoryPanel   = t.gameObject; }

                if (playButton == null)
                {
                    var t = ct.Find("MainMenuPanel/PlayButton");
                    if (t != null) playButton = t.GetComponent<Button>();
                }

                if (levelSelectUI == null)
                {
                    var t = ct.Find("LevelSelectPanel");
                    if (t != null) levelSelectUI = t.GetComponent<LevelSelectUI>();
                }
                if (resultPanel == null)
                {
                    var t = ct.Find("VictoryPanel");
                    if (t != null) resultPanel = t.GetComponent<ResultPanel>();
                }

                if (floorText == null)     floorText     = FindTMPInChild(ct, "HUDPanel/FloorText");
                if (comboText == null)     comboText     = FindTMPInChild(ct, "HUDPanel/ComboText");
                if (levelNameText == null) levelNameText = FindTMPInChild(ct, "HUDPanel/LevelNameText");
                if (gameOverFloorText == null) gameOverFloorText = FindTMPInChild(ct, "GameOverPanel/GoFloorText");

                if (floorProgressBar == null)
                {
                    var t = ct.Find("HUDPanel/ProgressBar");
                    if (t != null) floorProgressBar = t.GetComponent<Slider>();
                }
                if (gameOverRetryBtn == null)
                {
                    var t = ct.Find("GameOverPanel/RetryButton");
                    if (t != null) gameOverRetryBtn = t.GetComponent<Button>();
                }
                if (gameOverMenuBtn == null)
                {
                    var t = ct.Find("GameOverPanel/MenuButton");
                    if (t != null) gameOverMenuBtn = t.GetComponent<Button>();
                }
            }
        }

        private static TextMeshProUGUI FindTMPInChild(Transform root, string path)
        {
            var t = root.Find(path);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void ShowMainMenu()
        {
            SetPanelActive(mainMenuPanel, true);
            SetPanelActive(hudPanel, false);
            SetPanelActive(pausePanel, false);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, false);
            levelSelectUI?.gameObject.SetActive(false);
        }

        public void ShowLevelSelect()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(hudPanel, false);
            SetPanelActive(pausePanel, false);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, false);
            levelSelectUI?.gameObject.SetActive(true);
            levelSelectUI?.Refresh();
        }

        public void ShowHUD()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(hudPanel, true);
            SetPanelActive(pausePanel, false);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(victoryPanel, false);
            levelSelectUI?.gameObject.SetActive(false);

            if (levelNameText != null && StackTowerGameManager.Instance?.CurrentLevel != null)
                levelNameText.text = StackTowerGameManager.Instance.CurrentLevel.levelName;

            if (comboText != null)
                comboText.text = string.Empty;
        }

        public void ShowPausePanel()
        {
            SetPanelActive(pausePanel, true);
        }

        public void HidePausePanel()
        {
            SetPanelActive(pausePanel, false);
        }

        public void ShowGameOverPanel(int reachedFloor, int targetFloor)
        {
            SetPanelActive(hudPanel, false);
            SetPanelActive(gameOverPanel, true);

            if (gameOverFloorText != null)
                gameOverFloorText.text = $"Reached floor {reachedFloor} / {targetFloor}";
        }

        public void ShowVictoryPanel(int floors, int stars, int perfects, int total)
        {
            SetPanelActive(hudPanel, false);
            SetPanelActive(victoryPanel, true);
            resultPanel?.Show(floors, stars, perfects, total);
        }

        public void UpdateFloorDisplay(int current, int target)
        {
            if (floorText != null)
                floorText.text = target > 0 ? $"Tầng: {current} / {target}" : $"Tầng: {current}";

            if (floorProgressBar != null)
            {
                bool hasTarget = target > 0;
                floorProgressBar.gameObject.SetActive(hasTarget);
                if (hasTarget)
                {
                    floorProgressBar.minValue = 0;
                    floorProgressBar.maxValue = target;
                    floorProgressBar.value = current;
                }
            }
        }

        public void UpdatePerfectCombo(int perfectCount, int total)
        {
            if (comboText == null) return;

            if (perfectCount == 0)
            {
                comboText.text = string.Empty;
                return;
            }

            // Show combo if last placement was perfect
            // (We detect this by checking if ratio is high enough)
            string mark = perfectCount >= 10 ? "!!!" :
                          perfectCount >= 5  ? "!!"  : "!";

            comboText.text = $"Perfect x{perfectCount} {mark}";
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
        }
    }
}
