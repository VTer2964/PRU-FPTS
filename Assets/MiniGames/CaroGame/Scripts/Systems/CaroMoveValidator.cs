using UnityEngine;

namespace CaroGame
{
    public class CaroMoveValidator
    {
        private readonly int boardSize;

        public CaroMoveValidator(int boardSize)
        {
            this.boardSize = boardSize;
        }

        public bool IsValidMove(int[,] board, Vector2Int position)
        {
            return IsInBounds(position.x, position.y) && board[position.x, position.y] == 0;
        }

        private bool IsInBounds(int row, int col)
        {
            return row >= 0 && row < boardSize && col >= 0 && col < boardSize;
        }
    }
}
