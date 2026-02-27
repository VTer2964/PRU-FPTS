namespace CaroGame
{
    public class BoardEvaluator
    {
        private readonly int boardSize;
        private readonly int winCondition;

        // Direction vectors: horizontal, vertical, diagonal \, diagonal /
        private static readonly int[,] Directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };

        public BoardEvaluator(int boardSize, int winCondition)
        {
            this.boardSize = boardSize;
            this.winCondition = winCondition;
        }

        /// <summary>
        /// Evaluate the board from the perspective of the given player.
        /// Positive score = favorable for player, negative = unfavorable.
        /// </summary>
        public int Evaluate(int[,] board, int player)
        {
            int opponent = player == 1 ? 2 : 1;
            int score = 0;

            score += EvaluateAllLines(board, player);
            score -= EvaluateAllLines(board, opponent);

            return score;
        }

        private int EvaluateAllLines(int[,] board, int player)
        {
            int totalScore = 0;

            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        int dr = Directions[d, 0];
                        int dc = Directions[d, 1];

                        // Only evaluate if a full window of winCondition length fits
                        int endR = row + (winCondition - 1) * dr;
                        int endC = col + (winCondition - 1) * dc;
                        if (!IsInBounds(endR, endC)) continue;

                        totalScore += EvaluateWindow(board, row, col, dr, dc, player);
                    }
                }
            }

            return totalScore;
        }

        /// <summary>
        /// Evaluate a window of 'winCondition' cells starting at (row, col) in direction (dr, dc).
        /// Scores based on how many pieces of 'player' are in the window and whether it's blocked.
        /// </summary>
        private int EvaluateWindow(int[,] board, int row, int col, int dr, int dc, int player)
        {
            int opponent = player == 1 ? 2 : 1;
            int playerCount = 0;
            int emptyCount = 0;

            for (int i = 0; i < winCondition; i++)
            {
                int r = row + i * dr;
                int c = col + i * dc;
                int cell = board[r, c];

                if (cell == player)
                    playerCount++;
                else if (cell == 0)
                    emptyCount++;
                else
                    return 0; // Window is blocked by opponent, no value
            }

            // Score based on player pieces in an unblocked window
            return ScoreFromCount(playerCount, emptyCount);
        }

        private int ScoreFromCount(int playerCount, int emptyCount)
        {
            if (playerCount == 5) return 100000;  // Win
            if (playerCount == 4 && emptyCount == 1) return 10000;  // One away from win
            if (playerCount == 3 && emptyCount == 2) return 1000;   // Strong threat
            if (playerCount == 2 && emptyCount == 3) return 100;    // Building threat
            if (playerCount == 1 && emptyCount == 4) return 10;     // Weak position
            return 0;
        }

        /// <summary>
        /// Quick check: does this player have winCondition in a row?
        /// </summary>
        public bool HasWon(int[,] board, int player)
        {
            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    if (board[row, col] != player) continue;

                    for (int d = 0; d < 4; d++)
                    {
                        int dr = Directions[d, 0];
                        int dc = Directions[d, 1];

                        int count = 0;
                        for (int i = 0; i < winCondition; i++)
                        {
                            int r = row + i * dr;
                            int c = col + i * dc;
                            if (!IsInBounds(r, c) || board[r, c] != player) break;
                            count++;
                        }

                        if (count >= winCondition) return true;
                    }
                }
            }
            return false;
        }

        private bool IsInBounds(int row, int col)
        {
            return row >= 0 && row < boardSize && col >= 0 && col < boardSize;
        }
    }
}
