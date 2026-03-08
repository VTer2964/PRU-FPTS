using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CountryGuessGame.Data;
using CountryGuessGame.Managers;

namespace CountryGuessGame.Core
{
    /// <summary>
    /// Main game logic controller for Country Guess Game
    /// </summary>
    public class CountryMatchingController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CountryDatabase countryDatabase;
        [SerializeField] private CountryLevelData currentLevelData;

        [Header("UI References")]
        [SerializeField] private FlagDisplay flagDisplay;
        [SerializeField] private List<AnswerButton> answerButtons = new List<AnswerButton>();

        [Header("Settings")]
        [SerializeField] private float delayBeforeNextQuestion = 1.5f;

        // State
        private List<CountryData> levelCountries;
        private int currentQuestionIndex = 0;
        private CountryData currentCorrectCountry;
        private bool isProcessingAnswer = false;

        // References to managers
        private CountryScoreManager scoreManager;
        private CountryLevelController levelController;
        private CountryUIManager uiManager;

        // Events
        public System.Action OnQuestionStarted;
        public System.Action<bool> OnAnswerSubmitted;
        public System.Action OnAllQuestionsCompleted;

        private void Awake()
        {
            // Get manager references
            scoreManager = FindObjectOfType<CountryScoreManager>();
            levelController = FindObjectOfType<CountryLevelController>();
            uiManager = FindObjectOfType<CountryUIManager>();
        }

        /// <summary>
        /// Initialize the game with a level
        /// </summary>
        public void InitializeLevel(CountryLevelData levelData, CountryDatabase database)
        {
            currentLevelData = levelData;
            countryDatabase = database;

            // Get random countries for this level
            levelCountries = countryDatabase.GetRandomCountries(
                currentLevelData.QuestionCount,
                currentLevelData.MaxDifficulty
            );

            currentQuestionIndex = 0;

            if (levelCountries.Count > 0)
            {
                StartNextQuestion();
            }
            else
            {
                Debug.LogError("No countries available for this level!");
            }
        }

        /// <summary>
        /// Start the next question
        /// </summary>
        public void StartNextQuestion()
        {
            if (currentQuestionIndex >= levelCountries.Count)
            {
                // All questions completed
                CompleteLevel();
                return;
            }

            // Get current country
            currentCorrectCountry = levelCountries[currentQuestionIndex];

            // Display flag
            if (flagDisplay != null)
            {
                flagDisplay.SetFlag(currentCorrectCountry.FlagSprite);
            }

            // Generate answer options
            GenerateAnswerOptions();

            // Update UI
            if (uiManager != null)
            {
                uiManager.UpdateQuestionCounter(currentQuestionIndex + 1, levelCountries.Count);
            }

            isProcessingAnswer = false;

            OnQuestionStarted?.Invoke();
        }

        /// <summary>
        /// Generate 4 answer options (1 correct + 3 wrong)
        /// </summary>
        private void GenerateAnswerOptions()
        {
            // Get 3 distractor countries
            List<CountryData> distractors = countryDatabase.GetDistractorCountries(
                currentCorrectCountry,
                currentLevelData.AnswerOptionsCount - 1,
                currentLevelData.MaxDifficulty
            );

            // Create answer list
            List<CountryData> allAnswers = new List<CountryData> { currentCorrectCountry };
            allAnswers.AddRange(distractors);

            // Shuffle answers
            allAnswers = allAnswers.OrderBy(x => Random.value).ToList();

            // Setup answer buttons
            for (int i = 0; i < answerButtons.Count && i < allAnswers.Count; i++)
            {
                CountryData country = allAnswers[i];
                bool isCorrect = (country == currentCorrectCountry);

                answerButtons[i].SetupButton(
                    country.GetDisplayName(false), // Use English name
                    isCorrect,
                    OnAnswerSelected
                );
            }
        }

        /// <summary>
        /// Handle answer selection
        /// </summary>
        private void OnAnswerSelected(bool isCorrect)
        {
            if (isProcessingAnswer) return;
            isProcessingAnswer = true;

            // Update score
            if (scoreManager != null)
            {
                if (isCorrect)
                {
                    scoreManager.AddCorrectMatch(currentLevelData.PointsPerCorrect);
                }
                else
                {
                    scoreManager.AddWrongMatch(currentLevelData.PointsPerWrong);

                    // Highlight correct answer
                    foreach (var btn in answerButtons)
                    {
                        btn.HighlightCorrect();
                    }
                }
            }

            // Disable all buttons
            foreach (var btn in answerButtons)
            {
                btn.DisableButton();
            }

            OnAnswerSubmitted?.Invoke(isCorrect);

            // Move to next question after delay
            StartCoroutine(MoveToNextQuestionAfterDelay());
        }

        /// <summary>
        /// Move to next question after a delay
        /// </summary>
        private IEnumerator MoveToNextQuestionAfterDelay()
        {
            yield return new WaitForSeconds(delayBeforeNextQuestion);

            currentQuestionIndex++;
            StartNextQuestion();
        }

        /// <summary>
        /// Complete the level
        /// </summary>
        private void CompleteLevel()
        {
            OnAllQuestionsCompleted?.Invoke();

            if (levelController != null)
            {
                levelController.CompleteLevel();
            }
        }

        /// <summary>
        /// Get current progress
        /// </summary>
        public int GetCurrentQuestionIndex() => currentQuestionIndex;
        public int GetTotalQuestions() => levelCountries?.Count ?? 0;
    }
}
