using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using FPTSim.Core;
using CaroGame;

namespace FPTSim.Minigames
{
    /// <summary>
    /// Adapter tích hợp CaroGame vào FPTSim.
    /// Gắn script này lên cùng GameObject với CaroGameManager trong scene Minigame_Caro.
    /// CaroGame tự quản lý timer của nó — MinigameBase timer bị vô hiệu hóa.
    /// Điều khiển bàn phím: WASD/Mũi tên = di chuyển con trỏ, Space = đặt quân.
    /// </summary>
    public class CaroMinigame : MinigameBase
    {
        [SerializeField] private CaroGameManager caroGameManager;

        // ── Keyboard cursor ───────────────────────────────────────────────────
        private int _cursorRow;
        private int _cursorCol;
        private bool _boardReady;

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

            // Chờ 1 frame để CaroGameManager.Start() chạy ShowMainMenu() trước,
            // rồi mới override bằng StartGame()
            StartCoroutine(StartCaroNextFrame());
        }

        private IEnumerator StartCaroNextFrame()
        {
            yield return null;
            caroGameManager.SetAiDepth(3);
            caroGameManager.StartGame();

            // Khởi tạo con trỏ bàn phím ở trung tâm bàn cờ
            int mid = caroGameManager.BoardController.BoardSize / 2;
            _cursorRow = mid;
            _cursorCol = mid;
            _boardReady = true;
            caroGameManager.BoardController.SetCursorHighlight(_cursorRow, _cursorCol, true);
        }

        protected override void Update()
        {
            base.Update();
            if (!_boardReady || caroGameManager == null || finished) return;
            HandleCaroKeyboard();
        }

        private void HandleCaroKeyboard()
        {
            if (!caroGameManager.IsPlayerTurn) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            int dr = 0, dc = 0;
            if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)    dr = -1;
            if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)  dr =  1;
            if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)  dc = -1;
            if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) dc =  1;

            if (dr != 0 || dc != 0)
            {
                var board = caroGameManager.BoardController;
                board.SetCursorHighlight(_cursorRow, _cursorCol, false);
                _cursorRow = Mathf.Clamp(_cursorRow + dr, 0, board.BoardSize - 1);
                _cursorCol = Mathf.Clamp(_cursorCol + dc, 0, board.BoardSize - 1);
                board.SetCursorHighlight(_cursorRow, _cursorCol, true);
            }

            if (kb.spaceKey.wasPressedThisFrame)
            {
                var board = caroGameManager.BoardController;
                if (board.IsCellEmpty(_cursorRow, _cursorCol))
                {
                    board.SetCursorHighlight(_cursorRow, _cursorCol, false);
                    board.SimulateCellClick(_cursorRow, _cursorCol);
                    // Giữ cursor ở ô vừa đặt; người chơi tự di chuyển sang ô khác
                }
            }
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

            // Ẩn panel kết quả của CaroGame, dùng MinigameResultPanel thay thế
            var resultsPanel = FindFirstObjectByType<CaroResultsPanel>();
            if (resultsPanel != null) resultsPanel.Hide();

            StartCoroutine(FinishAfterDelay(medal, 0f));
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
