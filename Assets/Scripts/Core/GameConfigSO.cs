using UnityEngine;

namespace FPTSim.Core
{
    [CreateAssetMenu(menuName = "FPT Sim/Game Config", fileName = "GameConfig")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Loop")]
        public int totalDays = 7;

        [Header("Day Time Limit")]
        [Tooltip("Thời lượng 1 ngày (giây). 24 phút = 1440 giây.")]
        public float dayDurationSeconds = 24f * 60f;
    }
}