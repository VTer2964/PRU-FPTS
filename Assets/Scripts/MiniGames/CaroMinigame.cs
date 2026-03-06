using System.Collections;
using UnityEngine;
using FPTSim.Core;
using CaroGame;

namespace FPTSim.Minigames
{
    /// <summary>
    /// Adapter tích hợp CaroGame vào FPTSim.
    /// Gắn script này lên cùng GameObject với CaroGameManager trong scene Minigame_Caro.
    /// CaroGame tự quản lý timer của nó — MinigameBase timer bị vô hiệu hóa.
    /// </summary>
    public class CaroMinigame : MinigameBase
    {
        [SerializeField] private CaroGameManager caroGameManager;

        protected override void Start()
        {
            minigameId = "Caro";
            timeLimit = 99999f; // Vô hiệu hóa timer của MinigameBase
            base.Start();

            if (caroGameManager == null)
                caroGameManager = FindFirstObjectByType<CaroGameManager>();

            if (caroGameManager == null)
            {
                Debug.LogError("[CaroMinigame] Không tìm thấy CaroGameManager!");
                return;
            }

            caroGameManager.OnGameEnded += HandleGameEnded;

            // Bỏ qua menu — bắt đầu game ngay
            caroGameManager.StartGame();
        }

        private void HandleGameEnded(CaroMedalType caroMedal)
        {
            Medal medal = caroMedal switch
            {
                CaroMedalType.Gold   => Medal.Gold,
                CaroMedalType.Silver => Medal.Silver,
                CaroMedalType.Bronze => Medal.Bronze,
                _                    => Medal.None
            };

            // Chờ 3 giây để player xem kết quả rồi quay về Campus
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
            if (caroGameManager != null)
                caroGameManager.OnGameEnded -= HandleGameEnded;
        }
    }
}
