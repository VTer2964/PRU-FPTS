using System.Collections.Generic;
using UnityEngine;

namespace CaroGame
{
    public class CaroWinChecker
    {
        private readonly int boardSize;
        private readonly int winCondition;

        // Direction vectors: horizontal, vertical, diagonal \, diagonal /
        private static readonly int[,] Directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };

        public CaroWinChecker(int boardSize, int winCondition)
        {
            this.boardSize = boardSize;
            this.winCondition = winCondition;
        }

        /// <summary>
        /// Check if there is a winner on the board.
        /// Returns: 0 = no winner, 1 = player wins, 2 = AI wins, -1 = draw
        /// </summary>
        public int CheckWinner(int[,] board)
        {
            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    int player = board[row, col];
                    if (player == 0) continue;

                    for (int d = 0; d < 4; d++)
                    {
                        int dr = Directions[d, 0];
                        int dc = Directions[d, 1];

                        if (CountInDirection(board, row, col, dr, dc, player) >= winCondition)
                            return player;
                    }
                }
            }

            if (IsBoardFull(board))
                return -1;

            return 0;
        }

        /// <summary>
        /// Count consecutive pieces starting from (row, col) in one direction only.
        /// </summary>
        private int CountInDirection(int[,] board, int row, int col, int dr, int dc, int player)
        {
            int count = 0;
            for (int i = 0; i < winCondition; i++)
            {
                int r = row + i * dr;
                int c = col + i * dc;
                if (!IsInBounds(r, c) || board[r, c] != player)
                    break;
                count++;
            }
            return count;
        }

        /// <summary>
        /// Get the winning line positions (for visual highlight).
        /// </summary>
        public List<Vector2Int> GetWinningLine(int[,] board)
        {
            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    int player = board[row, col];
                    if (player == 0) continue;

                    for (int d = 0; d < 4; d++)
                    {
                        int dr = Directions[d, 0];
                        int dc = Directions[d, 1];

                        int count = CountInDirection(board, row, col, dr, dc, player);
                        if (count >= winCondition)
                        {
                            var line = new List<Vector2Int>();
                            for (int i = 0; i < count; i++)
                            {
                                line.Add(new Vector2Int(row + i * dr, col + i * dc));
                            }
                            return line;
                        }
                    }
                }
            }
            return null;
        }

        public bool IsBoardFull(int[,] board)
        {
            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    if (board[row, col] == 0)
                        return false;
                }
            }
            return true;
        }

        private bool IsInBounds(int row, int col)
        {
            return row >= 0 && row < boardSize && col >= 0 && col < boardSize;
        }
    }
}
