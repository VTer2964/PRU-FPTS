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
    /// Khi level hoàn thành → lấy medal → quay về Campus.
    /// </summary>
    public class CountryGuessMinigame : MinigameBase
    {
        [SerializeField] private CountryGameManager countryGameManager;
        [SerializeField] private CountryLevelController levelController;

        protected override void Start()
        {
            minigameId = "CountryGuess";
            timeLimit = 99999f; // CountryGuessGame tự quản lý timer
            base.Start();

            if (countryGameManager == null)
                countryGameManager = FindFirstObjectByType<CountryGameManager>();
            if (levelController == null)
                levelController = FindFirstObjectByType<CountryLevelController>();

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

            // Chờ 3 giây để player xem result panel rồi quay về Campus
            StartCoroutine(FinishAfterDelay(medal, 3f));
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
            if (levelController != null)
                levelController.OnLevelCompleted -= HandleLevelCompleted;
        }
    }
}
