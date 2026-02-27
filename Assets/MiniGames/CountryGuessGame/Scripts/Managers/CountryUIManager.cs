using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CountryGuessGame.Data;

namespace CountryGuessGame.Managers
{
    /// <summary>
    /// Manages UI updates and panel visibility for Country Guess Game
    /// </summary>
    public class CountryUIManager : MonoBehaviour
    {
        [Header("Game UI")]
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI questionCounterText;

        [Header("Result Panel")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI medalText;
        [SerializeField] private Image medalImage;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI accuracyText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextLevelButton;

        [Header("Medal Sprites")]
        [SerializeField] private Sprite goldMedalSprite;
        [SerializeField] private Sprite silverMedalSprite;
        [SerializeField] private Sprite bronzeMedalSprite;

        [Header("Pause Panel")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;

        // References
        private CountryScoreManager scoreManager;
        private CountryLevelController levelController;
        private CountryGameManager gameManager;

        private void Awake()
        {
            scoreManager = FindObjectOfType<CountryScoreManager>();
            levelController = FindObjectOfType<CountryLevelController>();
            gameManager = CountryGameManager.Instance;

            // Subscribe to score changes
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += UpdateScore;
            }

            // Setup button listeners
            SetupButtons();

            // Hide panels initially
            if (resultPanel != null) resultPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        private void SetupButtons()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryClicked);
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        /// <summary>
        /// Update level info display
        /// </summary>
        public void UpdateLevelInfo(int levelNumber, string levelName)
        {
            if (levelText != null)
            {
                levelText.text = $"Level {levelNumber}: {levelName}";
            }
        }

        /// <summary>
        /// Update score display
        /// </summary>
        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        /// <summary>
        /// Update question counter (e.g., "Question 3/10")
        /// </summary>
        public void UpdateQuestionCounter(int currentQuestion, int totalQuestions)
        {
            if (questionCounterText != null)
            {
                questionCounterText.text = $"Question {currentQuestion}/{totalQuestions}";
            }
        }

        /// <summary>
        /// Show game UI
        /// </summary>
        public void ShowGameUI()
        {
            if (gamePanel != null)
            {
                gamePanel.SetActive(true);
            }

            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Show result panel with medal and stats
        /// </summary>
        public void ShowResultPanel(CountryGuessGame.Data.MedalType medal, int finalScore, float accuracy, int correct, int wrong, bool hasNextLevel)
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            // Update medal
            if (medalText != null)
            {
                medalText.text = $"{medal} Medal!";
            }

            if (medalImage != null)
            {
                medalImage.sprite = GetMedalSprite(medal);
            }

            // Update stats
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {finalScore}";
            }

            if (accuracyText != null)
            {
                accuracyText.text = $"Accuracy: {accuracy:F1}%";
            }

            if (statsText != null)
            {
                statsText.text = $"Correct: {correct} | Wrong: {wrong}";
            }

            // Show/hide next level button
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(hasNextLevel);
            }
        }

        /// <summary>
        /// Get medal sprite based on type
        /// </summary>
        private Sprite GetMedalSprite(CountryGuessGame.Data.MedalType medal)
        {
            switch (medal)
            {
                case CountryGuessGame.Data.MedalType.Gold:
                    return goldMedalSprite;
                case CountryGuessGame.Data.MedalType.Silver:
                    return silverMedalSprite;
                case CountryGuessGame.Data.MedalType.Bronze:
                    return bronzeMedalSprite;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Show pause panel
        /// </summary>
        public void ShowPausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hide pause panel
        /// </summary>
        public void HidePausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        // Button Handlers
        private void OnRetryClicked()
        {
            Debug.Log("[CountryUIManager] Retry button clicked");

            if (levelController == null)
            {
                levelController = FindObjectOfType<CountryLevelController>();
            }

            if (levelController != null)
            {
                // Hide result panel first
                if (resultPanel != null) resultPanel.SetActive(false);
                levelController.LoadLevel(levelController.GetCurrentLevel().LevelNumber - 1);
            }
            else
            {
                Debug.LogError("[CountryUIManager] LevelController not found!");
            }
        }

        private void OnNextLevelClicked()
        {
            Debug.Log("[CountryUIManager] Next Level button clicked");

            if (levelController == null)
            {
                levelController = FindObjectOfType<CountryLevelController>();
            }

            if (levelController != null)
            {
                // Hide result panel first
                if (resultPanel != null) resultPanel.SetActive(false);
                int nextLevelIndex = levelController.GetCurrentLevel().LevelNumber; // LevelNumber is 1-based, index is 0-based
                levelController.LoadLevel(nextLevelIndex);
            }
            else
            {
                Debug.LogError("[CountryUIManager] LevelController not found!");
            }
        }

        private void OnResumeClicked()
        {
            if (gameManager != null)
            {
                gameManager.ResumeGame();
                HidePausePanel();
            }
        }

        private void OnQuitClicked()
        {
            if (gameManager != null)
            {
                gameManager.ReturnToMenu();
            }
        }
    }
}
