using System.Collections.Generic;
using UnityEngine;

namespace CaroGame
{
    public class MinimaxSolver
    {
        private readonly int boardSize;
        private readonly BoardEvaluator evaluator;
        private readonly int searchRadius;

        public MinimaxSolver(int boardSize, int winCondition)
        {
            this.boardSize = boardSize;
            this.evaluator = new BoardEvaluator(boardSize, winCondition);
            this.searchRadius = 2; // Only search cells within 2 squares of existing pieces
        }

        /// <summary>
        /// Get the best move for the AI player using minimax with alpha-beta pruning.
        /// </summary>
        public Vector2Int GetBestMove(int[,] board, int aiPlayer, int depth)
        {
            int bestScore = int.MinValue;
            Vector2Int bestMove = new Vector2Int(-1, -1);
            int opponent = aiPlayer == 1 ? 2 : 1;

            List<Vector2Int> validMoves = GetPrioritizedMoves(board);

            // If board is empty, play center
            if (validMoves.Count == 0 || IsBoardEmpty(board))
            {
                return new Vector2Int(boardSize / 2, boardSize / 2);
            }

            // Check for immediate winning move
            foreach (var move in validMoves)
            {
                board[move.x, move.y] = aiPlayer;
                if (evaluator.HasWon(board, aiPlayer))
                {
                    board[move.x, move.y] = 0;
                    return move;
                }
                board[move.x, move.y] = 0;
            }

            // Check for immediate blocking move
            foreach (var move in validMoves)
            {
                board[move.x, move.y] = opponent;
                if (evaluator.HasWon(board, opponent))
                {
                    board[move.x, move.y] = 0;
                    return move; // Must block
                }
                board[move.x, move.y] = 0;
            }

            // Minimax search
            foreach (var move in validMoves)
            {
                board[move.x, move.y] = aiPlayer;
                int score = Minimax(board, depth - 1, false, int.MinValue, int.MaxValue, aiPlayer);
                board[move.x, move.y] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            // Fallback: if no good move found, pick first valid move
            if (bestMove.x == -1)
            {
                return validMoves.Count > 0 ? validMoves[0] : new Vector2Int(boardSize / 2, boardSize / 2);
            }

            return bestMove;
        }

        private int Minimax(int[,] board, int depth, bool isMaximizing, int alpha, int beta, int aiPlayer)
        {
            int opponent = aiPlayer == 1 ? 2 : 1;

            // Terminal conditions
            if (evaluator.HasWon(board, aiPlayer)) return 100000 + depth; // Prefer faster wins
            if (evaluator.HasWon(board, opponent)) return -100000 - depth; // Prefer slower losses

            if (depth == 0)
                return evaluator.Evaluate(board, aiPlayer);

            List<Vector2Int> moves = GetPrioritizedMoves(board);
            if (moves.Count == 0) return 0; // Draw

            if (isMaximizing)
            {
                int maxScore = int.MinValue;
                foreach (var move in moves)
                {
                    board[move.x, move.y] = aiPlayer;
                    int score = Minimax(board, depth - 1, false, alpha, beta, aiPlayer);
                    board[move.x, move.y] = 0;

                    maxScore = Mathf.Max(maxScore, score);
                    alpha = Mathf.Max(alpha, score);
                    if (beta <= alpha) break;
                }
                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;
                foreach (var move in moves)
                {
                    board[move.x, move.y] = opponent;
                    int score = Minimax(board, depth - 1, true, alpha, beta, aiPlayer);
                    board[move.x, move.y] = 0;

                    minScore = Mathf.Min(minScore, score);
                    beta = Mathf.Min(beta, score);
                    if (beta <= alpha) break;
                }
                return minScore;
            }
        }

        /// <summary>
        /// Get valid moves prioritized by proximity to existing pieces.
        /// Only considers cells within searchRadius of existing pieces for performance.
        /// </summary>
        private List<Vector2Int> GetPrioritizedMoves(int[,] board)
        {
            var moves = new HashSet<Vector2Int>();

            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    if (board[row, col] == 0) continue;

                    // Add empty cells around this occupied cell
                    for (int dr = -searchRadius; dr <= searchRadius; dr++)
                    {
                        for (int dc = -searchRadius; dc <= searchRadius; dc++)
                        {
                            int r = row + dr;
                            int c = col + dc;
                            if (r >= 0 && r < boardSize && c >= 0 && c < boardSize && board[r, c] == 0)
                            {
                                moves.Add(new Vector2Int(r, c));
                            }
                        }
                    }
                }
            }

            // Sort by proximity to center (prefer central moves)
            var sortedMoves = new List<Vector2Int>(moves);
            int center = boardSize / 2;
            sortedMoves.Sort((a, b) =>
            {
                float distA = Mathf.Abs(a.x - center) + Mathf.Abs(a.y - center);
                float distB = Mathf.Abs(b.x - center) + Mathf.Abs(b.y - center);
                return distA.CompareTo(distB);
            });

            return sortedMoves;
        }

        private bool IsBoardEmpty(int[,] board)
        {
            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    if (board[row, col] != 0) return false;
                }
            }
            return true;
        }
    }
}
