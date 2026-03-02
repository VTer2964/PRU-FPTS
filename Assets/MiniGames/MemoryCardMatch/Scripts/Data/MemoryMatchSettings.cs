using UnityEngine;

namespace MemoryCardMatch
{
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    [System.Serializable]
    public class DifficultySettings
    {
        [Tooltip("Number of rows in the card grid")]
        public int gridRows = 3;

        [Tooltip("Number of columns in the card grid")]
        public int gridColumns = 4;

        [Tooltip("Time limit in seconds (0 = no limit)")]
        public float timeLimit = 120f;
    }

    [CreateAssetMenu(fileName = "MemoryMatchSettings", menuName = "MemoryCardMatch/Settings")]
    public class MemoryMatchSettings : ScriptableObject
    {
        [Header("Difficulty Configurations")]
        [Tooltip("Settings for Easy difficulty (4x3 = 6 pairs)")]
        public DifficultySettings easySettings = new DifficultySettings
        {
            gridRows = 3,
            gridColumns = 4,
            timeLimit = 120f
        };

        [Tooltip("Settings for Medium difficulty (4x4 = 8 pairs)")]
        public DifficultySettings mediumSettings = new DifficultySettings
        {
            gridRows = 4,
            gridColumns = 4,
            timeLimit = 90f
        };

        [Tooltip("Settings for Hard difficulty (5x4 = 10 pairs)")]
        public DifficultySettings hardSettings = new DifficultySettings
        {
            gridRows = 4,
            gridColumns = 5,
            timeLimit = 60f
        };

        [Header("Animation Timings")]
        [Tooltip("Duration of the card flip animation in seconds")]
        [Range(0.1f, 1f)]
        public float cardFlipDuration = 0.3f;

        [Tooltip("How long mismatched cards stay visible before flipping back")]
        [Range(0.3f, 3f)]
        public float mismatchDisplayTime = 1f;

        [Header("Scoring")]
        [Tooltip("Base score awarded per matched pair")]
        public int baseMatchScore = 100;

        [Tooltip("Score multiplier for 2 consecutive matches")]
        public float comboMultiplier2 = 1.5f;

        [Tooltip("Score multiplier for 3+ consecutive matches")]
        public float comboMultiplier3Plus = 2.0f;

        [Tooltip("Bonus score per second of time remaining on Victory")]
        public int timeBonusPerSecond = 5;

        /// <summary>
        /// Returns the DifficultySettings for the given difficulty level.
        /// </summary>
        public DifficultySettings GetSettingsForDifficulty(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => easySettings,
                DifficultyLevel.Medium => mediumSettings,
                DifficultyLevel.Hard => hardSettings,
                _ => mediumSettings
            };
        }
    }
}
