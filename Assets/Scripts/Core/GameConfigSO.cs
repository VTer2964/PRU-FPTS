using UnityEngine;

namespace FPTSim.Core
{
    [CreateAssetMenu(menuName = "FPT Sim/Game Config", fileName = "GameConfig")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Loop")]
        public int totalDays = 7;
        public int maxMinigamesPerDay = 5;

        [Header("Score by Medal")]
        public int goldPoints = 3;
        public int silverPoints = 2;
        public int bronzePoints = 1;

        [Header("Endings")]
        public int stressThreshold = 100;

        [Header("Random Events")]
        [Range(0f, 1f)]
        public float randomEventChancePerMinigame = 0.25f;
    }
}