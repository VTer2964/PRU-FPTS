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

            // Bỏ qua menu — bắt đầu Level 0 ngay
            if (countryGameManager != null)
                countryGameManager.StartLevel(0);
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

            // Chờ 3 giây để player xem result panel rồi quay về Campus
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
            if (levelController != null)
                levelController.OnLevelCompleted -= HandleLevelCompleted;
        }
    }
}
