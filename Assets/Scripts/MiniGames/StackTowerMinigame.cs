using System.Collections;
using UnityEngine;
using FPTSim.Core;
using StackTower;

namespace FPTSim.Minigames
{
    /// <summary>
    /// Adapter tích hợp StackTower vào FPTSim.
    /// Gắn script này lên một GameObject trong scene Minigame_StackTower.
    /// Game tự động bắt đầu Level 0 (Level 1 trong game), bỏ qua menu.
    ///
    /// Medal dựa trên số sao khi Victory:
    ///   3 sao → Gold
    ///   2 sao → Silver
    ///   1 sao → Bronze
    ///   GameOver (miss block) → None
    /// </summary>
    public class StackTowerMinigame : MinigameBase
    {
        [Header("Medal Thresholds (score = stars × 33)")]
        [SerializeField] private int goldScore   = 80; // ≥ 3 sao (99)
        [SerializeField] private int silverScore = 50; // ≥ 2 sao (66)
        [SerializeField] private int bronzeScore = 20; // ≥ 1 sao (33)

        [SerializeField] private StackTowerGameManager stackManager;

        protected override void Start()
        {
            minigameId = "StackTower";
            timeLimit = 99999f; // StackTower không có time limit
            base.Start();

            if (stackManager == null)
                stackManager = FindFirstObjectByType<StackTowerGameManager>();

            if (stackManager == null)
            {
                Debug.LogError("[StackTowerMinigame] Không tìm thấy StackTowerGameManager!");
                return;
            }

            stackManager.OnVictory  += HandleVictory;
            stackManager.OnGameOver += HandleGameOver;

            // Bỏ qua menu — bắt đầu Level 0 ngay
            stackManager.StartGame(0);
        }

        private void HandleVictory(int stars)
        {
            int score = stars * 33; // 1 sao=33, 2 sao=66, 3 sao=99
            Medal medal = CalculateMedal(score);
            StartCoroutine(FinishAfterDelay(medal, score, 3f));
        }

        private void HandleGameOver()
        {
            StartCoroutine(FinishAfterDelay(Medal.None, 0, 3f));
        }

        private Medal CalculateMedal(int score)
        {
            if (score >= goldScore)   return Medal.Gold;
            if (score >= silverScore) return Medal.Silver;
            if (score >= bronzeScore) return Medal.Bronze;
            return Medal.None;
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
            if (stackManager != null)
            {
                stackManager.OnVictory  -= HandleVictory;
                stackManager.OnGameOver -= HandleGameOver;
            }
        }
    }
}
