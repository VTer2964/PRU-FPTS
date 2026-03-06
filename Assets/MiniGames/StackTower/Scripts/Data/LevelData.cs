using UnityEngine;

namespace StackTower
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "StackTower/LevelData")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public int levelNumber = 1;
        public string levelName = "Level 1";

        [Header("Win Condition")]
        [Tooltip("Number of floors player must build to win")]
        public int targetFloors = 10;

        [Header("Block Settings")]
        [Tooltip("Initial block size (width/depth)")]
        public float initialBlockSize = 3f;
        [Tooltip("Minimum block size before game over")]
        public float minBlockSize = 0.3f;

        [Header("Speed Settings")]
        [Tooltip("Initial oscillation speed (units/second)")]
        public float initialMoveSpeed = 2f;
        [Tooltip("Maximum speed cap")]
        public float maxMoveSpeed = 10f;
        [Tooltip("Speed added each interval")]
        public float speedIncreaseAmount = 0.2f;
        [Tooltip("Every N floors, increase speed")]
        public int speedIncreaseInterval = 5;

        [Header("Perfect Settings")]
        [Tooltip("Offset distance that still counts as Perfect")]
        public float perfectThreshold = 0.15f;

        [Header("Visual")]
        [Tooltip("Block colors from bottom to top. Loops if not enough entries.")]
        public Color[] blockColors = new Color[]
        {
            new Color(0.4f, 0.8f, 1f),
            new Color(0.3f, 0.9f, 0.7f),
            new Color(0.5f, 0.7f, 1f),
            new Color(0.9f, 0.6f, 0.3f),
            new Color(1f, 0.4f, 0.5f)
        };

        [Header("Star Conditions")]
        [Range(0f, 1f)]
        [Tooltip("Min ratio of perfect placements for 3 stars")]
        public float threeStarPerfectRatio = 0.6f;
        [Range(0f, 1f)]
        [Tooltip("Min ratio of perfect placements for 2 stars")]
        public float twoStarPerfectRatio = 0.3f;

        /// <summary>Returns the block color for the given floor index.</summary>
        public Color GetBlockColor(int floorIndex)
        {
            if (blockColors == null || blockColors.Length == 0)
                return Color.white;
            return blockColors[floorIndex % blockColors.Length];
        }

        /// <summary>Calculate move speed at a given floor.</summary>
        public float GetSpeedAtFloor(int floor)
        {
            int increments = floor / Mathf.Max(1, speedIncreaseInterval);
            float speed = initialMoveSpeed + increments * speedIncreaseAmount;
            return Mathf.Min(speed, maxMoveSpeed);
        }
    }
}
