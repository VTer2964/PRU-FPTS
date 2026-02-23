using System;

namespace FPTSim.Core
{
    [Serializable]
    public class GameState
    {
        // Timer tổng còn lại (giây)
        public float timeLeft;

        // Medals
        public int gold = 0;
        public int silver = 0;
        public int bronze = 0;

        // kết quả run
        public bool isGameOver = false;
        public bool isWin = false;

        public void Reset(float runDurationSeconds)
        {
            timeLeft = runDurationSeconds;

            gold = silver = bronze = 0;

            isGameOver = false;
            isWin = false;
        }
    }
}