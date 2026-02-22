using System;
using System.Collections.Generic;

namespace FPTSim.Core
{
    [Serializable]
    public class GameState
    {
        public int currentDay = 1;
        public int playedMinigamesToday = 0;

        public int totalScore = 0;
        public int gold = 0;
        public int silver = 0;
        public int bronze = 0;

        // Flags for endings
        public bool usedToolCheat = false; // Bad Ending 1
        public int stress = 0;             // Bad Ending 2
        public bool disappeared = false;   // Bad Ending 3

        // optional: track what you played
        public List<string> history = new List<string>();

        public void Reset()
        {
            currentDay = 1;
            playedMinigamesToday = 0;
            totalScore = 0;
            gold = silver = bronze = 0;
            usedToolCheat = false;
            stress = 0;
            disappeared = false;
            history.Clear();
        }
    }
}