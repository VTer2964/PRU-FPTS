using UnityEngine;

namespace CaroGame
{
    public class CaroAI : MonoBehaviour
    {
        [SerializeField] private CaroGameConfig config;

        private MinimaxSolver solver;
        private int depthOverride = -1; // -1 = dùng config

        private void Awake()
        {
            InitializeSolver();
        }

        public void InitializeSolver()
        {
            solver = new MinimaxSolver(config.boardSize, config.winCondition);
        }

        public void SetDepth(int depth) => depthOverride = depth;

        /// <summary>
        /// Get the best move for AI. Runs synchronously (fast enough for depth 2 on 10x10).
        /// </summary>
        public Vector2Int GetBestMove(int[,] boardState)
        {
            int depth = depthOverride > 0 ? depthOverride : config.aiDepth;
            return solver.GetBestMove(boardState, (int)CellState.AI, depth);
        }
    }
}
