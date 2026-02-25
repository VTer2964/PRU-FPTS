using System;
using System.Collections.Generic;
namespace FPTSim.Core
{
    [Serializable]
    public class GameState
    {
        //stroy flags (cờ cốt truyện) - có thể dùng để theo dõi sự kiện đã xảy ra, lựa chọn của người chơi, v.v.
        public List<string> storyFlags = new List<string>();

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

        public bool HasFlag(string flag) => storyFlags != null && storyFlags.Contains(flag);

        public void SetFlag(string flag)
        {
            if (string.IsNullOrWhiteSpace(flag)) return;
            if (storyFlags == null) storyFlags = new List<string>();
            if (!storyFlags.Contains(flag)) storyFlags.Add(flag);
        }
    }
}