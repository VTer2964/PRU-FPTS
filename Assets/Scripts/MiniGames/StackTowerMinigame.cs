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
            Medal medal = stars switch
            {
                3 => Medal.Gold,
                2 => Medal.Silver,
                _ => Medal.Bronze
            };

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
            if (stackManager != null)
            {
                stackManager.OnVictory  -= HandleVictory;
                stackManager.OnGameOver -= HandleGameOver;
            }
        }
    }
}
