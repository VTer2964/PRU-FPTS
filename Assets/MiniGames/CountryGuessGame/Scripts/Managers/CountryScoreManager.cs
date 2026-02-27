using UnityEngine;
using CountryGuessGame.Data;

namespace CountryGuessGame.Managers
{
    /// <summary>
    /// Manages scoring, accuracy tracking, and medal calculation for Country Guess Game
    /// </summary>
    public class CountryScoreManager : MonoBehaviour
    {
        [Header("Current Session")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int correctMatches = 0;
        [SerializeField] private int wrongMatches = 0;
        [SerializeField] private int totalAttempts = 0;

        // Events
        public System.Action<int> OnScoreChanged;
        public System.Action<float> OnAccuracyChanged;

        public int CurrentScore => currentScore;
        public int CorrectMatches => correctMatches;
        public int WrongMatches => wrongMatches;
        public float Accuracy => totalAttempts > 0 ? (float)correctMatches / totalAttempts * 100f : 0f;

        /// <summary>
        /// Reset score for new level
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            correctMatches = 0;
            wrongMatches = 0;
            totalAttempts = 0;

            OnScoreChanged?.Invoke(currentScore);
            OnAccuracyChanged?.Invoke(Accuracy);
        }

        /// <summary>
        /// Add points for correct match
        /// </summary>
        public void AddCorrectMatch(int points)
        {
            correctMatches++;
            totalAttempts++;
            currentScore += points;

            OnScoreChanged?.Invoke(currentScore);
            OnAccuracyChanged?.Invoke(Accuracy);

            Debug.Log($"Correct! Score: {currentScore}, Accuracy: {Accuracy:F1}%");
        }

        /// <summary>
        /// Deduct points for wrong match
        /// </summary>
        public void AddWrongMatch(int penalty)
        {
            wrongMatches++;
            totalAttempts++;
            currentScore = Mathf.Max(0, currentScore + penalty); // Don't go below 0

            OnScoreChanged?.Invoke(currentScore);
            OnAccuracyChanged?.Invoke(Accuracy);

            Debug.Log($"Wrong! Score: {currentScore}, Accuracy: {Accuracy:F1}%");
        }

        /// <summary>
        /// Calculate medal based on level data
        /// </summary>
        public CountryGuessGame.Data.MedalType CalculateMedal(CountryLevelData levelData)
        {
            return levelData.CalculateMedal(Accuracy);
        }

        /// <summary>
        /// Get medal points
        /// </summary>
        public int GetMedalPoints(CountryGuessGame.Data.MedalType medal, CountryLevelData levelData)
        {
            return levelData.GetMedalPoints(medal);
        }

        /// <summary>
        /// Get total score for PlayerPrefs
        /// </summary>
        public int GetTotalScore()
        {
            return PlayerPrefs.GetInt("CountryGame_TotalScore", 0);
        }

        /// <summary>
        /// Add to total score (cumulative across all levels)
        /// </summary>
        public void AddToTotalScore(int medalPoints)
        {
            int totalScore = GetTotalScore();
            totalScore += medalPoints;
            PlayerPrefs.SetInt("CountryGame_TotalScore", totalScore);
            PlayerPrefs.Save();

            Debug.Log($"Country Game Total Score: {totalScore}");
        }

        /// <summary>
        /// Reset total score
        /// </summary>
        public void ResetTotalScore()
        {
            PlayerPrefs.SetInt("CountryGame_TotalScore", 0);
            PlayerPrefs.Save();
        }
    }
}
