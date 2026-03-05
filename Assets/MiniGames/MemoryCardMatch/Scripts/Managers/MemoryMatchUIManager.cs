using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryCardMatch
{
    public class MemoryMatchUIManager : MonoBehaviour
    {
        // -----------------------------------------------------------------------
        // Inspector References – Panels
        // -----------------------------------------------------------------------

        [Header("Panels")]
        [Tooltip("Shown at the start before a game begins")]
        [SerializeField] private GameObject mainMenuPanel;

        [Tooltip("Active during gameplay (score bar + card grid)")]
        [SerializeField] private GameObject gamePanel;

        [Tooltip("Shown when the player pauses")]
        [SerializeField] private GameObject pausePanel;

        [Tooltip("Shown when time runs out (loss)")]
        [SerializeField] private GameObject gameOverPanel;

        [Tooltip("Shown when all pairs are found (win)")]
        [SerializeField] private GameObject victoryPanel;

        // -----------------------------------------------------------------------
        // Inspector References – Main Menu
        // -----------------------------------------------------------------------

        [Header("Main Menu UI")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button[] difficultyButtons; // index 0=Easy, 1=Medium, 2=Hard

        // Colors used to highlight the selected difficulty button
        private static readonly Color ColorSelected   = new Color(0.98f, 0.85f, 0.10f); // gold
        private static readonly Color ColorEasy       = new Color(0.20f, 0.70f, 0.30f);
        private static readonly Color ColorMedium     = new Color(0.20f, 0.50f, 0.90f);
        private static readonly Color ColorHard       = new Color(0.80f, 0.20f, 0.20f);
        private static readonly Color[] DiffBaseColors = { ColorEasy, ColorMedium, ColorHard };

        private int currentDifficultyIndex = 1; // default Medium

        // -----------------------------------------------------------------------
        // Inspector References – Game HUD
        // -----------------------------------------------------------------------

        [Header("Game HUD")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private Button pauseButton;

        // -----------------------------------------------------------------------
        // Inspector References – Pause Panel
        // -----------------------------------------------------------------------

        [Header("Pause Panel")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButtonPause;
        [SerializeField] private Button mainMenuButtonPause;

        // -----------------------------------------------------------------------
        // Inspector References – Game Over Panel
        // -----------------------------------------------------------------------

        [Header("Game Over Panel")]
        [SerializeField] private TextMeshProUGUI gameOverScoreText;
        [SerializeField] private TextMeshProUGUI gameOverMovesText;
        [SerializeField] private TextMeshProUGUI gameOverMatchesText;
        [SerializeField] private Button restartButtonGameOver;
        [SerializeField] private Button mainMenuButtonGameOver;

        // -----------------------------------------------------------------------
        // Inspector References – Victory Panel
        // -----------------------------------------------------------------------

        [Header("Victory Panel")]
        [SerializeField] private TextMeshProUGUI victoryScoreText;
        [SerializeField] private TextMeshProUGUI victoryMovesText;
        [SerializeField] private TextMeshProUGUI victoryTimeText;
        [SerializeField] private TextMeshProUGUI victoryTimeBonusText;
        [SerializeField] private Button restartButtonVictory;
        [SerializeField] private Button mainMenuButtonVictory;

        // -----------------------------------------------------------------------
        // Inspector References – Combo Popup
        // -----------------------------------------------------------------------

        [Header("Combo Popup")]
        [Tooltip("Small floating text that appears on combo matches")]
        [SerializeField] private TextMeshProUGUI comboPopupText;
        [SerializeField] private float comboPopupDuration = 1.2f;

        private Coroutine comboPopupCoroutine;

        // -----------------------------------------------------------------------
        // Unity Lifecycle
        // -----------------------------------------------------------------------

        private void Awake()
        {
            SetupButtonListeners();
            HideAllPanels();
        }

        private void OnDestroy()
        {
            RemoveButtonListeners();
        }

        // -----------------------------------------------------------------------
        // Button Setup
        // -----------------------------------------------------------------------

        private void SetupButtonListeners()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (restartButtonPause != null) restartButtonPause.onClick.AddListener(OnRestartClicked);
            if (mainMenuButtonPause != null) mainMenuButtonPause.onClick.AddListener(OnMainMenuClicked);
            if (restartButtonGameOver != null) restartButtonGameOver.onClick.AddListener(OnRestartClicked);
            if (mainMenuButtonGameOver != null) mainMenuButtonGameOver.onClick.AddListener(OnMainMenuClicked);
            if (restartButtonVictory != null) restartButtonVictory.onClick.AddListener(OnRestartClicked);
            if (mainMenuButtonVictory != null) mainMenuButtonVictory.onClick.AddListener(OnMainMenuClicked);

            if (difficultyButtons != null)
            {
                for (int i = 0; i < difficultyButtons.Length; i++)
                {
                    int index = i; // capture for lambda
                    if (difficultyButtons[i] != null)
                        difficultyButtons[i].onClick.AddListener(() => OnDifficultyClicked(index));
                }
            }
        }

        private void RemoveButtonListeners()
        {
            if (playButton != null) playButton.onClick.RemoveAllListeners();
            if (pauseButton != null) pauseButton.onClick.RemoveAllListeners();
            if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
            if (restartButtonPause != null) restartButtonPause.onClick.RemoveAllListeners();
            if (mainMenuButtonPause != null) mainMenuButtonPause.onClick.RemoveAllListeners();
            if (restartButtonGameOver != null) restartButtonGameOver.onClick.RemoveAllListeners();
            if (mainMenuButtonGameOver != null) mainMenuButtonGameOver.onClick.RemoveAllListeners();
            if (restartButtonVictory != null) restartButtonVictory.onClick.RemoveAllListeners();
            if (mainMenuButtonVictory != null) mainMenuButtonVictory.onClick.RemoveAllListeners();

            if (difficultyButtons != null)
            {
                foreach (Button btn in difficultyButtons)
                {
                    if (btn != null) btn.onClick.RemoveAllListeners();
                }
            }
        }

        // -----------------------------------------------------------------------
        // Button Handlers
        // -----------------------------------------------------------------------

        private void OnPlayClicked() => MemoryMatchGameManager.Instance?.StartGame();
        private void OnPauseClicked() => MemoryMatchGameManager.Instance?.PauseGame();
        private void OnResumeClicked() => MemoryMatchGameManager.Instance?.ResumeGame();
        private void OnRestartClicked() => MemoryMatchGameManager.Instance?.RestartGame();
        private void OnMainMenuClicked() => MemoryMatchGameManager.Instance?.ShowMainMenu();

        private void OnDifficultyClicked(int index)
        {
            MemoryMatchGameManager.Instance?.SetDifficulty(index);
            HighlightSelectedDifficulty(index);
        }

        // -----------------------------------------------------------------------
        // Panel Management
        // -----------------------------------------------------------------------

        public void HideAllPanels()
        {
            SetActive(mainMenuPanel, false);
            SetActive(gamePanel, false);
            SetActive(pausePanel, false);
            SetActive(gameOverPanel, false);
            SetActive(victoryPanel, false);
            if (comboPopupText != null) comboPopupText.gameObject.SetActive(false);
        }

        public void ShowMainMenu()
        {
            HideAllPanels();
            SetActive(mainMenuPanel, true);
            // Restore highlight for the currently selected difficulty
            HighlightSelectedDifficulty(currentDifficultyIndex);
        }

        public void ShowGameUI()
        {
            HideAllPanels();
            SetActive(gamePanel, true);
        }

        public void ShowPauseMenu()
        {
            SetActive(pausePanel, true);
        }

        public void HidePauseMenu()
        {
            SetActive(pausePanel, false);
        }

        public void ShowGameOver(int score, int moves, int matchesFound, int totalPairs)
        {
            SetActive(gameOverPanel, true);
            if (gameOverScoreText != null) gameOverScoreText.text = $"Score: {score}";
            if (gameOverMovesText != null) gameOverMovesText.text = $"Moves: {moves}";
            if (gameOverMatchesText != null) gameOverMatchesText.text = $"Pairs: {matchesFound} / {totalPairs}";
        }

        public void ShowVictory(int score, int moves, float timeRemaining, int timeBonus)
        {
            SetActive(victoryPanel, true);
            if (victoryScoreText != null) victoryScoreText.text = $"Score: {score}";
            if (victoryMovesText != null) victoryMovesText.text = $"Moves: {moves}";
            if (victoryTimeText != null) victoryTimeText.text = $"Time Left: {FormatTime(timeRemaining)}";
            if (victoryTimeBonusText != null) victoryTimeBonusText.text = $"Time Bonus: +{timeBonus}";
        }

        // -----------------------------------------------------------------------
        // HUD Display Updates
        // -----------------------------------------------------------------------

        /// <summary>
        /// Convenience method to update all HUD elements at once.
        /// </summary>
        public void UpdateAll(int score, int moves, int combo, float timeRemaining)
        {
            UpdateScore(score);
            UpdateMoves(moves);
            UpdateCombo(combo);
            UpdateTimer(timeRemaining);
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = $"Score: {score}";
        }

        public void UpdateMoves(int moves)
        {
            if (movesText != null) movesText.text = $"Moves: {moves}";
        }

        public void UpdateCombo(int combo)
        {
            if (comboText != null)
            {
                if (combo >= 2)
                {
                    comboText.text = $"Combo x{combo}";
                    comboText.gameObject.SetActive(true);
                }
                else
                {
                    comboText.gameObject.SetActive(false);
                }
            }
        }

        public void UpdateTimer(float seconds)
        {
            if (timerText != null) timerText.text = FormatTime(seconds);
        }

        /// <summary>
        /// Shows a floating combo popup with a scale animation, then hides it.
        /// </summary>
        public void ShowComboPopup(int combo, float multiplier)
        {
            if (comboPopupText == null) return;

            if (combo < 2) return; // only show for actual combos

            if (comboPopupCoroutine != null) StopCoroutine(comboPopupCoroutine);
            comboPopupCoroutine = StartCoroutine(ComboPopupAnimation(combo, multiplier));
        }

        // -----------------------------------------------------------------------
        // Coroutines
        // -----------------------------------------------------------------------

        /// <summary>
        /// Shows a combo popup text with a scale-in / scale-out tween, then hides it.
        /// </summary>
        private IEnumerator ComboPopupAnimation(int combo, float multiplier)
        {
            comboPopupText.text = $"COMBO x{combo}!\n+{multiplier:F1}x";
            comboPopupText.gameObject.SetActive(true);

            Vector3 originalScale = Vector3.one;
            Vector3 targetScale = originalScale * 1.3f;

            float halfDuration = comboPopupDuration * 0.2f;

            // Scale in
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                comboPopupText.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
                yield return null;
            }

            // Hold
            yield return new WaitForSeconds(comboPopupDuration * 0.6f);

            // Scale out
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                comboPopupText.transform.localScale = Vector3.Lerp(targetScale, Vector3.zero, t);
                yield return null;
            }

            comboPopupText.gameObject.SetActive(false);
            comboPopupText.transform.localScale = originalScale;
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Highlights the selected difficulty button with a gold color + larger scale,
        /// restores the others to their base colors.
        /// Modifies the Button's ColorBlock.normalColor so it persists through Unity's
        /// transition system (avoids being overridden by ColorTint).
        /// </summary>
        public void HighlightSelectedDifficulty(int selectedIndex)
        {
            currentDifficultyIndex = selectedIndex;

            if (difficultyButtons == null) return;

            for (int i = 0; i < difficultyButtons.Length; i++)
            {
                if (difficultyButtons[i] == null) continue;

                Button btn = difficultyButtons[i];
                RectTransform rt = btn.GetComponent<RectTransform>();
                TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();

                Color targetColor = i < DiffBaseColors.Length ? DiffBaseColors[i] : Color.gray;
                bool isSelected = (i == selectedIndex);

                if (isSelected) targetColor = ColorSelected;

                // Update ColorBlock so Button's ColorTint transition uses the new normal color
                ColorBlock cb = btn.colors;
                cb.normalColor      = targetColor;
                cb.highlightedColor = targetColor * 1.15f;
                cb.pressedColor     = targetColor * 0.75f;
                cb.selectedColor    = targetColor;
                cb.colorMultiplier  = 1f;
                btn.colors = cb;

                // Scale: selected is slightly larger
                if (rt != null)
                    rt.localScale = isSelected ? Vector3.one * 1.08f : Vector3.one;

                // Text color: dark text on gold, white on colored backgrounds
                if (label != null)
                    label.color = isSelected ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
            }
        }

        private void SetActive(GameObject obj, bool active)
        {
            if (obj != null) obj.SetActive(active);
        }

        private string FormatTime(float seconds)
        {
            int mins = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{mins:00}:{secs:00}";
        }
    }
}
