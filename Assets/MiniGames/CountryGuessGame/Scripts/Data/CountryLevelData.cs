using UnityEngine;

namespace CountryGuessGame.Data
{
    /// <summary>
    /// Configuration data for each level in Country Guess Game
    /// </summary>
    [CreateAssetMenu(fileName = "CountryLevelData", menuName = "Country Guess Game/Level Data")]
    public class CountryLevelData : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private int levelNumber;
        [SerializeField] private string levelName;

        [Header("Gameplay Settings")]
        [SerializeField] private int questionCount = 5; // Number of countries to guess per level
        [SerializeField] private float timePerQuestion = 30f; // seconds per question
        [SerializeField] private int maxDifficulty = 1; // Max difficulty level for countries

        [Header("Scoring")]
        [SerializeField] private int pointsPerCorrect = 10;
        [SerializeField] private int pointsPerWrong = -5;
        [SerializeField] private float timeBonusMultiplier = 1f; // Bonus points for quick answers

        [Header("Medal Requirements")]
        [SerializeField] private int goldThreshold = 90; // 90% accuracy or higher
        [SerializeField] private int silverThreshold = 70; // 70-89% accuracy
        // Bronze = complete the level (below 70%)

        [Header("Answer Options")]
        [SerializeField] private int answerOptionsCount = 4; // Number of multiple choice options

        // Properties
        public int LevelNumber => levelNumber;
        public string LevelName => levelName;
        public int QuestionCount => questionCount;
        public float TimePerQuestion => timePerQuestion;
        public int MaxDifficulty => maxDifficulty;
        public int PointsPerCorrect => pointsPerCorrect;
        public int PointsPerWrong => pointsPerWrong;
        public float TimeBonusMultiplier => timeBonusMultiplier;
        public int GoldThreshold => goldThreshold;
        public int SilverThreshold => silverThreshold;
        public int AnswerOptionsCount => answerOptionsCount;

        /// <summary>
        /// Calculate medal based on accuracy percentage
        /// </summary>
        public MedalType CalculateMedal(float accuracyPercent)
        {
            if (accuracyPercent >= goldThreshold)
                return MedalType.Gold;
            else if (accuracyPercent >= silverThreshold)
                return MedalType.Silver;
            else
                return MedalType.Bronze;
        }

        /// <summary>
        /// Get points for medal type
        /// </summary>
        public int GetMedalPoints(MedalType medal)
        {
            switch (medal)
            {
                case MedalType.Gold:
                    return 3;
                case MedalType.Silver:
                    return 2;
                case MedalType.Bronze:
                    return 1;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Calculate time bonus points based on remaining time
        /// </summary>
        public int CalculateTimeBonus(float timeRemaining)
        {
            return Mathf.RoundToInt(timeRemaining * timeBonusMultiplier);
        }
    }

    /// <summary>
    /// Medal types for level completion
    /// </summary>
    public enum MedalType
    {
        None = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 3
    }
}
