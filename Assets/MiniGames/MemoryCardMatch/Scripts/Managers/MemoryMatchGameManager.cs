using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MemoryCardMatch
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Victory,
        GameOver
    }

    public class MemoryMatchGameManager : MonoBehaviour
    {
        public static MemoryMatchGameManager Instance { get; private set; }

        // -----------------------------------------------------------------------
        // Inspector References
        // -----------------------------------------------------------------------

        [Header("References")]
        [Tooltip("Settings ScriptableObject with difficulty configs and timings")]
        [SerializeField] private MemoryMatchSettings settings;

        [Tooltip("Card database containing all available card data")]
        [SerializeField] private CardDatabase cardDatabase;

        [Tooltip("Spawner responsible for creating the card grid")]
        [SerializeField] private CardGridSpawner gridSpawner;

        [Tooltip("UI manager for panel switching and display updates")]
        [SerializeField] private MemoryMatchUIManager uiManager;

        [Header("Events")]
        public UnityEvent OnGameStart;
        public UnityEvent OnGamePause;
        public UnityEvent OnGameResume;
        public UnityEvent OnVictory;
        public UnityEvent OnGameOver;

        // -----------------------------------------------------------------------
        // State
        // -----------------------------------------------------------------------

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        private DifficultyLevel selectedDifficulty = DifficultyLevel.Medium;
        private DifficultySettings activeSettings;

        // Cards
        private List<CardController> allCards = new List<CardController>();
        private CardController firstCard;
        private CardController secondCard;
        private bool isProcessingPair = false;

        // Stats
        public int Score { get; private set; }
        public int MoveCount { get; private set; }
        public int MatchesFound { get; private set; }
        public int ComboCount { get; private set; }
        public float TimeRemaining { get; private set; }

        private int totalPairs;
        private Coroutine timerCoroutine;
        private Coroutine mismatchCoroutine;

        // -----------------------------------------------------------------------
        // Unity Lifecycle
        // -----------------------------------------------------------------------

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // -----------------------------------------------------------------------
        // Game State Transitions
        // -----------------------------------------------------------------------

        public void ShowMainMenu()
        {
            CurrentState = GameState.MainMenu;
            StopTimerIfRunning();
            uiManager?.ShowMainMenu();
        }

        public void SetDifficulty(int difficultyIndex)
        {
            selectedDifficulty = (DifficultyLevel)difficultyIndex;
            // UIManager.HighlightSelectedDifficulty is already called by the button handler,
            // but also sync here in case SetDifficulty is called programmatically.
            uiManager?.HighlightSelectedDifficulty(difficultyIndex);
            Debug.Log($"[GameManager] Difficulty set to {selectedDifficulty}");
        }

        public void StartGame()
        {
            ResetStats();

            activeSettings = settings.GetSettingsForDifficulty(selectedDifficulty);
            TimeRemaining = activeSettings.timeLimit;
            totalPairs = (activeSettings.gridRows * activeSettings.gridColumns) / 2;

            // Spawn cards
            allCards = gridSpawner.SpawnGrid(activeSettings, settings.cardFlipDuration);

            CurrentState = GameState.Playing;
            uiManager?.ShowGameUI();
            uiManager?.UpdateAll(Score, MoveCount, ComboCount, TimeRemaining);

            // Start countdown timer (only if there's a time limit)
            if (activeSettings.timeLimit > 0)
            {
                timerCoroutine = StartCoroutine(TimerCountdown());
            }

            OnGameStart?.Invoke();
            Debug.Log($"[GameManager] Game started. Difficulty={selectedDifficulty}, Pairs={totalPairs}");
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing) return;

            CurrentState = GameState.Paused;
            StopTimerIfRunning();
            uiManager?.ShowPauseMenu();
            OnGamePause?.Invoke();
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;

            CurrentState = GameState.Playing;
            uiManager?.HidePauseMenu();

            if (activeSettings.timeLimit > 0)
            {
                timerCoroutine = StartCoroutine(TimerCountdown());
            }

            OnGameResume?.Invoke();
        }

        public void RestartGame()
        {
            StopTimerIfRunning();
            StopMismatchIfRunning();
            firstCard = null;
            secondCard = null;
            isProcessingPair = false;
            StartGame();
        }

        // -----------------------------------------------------------------------
        // Card Click Handling
        // -----------------------------------------------------------------------

        /// <summary>
        /// Called by CardController when the player clicks a card.
        /// Implements the two-card match logic.
        /// </summary>
        public void OnCardClicked(CardController card)
        {
            if (CurrentState != GameState.Playing) return;
            if (isProcessingPair) return;
            if (card == firstCard) return; // same card clicked twice

            card.FlipFaceUp();

            if (firstCard == null)
            {
                // First card of the pair
                firstCard = card;
            }
            else
            {
                // Second card of the pair
                secondCard = card;
                MoveCount++;
                uiManager?.UpdateMoves(MoveCount);

                isProcessingPair = true;
                mismatchCoroutine = StartCoroutine(EvaluatePair());
            }
        }

        // -----------------------------------------------------------------------
        // Pair Evaluation
        // -----------------------------------------------------------------------

        /// <summary>
        /// Wait a short moment, then check if the two flipped cards match.
        /// If they do: mark matched, award score.
        /// If not: flip both back after a delay.
        /// </summary>
        private IEnumerator EvaluatePair()
        {
            // Brief pause so the player can see both cards
            yield return new WaitForSeconds(0.1f);

            bool isMatch = firstCard.Data.cardId == secondCard.Data.cardId;

            if (isMatch)
            {
                HandleMatch();
            }
            else
            {
                yield return StartCoroutine(HandleMismatch());
            }

            firstCard = null;
            secondCard = null;
            isProcessingPair = false;
        }

        private void HandleMatch()
        {
            MatchesFound++;
            ComboCount++;

            // Apply combo multiplier
            float multiplier = ComboCount >= 3 ? settings.comboMultiplier3Plus
                             : ComboCount == 2 ? settings.comboMultiplier2
                             : 1f;

            int gained = Mathf.RoundToInt(settings.baseMatchScore * multiplier);
            Score += gained;

            firstCard.SetMatched();
            secondCard.SetMatched();

            uiManager?.UpdateAll(Score, MoveCount, ComboCount, TimeRemaining);
            uiManager?.ShowComboPopup(ComboCount, multiplier);

            Debug.Log($"[GameManager] Match! Combo={ComboCount}, +{gained} pts, Total={Score}");

            // Check win condition
            if (MatchesFound >= totalPairs)
            {
                StartCoroutine(TriggerVictory());
            }
        }

        private IEnumerator HandleMismatch()
        {
            ComboCount = 0; // reset combo on mismatch
            uiManager?.UpdateAll(Score, MoveCount, ComboCount, TimeRemaining);

            // Display mismatched cards for configured duration
            yield return new WaitForSeconds(settings.mismatchDisplayTime);

            firstCard.FlipFaceDown();
            secondCard.FlipFaceDown();
        }

        // -----------------------------------------------------------------------
        // Victory / Game Over
        // -----------------------------------------------------------------------

        private IEnumerator TriggerVictory()
        {
            CurrentState = GameState.Victory;
            StopTimerIfRunning();

            // Time bonus
            int timeBonus = Mathf.RoundToInt(TimeRemaining * settings.timeBonusPerSecond);
            Score += timeBonus;

            // Wait a moment for match effects to finish
            yield return new WaitForSeconds(0.6f);

            uiManager?.ShowVictory(Score, MoveCount, TimeRemaining, timeBonus);
            OnVictory?.Invoke();
            Debug.Log($"[GameManager] Victory! Final Score={Score}, Moves={MoveCount}, TimeBonus={timeBonus}");
        }

        private void TriggerGameOver()
        {
            CurrentState = GameState.GameOver;
            StopMismatchIfRunning();
            uiManager?.ShowGameOver(Score, MoveCount, MatchesFound, totalPairs);
            OnGameOver?.Invoke();
            Debug.Log($"[GameManager] Game Over! Score={Score}, Matches={MatchesFound}/{totalPairs}");
        }

        // -----------------------------------------------------------------------
        // Timer Coroutine
        // -----------------------------------------------------------------------

        /// <summary>
        /// Counts down TimeRemaining to zero, then triggers Game Over.
        /// Updates the UI timer display every frame.
        /// </summary>
        private IEnumerator TimerCountdown()
        {
            while (TimeRemaining > 0f)
            {
                yield return null; // wait one frame
                TimeRemaining -= Time.deltaTime;
                TimeRemaining = Mathf.Max(0f, TimeRemaining);
                uiManager?.UpdateTimer(TimeRemaining);
            }

            if (CurrentState == GameState.Playing)
            {
                TriggerGameOver();
            }
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private void ResetStats()
        {
            Score = 0;
            MoveCount = 0;
            MatchesFound = 0;
            ComboCount = 0;
            TimeRemaining = 0f;
            firstCard = null;
            secondCard = null;
            isProcessingPair = false;
        }

        private void StopTimerIfRunning()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }

        private void StopMismatchIfRunning()
        {
            if (mismatchCoroutine != null)
            {
                StopCoroutine(mismatchCoroutine);
                mismatchCoroutine = null;
            }
        }
    }
}
