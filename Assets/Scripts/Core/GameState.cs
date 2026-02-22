using System;

namespace FPTSim.Core
{
    [Serializable]
    public class GameState
    {
        public int currentDay = 1;

        // Thời gian còn lại trong ngày (giây)
        public float dayTimeLeft;

        // Medals
        public int gold = 0;
        public int silver = 0;
        public int bronze = 0;


        public void Reset(float dayDurationSeconds)
        {
            currentDay = 1;
            dayTimeLeft = dayDurationSeconds;

            gold = silver = bronze = 0;

        }

        public void StartNewDay(float dayDurationSeconds)
        {
            dayTimeLeft = dayDurationSeconds;
        }
    }
}