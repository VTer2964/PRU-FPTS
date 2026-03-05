using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CaroGame
{
    public class CaroUIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private CaroResultsPanel resultsPanel;

        [Header("Main Menu")]
        [SerializeField] private Button playButton;

        [Header("Game UI")]
        [SerializeField] private CaroTimerDisplay timerDisplay;
        [SerializeField] private CaroMoveCounter moveCounter;
        [SerializeField] private TextMeshProUGUI turnIndicator;
        [SerializeField] private Button hintToggleButton;
        [SerializeField] private TextMeshProUGUI hintButtonText;

        public event Action OnPlayClicked;
        public event Action OnRestartClicked;
        public event Action OnMenuClicked;
        public event Action OnHintToggleClicked;

        private void Awake()
        {
            if (playButton != null)
                playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());

            if (hintToggleButton != null)
                hintToggleButton.onClick.AddListener(() => OnHintToggleClicked?.Invoke());

            // Wire results panel buttons directly with callbacks
            if (resultsPanel != null)
            {
                resultsPanel.Init(
                    () => { Debug.Log("[CaroGame] UIManager: restart forwarded"); OnRestartClicked?.Invoke(); },
                    () => { Debug.Log("[CaroGame] UIManager: menu forwarded"); OnMenuClicked?.Invoke(); }
                );
            }
        }

        public void ShowMainMenu()
        {
            SetPanel(mainMenu: true, game: false, results: false);
        }

        public void ShowGamePanel()
        {
            SetPanel(mainMenu: false, game: true, results: false);
            if (moveCounter != null)
                moveCounter.ResetCounter();
            if (timerDisplay != null)
                timerDisplay.ResetDisplay();
        }

        public void ShowResults(int winner, int playerMoves, CaroMedalType medal, bool timedOut)
        {
            if (resultsPanel != null)
                resultsPanel.ShowResults(winner, playerMoves, medal, timedOut);
        }

        public void UpdateMoveCount(int moves)
        {
            if (moveCounter != null)
                moveCounter.UpdateMoveCount(moves);
        }

        public void SetTurnIndicator(string text)
        {
            if (turnIndicator != null)
                turnIndicator.text = text;
        }

        public void SetHintButtonState(bool hintsEnabled)
        {
            if (hintButtonText != null)
                hintButtonText.text = hintsEnabled ? "Hints: ON" : "Hints: OFF";
        }

        private void SetPanel(bool mainMenu, bool game, bool results)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(mainMenu);
            if (gamePanel != null) gamePanel.SetActive(game);
            if (!results && resultsPanel != null) resultsPanel.Hide();
        }

        private void OnDestroy()
        {
            if (playButton != null)
                playButton.onClick.RemoveAllListeners();
            if (hintToggleButton != null)
                hintToggleButton.onClick.RemoveAllListeners();
        }
    }
}
