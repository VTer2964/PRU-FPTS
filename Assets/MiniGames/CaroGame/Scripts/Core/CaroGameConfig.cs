using UnityEngine;

namespace CaroGame
{
    [CreateAssetMenu(fileName = "CaroGameConfig", menuName = "CaroGame/Game Config")]
    public class CaroGameConfig : ScriptableObject
    {
        [Header("Board Settings")]
        [Tooltip("Size of the board (NxN)")]
        public int boardSize = 10;

        [Tooltip("Number of consecutive pieces needed to win")]
        public int winCondition = 5;

        [Header("Timer Settings")]
        [Tooltip("Time limit per turn in seconds")]
        public float turnTimeLimit = 60f;

        [Header("AI Settings")]
        [Tooltip("Visual delay before AI places its piece")]
        public float aiThinkDelay = 0.5f;

        [Tooltip("Minimax search depth (1=Easy, 2=Medium, 3=Hard)")]
        [Range(1, 3)]
        public int aiDepth = 2;

        [Header("Medal Thresholds")]
        [Tooltip("Win in this many moves or fewer for Gold")]
        public int goldMedalMoves = 10;

        [Tooltip("Win in this many moves or fewer for Silver")]
        public int silverMedalMoves = 20;

        [Header("Hint Settings")]
        [Tooltip("Enable hint system that highlights dangerous cells")]
        public bool showHints = true;

        [Header("Visual Settings")]
        [Tooltip("Size of each cell in pixels")]
        public float cellSize = 55f;

        [Tooltip("Spacing between cells")]
        public float cellSpacing = 2f;
    }
}
