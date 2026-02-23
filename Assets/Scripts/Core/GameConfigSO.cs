using UnityEngine;

namespace FPTSim.Core
{
    [CreateAssetMenu(menuName = "FPT Sim/Game Config", fileName = "GameConfig")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Run Time Limit")]
        [Tooltip("Tổng thời gian cho cả run. 18 phút = 1080 giây.")]
        public float runDurationSeconds = 18f * 60f;

        [Header("Win Condition (Required Medals)")]
        public int requiredGold = 1;
        public int requiredSilver = 2;
        public int requiredBronze = 3;

        [Header("Buy Time Shop")]
        [Tooltip("Số giây cộng thêm khi mua bằng Bronze/Silver/Gold.")]
        public int addSecondsBronze = 60;   // +1 phút
        public int addSecondsSilver = 120;  // +2 phút
        public int addSecondsGold = 180;    // +3 phút

        [Header("Max Time Cap (optional)")]
        [Tooltip("Giới hạn tối đa timeLeft để tránh spam mua quá nhiều. 0 = không giới hạn.")]
        public int maxTimeCapSeconds = 0;
    }
}