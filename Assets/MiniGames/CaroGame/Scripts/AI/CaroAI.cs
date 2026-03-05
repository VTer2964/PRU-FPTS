using UnityEngine;

namespace CaroGame
{
    public class CaroAI : MonoBehaviour
    {
        [SerializeField] private CaroGameConfig config;

        private MinimaxSolver solver;

        private void Awake()
        {
            InitializeSolver();
        }

        public void InitializeSolver()
        {
            solver = new MinimaxSolver(config.boardSize, config.winCondition);
        }

        /// <summary>
        /// Get the best move for AI. Runs synchronously (fast enough for depth 2 on 10x10).
        /// </summary>
        public Vector2Int GetBestMove(int[,] boardState)
        {
            return solver.GetBestMove(boardState, (int)CellState.AI, config.aiDepth);
        }
    }
}
