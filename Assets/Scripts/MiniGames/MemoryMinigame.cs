using System.Collections;
using UnityEngine;
using FPTSim.Core;

namespace FPTSim.Minigames
{
    /// <summary>
    /// Adapter tích hợp MemoryCardMatch vào FPTSim.
    /// Gắn script này lên một GameObject trong scene Minigame_Memory.
    /// Game tự động bắt đầu ở difficulty Medium, bỏ qua menu.
    ///
    /// Medal dựa trên Score khi Victory:
    ///   Gold   : score >= 1000
    ///   Silver : score >= 600
    ///   Bronze : bất kỳ Victory
    ///   None   : GameOver (hết giờ)
    /// </summary>
    public class MemoryMinigame : MinigameBase
    {
        [Header("Medal Thresholds")]
        [SerializeField] private int goldScore   = 1000;
        [SerializeField] private int silverScore = 600;

        [SerializeField] private MemoryCardMatch.MemoryMatchGameManager memoryManager;

        protected override void Start()
        {
            minigameId = "Memory";
            timeLimit = 99999f; // MemoryCardMatch tự quản lý timer
            base.Start();

            if (memoryManager == null)
                memoryManager = FindFirstObjectByType<MemoryCardMatch.MemoryMatchGameManager>();

            if (memoryManager == null)
            {
                Debug.LogError("[MemoryMinigame] Không tìm thấy MemoryMatchGameManager!");
                return;
            }

            memoryManager.OnVictory.AddListener(HandleVictory);
            memoryManager.OnGameOver.AddListener(HandleGameOver);

            // Bỏ qua menu — bắt đầu ở Medium difficulty ngay
            memoryManager.SetDifficulty(1); // 0=Easy, 1=Medium, 2=Hard
            memoryManager.StartGame();
        }

        private void HandleVictory()
        {
            int score = memoryManager != null ? memoryManager.Score : 0;
            Medal medal;
            if (score >= goldScore)        medal = Medal.Gold;
            else if (score >= silverScore) medal = Medal.Silver;
            else                           medal = Medal.Bronze;

            StartCoroutine(FinishAfterDelay(medal, 3f));
        }

        private void HandleGameOver()
        {
            StartCoroutine(FinishAfterDelay(Medal.None, 3f));
        }

        private IEnumerator FinishAfterDelay(Medal medal, float delay)
        {
            yield return new WaitForSeconds(delay);
            Finish(new MinigameResult
            {
                minigameId   = minigameId,
                medal        = medal,
                scoreAwarded = 0,
                success      = medal != Medal.None
            });
        }

        private void OnDestroy()
        {
            if (memoryManager != null)
            {
                memoryManager.OnVictory?.RemoveListener(HandleVictory);
                memoryManager.OnGameOver?.RemoveListener(HandleGameOver);
            }
        }
    }
}
