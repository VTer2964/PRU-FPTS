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
    /// Medal dựa trên số tầng xếp được (cả Victory lẫn GameOver):
    ///   >= goldFloors (10)   → Gold
    ///   >= silverFloors (7)  → Silver
    ///   >= bronzeFloors (4)  → Bronze
    ///   < bronzeFloors       → None
    /// </summary>
    public class StackTowerMinigame : MinigameBase
    {
        [Header("Medal Thresholds (floors stacked)")]
        [SerializeField] private int goldFloors   = 10; // đạt đủ target
        [SerializeField] private int silverFloors =  7;
        [SerializeField] private int bronzeFloors =  4;

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

        private void HandleVictory(int _)
        {
            // Dùng số tầng thực tế thay vì sao
            int floors = stackManager.CurrentFloor;
            Medal medal = CalculateMedal(floors);

            var stackUI = FindFirstObjectByType<StackTowerUIManager>();
            if (stackUI != null) stackUI.HideAllPanels();

            StartCoroutine(FinishAfterDelay(medal, floors, 0f));
        }

        private void HandleGameOver()
        {
            int floors = stackManager.CurrentFloor;
            Medal medal = CalculateMedal(floors);

            var stackUI = FindFirstObjectByType<StackTowerUIManager>();
            if (stackUI != null) stackUI.HideAllPanels();

            StartCoroutine(FinishAfterDelay(medal, floors, 0f));
        }

        private Medal CalculateMedal(int floors)
        {
            if (floors >= goldFloors)   return Medal.Gold;
            if (floors >= silverFloors) return Medal.Silver;
            if (floors >= bronzeFloors) return Medal.Bronze;
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
