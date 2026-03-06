using UnityEngine;
using CountryGuessGame.Data;
using CountryGuessGame.Core;

namespace CountryGuessGame.Managers
{
    /// <summary>
    /// Manages level loading, progression, and completion for Country Guess Game
    /// </summary>
    public class CountryLevelController : MonoBehaviour
    {
        [Header("Level Data")]
        [SerializeField] private CountryLevelData[] allLevels;
        [SerializeField] private CountryDatabase countryDatabase;

        [Header("Controllers")]
        [SerializeField] private CountryMatchingController matchingController;

        // Current level
        private CountryLevelData currentLevel;
        private int currentLevelIndex = 0;

        // References
        private CountryGameManager gameManager;
        private CountryScoreManager scoreManager;
        private CountryUIManager uiManager;

        // Events
        public System.Action<CountryLevelData> OnLevelStarted;
        public System.Action<CountryGuessGame.Data.MedalType, int> OnLevelCompleted;

        private void Awake()
        {
            gameManager = CountryGameManager.Instance;
            scoreManager = FindObjectOfType<CountryScoreManager>();
            uiManager = FindObjectOfType<CountryUIManager>();
        }

        private void Start()
        {
            // Subscribe to game manager events
            if (gameManager != null)
            {
                gameManager.OnLevelChanged += OnLevelChanged;
            }

            // Load first level
            LoadLevel(0);
        }

        /// <summary>
        /// Called when level changes in GameManager
        /// </summary>
        private void OnLevelChanged(int levelIndex)
        {
            LoadLevel(levelIndex);
        }

        /// <summary>
        /// Load a specific level
        /// </summary>
        public void LoadLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= allLevels.Length)
            {
                Debug.LogError($"Invalid level index: {levelIndex}");
                return;
            }

            currentLevelIndex = levelIndex;
            currentLevel = allLevels[levelIndex];

            // Reset score
            if (scoreManager != null)
            {
                scoreManager.ResetScore();
            }

            // Initialize matching controller
            if (matchingController != null && countryDatabase != null)
            {
                matchingController.InitializeLevel(currentLevel, countryDatabase);
            }

            // Update UI
            if (uiManager != null)
            {
                uiManager.ShowGameUI();
                uiManager.UpdateLevelInfo(currentLevel.LevelNumber, currentLevel.LevelName);
            }

            OnLevelStarted?.Invoke(currentLevel);

            Debug.Log($"Loaded Country Guess Level {currentLevel.LevelNumber}: {currentLevel.LevelName}");
        }

        /// <summary>
        /// Complete the current level
        /// </summary>
        public void CompleteLevel()
        {
            if (currentLevel == null || scoreManager == null)
            {
                Debug.LogError("Cannot complete level: Missing references");
                return;
            }

            // Calculate medal
            CountryGuessGame.Data.MedalType medal = scoreManager.CalculateMedal(currentLevel);
            int medalPoints = scoreManager.GetMedalPoints(medal, currentLevel);

            // Add to total score
            scoreManager.AddToTotalScore(medalPoints);

            // Show result UI
            if (uiManager != null)
            {
                uiManager.ShowResultPanel(
                    medal,
                    scoreManager.CurrentScore,
                    scoreManager.Accuracy,
                    scoreManager.CorrectMatches,
                    scoreManager.WrongMatches,
                    currentLevelIndex < allLevels.Length - 1
                );
            }

            OnLevelCompleted?.Invoke(medal, medalPoints);

            Debug.Log($"Level Completed! Medal: {medal}, Points: {medalPoints}, Accuracy: {scoreManager.Accuracy:F1}%");
        }

        /// <summary>
        /// Retry current level
        /// </summary>
        public void RetryCurrentLevel()
        {
            if (gameManager != null)
            {
                gameManager.RetryLevel();
            }
        }

        /// <summary>
        /// Move to next level
        /// </summary>
        public void GoToNextLevel()
        {
            if (currentLevelIndex < allLevels.Length - 1)
            {
                if (gameManager != null)
                {
                    gameManager.NextLevel();
                }
            }
            else
            {
                Debug.Log("All levels completed!");
            }
        }

        /// <summary>
        /// Get current level data
        /// </summary>
        public CountryLevelData GetCurrentLevel() => currentLevel;
    }
}
