using System.Collections;
using UnityEngine;
using FPTSim.Core;
using CountryGuessGame.Managers;
using CountryGuessGame.Data;

namespace FPTSim.Minigames
{
    /// <summary>
    /// Adapter tích hợp CountryGuessGame vào FPTSim.
    /// Gắn script này lên một GameObject trong scene Minigame_CountryGuess.
    /// Game sẽ tự động bắt đầu Level 0, bỏ qua menu.
    /// Khi level hoàn thành → lấy điểm thực từ CountryScoreManager → quy ra medal → quay về Campus.
    /// </summary>
    public class CountryGuessMinigame : MinigameBase
    {
        [SerializeField] private CountryGameManager countryGameManager;
        [SerializeField] private CountryLevelController levelController;
        [SerializeField] private CountryScoreManager countryScoreManager;

        protected override void Start()
        {
            minigameId = "CountryGuess";
            timeLimit = 99999f; // CountryGuessGame tự quản lý timer
            base.Start();

            if (countryGameManager == null)
                countryGameManager = FindFirstObjectByType<CountryGameManager>();
            if (levelController == null)
                levelController = FindFirstObjectByType<CountryLevelController>();
            if (countryScoreManager == null)
                countryScoreManager = FindFirstObjectByType<CountryScoreManager>();

            if (levelController == null)
            {
                Debug.LogError("[CountryGuessMinigame] Không tìm thấy CountryLevelController!");
                return;
            }

            levelController.OnLevelCompleted += HandleLevelCompleted;

            // Chờ 1 frame để CountryLevelController.Start() chạy LoadLevel(0) trước,
            // rồi override lên Level 2
            StartCoroutine(LoadLevelNextFrame(2));
        }

        private IEnumerator LoadLevelNextFrame(int levelIndex)
        {
            yield return null; // chờ 1 frame
            levelController.LoadLevel(levelIndex);
        }

        private void HandleLevelCompleted(MedalType countryMedal, int medalPoints)
        {
            Medal medal = countryMedal switch
            {
                MedalType.Gold   => Medal.Gold,
                MedalType.Silver => Medal.Silver,
                MedalType.Bronze => Medal.Bronze,
                _                => Medal.None
            };

            // Lấy điểm thực từ ScoreManager; fallback sang medalPoints * 33 nếu không có ref
            int score = countryScoreManager != null
                ? countryScoreManager.CurrentScore
                : medalPoints * 33;

            // Ẩn panel kết quả của CountryGuessGame, dùng MinigameResultPanel thay thế
            var countryUI = FindFirstObjectByType<CountryUIManager>();
            if (countryUI != null) countryUI.HideResultPanel();

            StartCoroutine(FinishAfterDelay(medal, score, 0f));
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
            if (levelController != null)
                levelController.OnLevelCompleted -= HandleLevelCompleted;
        }
    }
}
