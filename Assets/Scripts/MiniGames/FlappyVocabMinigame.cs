using System.Collections;
using UnityEngine;
using FPTSim.Core;

namespace FPTSim.Minigames
{
    /// <summary>
    /// Adapter tích hợp FlappyVocabGame vào FPTSim.
    /// Gắn script này lên một GameObject tên [FlappyVocabAdapter] trong scene Minigame_FlappyVocab.
    ///
    /// Game tự quản lý timer 60s — MinigameBase timer bị vô hiệu hóa (99999f).
    ///
    /// Medal dựa trên Score khi game kết thúc:
    ///   Gold   : score >= 100 (≥10 câu đúng)
    ///   Silver : score >= 60  (≥6 câu đúng)
    ///   Bronze : score >= 30  (≥3 câu đúng)
    ///   None   : dưới 30 điểm
    /// </summary>
    public class FlappyVocabMinigame : MinigameBase
    {
        [Header("Medal Thresholds")]
        [SerializeField] private int goldScore   = 100;
        [SerializeField] private int silverScore = 60;
        [SerializeField] private int bronzeScore = 30;

        [SerializeField] private FlappyVocabGame.FlappyVocabGameManager gameManager;

        protected override void Start()
        {
            minigameId = "FlappyVocab";
            timeLimit  = 99999f; // FlappyVocabGame tự quản lý timer 60s
            base.Start();

            if (gameManager == null)
                gameManager = FindFirstObjectByType<FlappyVocabGame.FlappyVocabGameManager>();

            if (gameManager == null)
            {
                Debug.LogError("[FlappyVocabMinigame] Không tìm thấy FlappyVocabGameManager!");
                return;
            }

            gameManager.OnGameEnd.AddListener(HandleGameEnd);
            gameManager.StartGame();
        }

        private void HandleGameEnd(int score)
        {
            Medal medal;
            if      (score >= goldScore)   medal = Medal.Gold;
            else if (score >= silverScore) medal = Medal.Silver;
            else if (score >= bronzeScore) medal = Medal.Bronze;
            else                           medal = Medal.None;

            StartCoroutine(FinishAfterDelay(medal, score, 3f));
        }

        private IEnumerator FinishAfterDelay(Medal medal, int score, float delay)
        {
            yield return new WaitForSeconds(delay);
            Finish(new MinigameResult
            {
                minigameId   = minigameId,
                medal        = medal,
                scoreAwarded = score,
                success      = medal != Medal.None
            });
        }

        private void OnDestroy()
        {
            if (gameManager != null)
                gameManager.OnGameEnd.RemoveListener(HandleGameEnd);
        }
    }
}
