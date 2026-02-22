namespace FPTSim.Core
{
    [System.Serializable]
    public struct MinigameResult
    {
        public string minigameId;
        public Medal medal;
        public int scoreAwarded;
        public bool success;
    }
}