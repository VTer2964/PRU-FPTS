using UnityEngine;

namespace CountryGuessGame.Managers
{
    /// <summary>
    /// Enum representing different game states
    /// </summary>
    public enum CountryGameState
    {
        Menu,
        Playing,
        Paused,
        LevelComplete,
        GameOver
    }

    /// <summary>
    /// Singleton GameManager that controls the overall game flow for Country Guess Game
    /// </summary>
    public class CountryGameManager : MonoBehaviour
    {
        public static CountryGameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private CountryGameState currentState = CountryGameState.Menu;

        [Header("Current Level")]
        [SerializeField] private int currentLevelIndex = 0;

        // Events
        public System.Action<CountryGameState> OnGameStateChanged;
        public System.Action<int> OnLevelChanged;

        public CountryGameState CurrentState => currentState;
        public int CurrentLevelIndex => currentLevelIndex;

        private void Awake()
        {
            // Singleton pattern (no DontDestroyOnLoad — scoped to minigame scene)
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
            // Load saved progress
            LoadProgress();
        }

        /// <summary>
        /// Change the current game state
        /// </summary>
        public void SetGameState(CountryGameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);

            Debug.Log($"Country Game State Changed: {currentState}");
        }

        /// <summary>
        /// Start a new level
        /// </summary>
        public void StartLevel(int levelIndex)
        {
            currentLevelIndex = levelIndex;
            OnLevelChanged?.Invoke(currentLevelIndex);
            SetGameState(CountryGameState.Playing);

            Debug.Log($"Starting Country Guess Level {currentLevelIndex}");
        }

        /// <summary>
        /// Complete current level and move to next
        /// </summary>
        public void CompleteLevel()
        {
            SetGameState(CountryGameState.LevelComplete);
            SaveProgress();
        }

        /// <summary>
        /// Move to next level
        /// </summary>
        public void NextLevel()
        {
            currentLevelIndex++;
            StartLevel(currentLevelIndex);
        }

        /// <summary>
        /// Retry current level
        /// </summary>
        public void RetryLevel()
        {
            StartLevel(currentLevelIndex);
        }

        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMenu()
        {
            SetGameState(CountryGameState.Menu);
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            if (currentState == CountryGameState.Playing)
            {
                SetGameState(CountryGameState.Paused);
                Time.timeScale = 0f;
            }
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        public void ResumeGame()
        {
            if (currentState == CountryGameState.Paused)
            {
                SetGameState(CountryGameState.Playing);
                Time.timeScale = 1f;
            }
        }

        /// <summary>
        /// Save game progress using PlayerPrefs
        /// </summary>
        private void SaveProgress()
        {
            PlayerPrefs.SetInt("CountryGame_CurrentLevel", currentLevelIndex);
            PlayerPrefs.Save();
            Debug.Log($"Country Game Progress saved: Level {currentLevelIndex}");
        }

        /// <summary>
        /// Load game progress from PlayerPrefs
        /// </summary>
        private void LoadProgress()
        {
            currentLevelIndex = PlayerPrefs.GetInt("CountryGame_CurrentLevel", 0);
            Debug.Log($"Country Game Progress loaded: Level {currentLevelIndex}");
        }

        /// <summary>
        /// Reset all progress
        /// </summary>
        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey("CountryGame_CurrentLevel");
            PlayerPrefs.DeleteKey("CountryGame_TotalScore");
            currentLevelIndex = 0;
            Debug.Log("Country Game Progress reset!");
        }

        private void OnApplicationQuit()
        {
            SaveProgress();
        }
    }
}
