using System.Collections.Generic;
using UnityEngine;

namespace CaroGame
{
    public class CaroHintSystem
    {
        private readonly int boardSize;

        // Direction vectors: horizontal, vertical, diagonal \, diagonal /
        private static readonly int[,] Directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };

        public CaroHintSystem(int boardSize)
        {
            this.boardSize = boardSize;
        }

        /// <summary>
        /// Find empty cells where the AI has building threats (2+ in a row that could extend).
        /// These are cells the player should consider blocking.
        /// </summary>
        public List<Vector2Int> GetDangerousCells(int[,] board, int aiPlayer)
        {
            var dangerCells = new HashSet<Vector2Int>();

            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    if (board[row, col] != 0) continue;

                    int threatLevel = EvaluateThreatAtCell(board, row, col, aiPlayer);

                    // Threshold: cell is dangerous if placing AI piece here creates 3+ in a row
                    if (threatLevel >= 3)
                    {
                        dangerCells.Add(new Vector2Int(row, col));
                    }
                }
            }

            // Also find cells adjacent to existing AI threats
            AddAdjacentThreats(board, aiPlayer, dangerCells);

            return new List<Vector2Int>(dangerCells);
        }

        /// <summary>
        /// Evaluate how dangerous it would be if AI placed a piece at (row, col).
        /// Returns the maximum consecutive count achievable.
        /// </summary>
        private int EvaluateThreatAtCell(int[,] board, int row, int col, int aiPlayer)
        {
            int maxConsecutive = 0;

            // Simulate placing AI piece
            board[row, col] = aiPlayer;

            for (int d = 0; d < 4; d++)
            {
                int dr = Directions[d, 0];
                int dc = Directions[d, 1];

                int count = CountBidirectional(board, row, col, dr, dc, aiPlayer);
                if (count > maxConsecutive)
                    maxConsecutive = count;
            }

            // Undo simulation
            board[row, col] = 0;

            return maxConsecutive;
        }

        /// <summary>
        /// Count consecutive pieces in both positive and negative direction.
        /// </summary>
        private int CountBidirectional(int[,] board, int row, int col, int dr, int dc, int player)
        {
            int count = 1; // Count the piece at (row, col)

            // Positive direction
            for (int i = 1; i < 5; i++)
            {
                int r = row + i * dr;
                int c = col + i * dc;
                if (!IsInBounds(r, c) || board[r, c] != player) break;
                count++;
            }

            // Negative direction
            for (int i = 1; i < 5; i++)
            {
                int r = row - i * dr;
                int c = col - i * dc;
                if (!IsInBounds(r, c) || board[r, c] != player) break;
                count++;
            }

            return count;
        }

        /// <summary>
        /// Add cells that are at the open ends of existing AI sequences of 2+.
        /// </summary>
        private void AddAdjacentThreats(int[,] board, int aiPlayer, HashSet<Vector2Int> dangerCells)
        {
            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    if (board[row, col] != aiPlayer) continue;

                    for (int d = 0; d < 4; d++)
                    {
                        int dr = Directions[d, 0];
                        int dc = Directions[d, 1];

                        // Count how many AI pieces are in a line from this cell
                        int count = 1;
                        for (int i = 1; i < 5; i++)
                        {
                            int r = row + i * dr;
                            int c = col + i * dc;
                            if (!IsInBounds(r, c) || board[r, c] != aiPlayer) break;
                            count++;
                        }

                        if (count >= 2)
                        {
                            // Check the open end after the sequence
                            int endR = row + count * dr;
                            int endC = col + count * dc;
                            if (IsInBounds(endR, endC) && board[endR, endC] == 0)
                                dangerCells.Add(new Vector2Int(endR, endC));

                            // Check the open end before the sequence
                            int startR = row - dr;
                            int startC = col - dc;
                            if (IsInBounds(startR, startC) && board[startR, startC] == 0)
                                dangerCells.Add(new Vector2Int(startR, startC));
                        }
                    }
                }
            }
        }

        private bool IsInBounds(int row, int col)
        {
            return row >= 0 && row < boardSize && col >= 0 && col < boardSize;
        }
    }
}
