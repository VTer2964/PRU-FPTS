using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

        // ── Keyboard navigation ────────────────────────────────────────────────
        private List<MemoryCardMatch.CardController> _cards = new();
        private int _selectedCardIndex;
        private static readonly Vector3 _selectedScale = new Vector3(1.12f, 1.12f, 1f);

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

            // Bỏ qua menu — bắt đầu ở Hard difficulty
            memoryManager.SetDifficulty(2); // 0=Easy, 1=Medium, 2=Hard
            memoryManager.StartGame();

            // Khởi tạo keyboard nav (cards đã được spawn đồng bộ trong StartGame)
            InitCardNav();
        }

        private void InitCardNav()
        {
            var found = FindObjectsByType<MemoryCardMatch.CardController>(FindObjectsSortMode.None);
            _cards = new List<MemoryCardMatch.CardController>(found);
            // Sắp xếp: trên → dưới, trái → phải (thứ tự đọc tự nhiên)
            _cards.Sort((a, b) =>
            {
                float dy = b.transform.position.y - a.transform.position.y;
                if (Mathf.Abs(dy) > 0.5f) return dy > 0 ? 1 : -1;
                return a.transform.position.x.CompareTo(b.transform.position.x);
            });
            _selectedCardIndex = 0;
            SetCardScale(_selectedCardIndex, true);
        }

        protected override void Update()
        {
            base.Update();
            if (_cards == null || _cards.Count == 0 || finished) return;
            if (memoryManager == null || memoryManager.CurrentState != MemoryCardMatch.GameState.Playing) return;
            HandleMemoryKeyboard();
        }

        private void HandleMemoryKeyboard()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            int move = 0;
            if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)  move = -1;
            if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) move =  1;
            if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)    move = -1;
            if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)  move =  1;

            if (move != 0) MoveCardSelection(move);

            if (kb.spaceKey.wasPressedThisFrame)
                ConfirmCardSelection();
        }

        private void MoveCardSelection(int delta)
        {
            if (_cards.Count == 0) return;
            SetCardScale(_selectedCardIndex, false);

            int next = _selectedCardIndex;
            int attempts = 0;
            do
            {
                next = (next + delta + _cards.Count) % _cards.Count;
                attempts++;
            }
            while (attempts < _cards.Count && !IsCardSelectable(next));

            _selectedCardIndex = next;
            SetCardScale(_selectedCardIndex, true);
        }

        private void ConfirmCardSelection()
        {
            if (_selectedCardIndex < 0 || _selectedCardIndex >= _cards.Count) return;
            var card = _cards[_selectedCardIndex];
            if (!IsCardSelectable(_selectedCardIndex)) return;
            memoryManager.OnCardClicked(card);
        }

        private bool IsCardSelectable(int index)
        {
            if (index < 0 || index >= _cards.Count) return false;
            var card = _cards[index];
            return card != null
                && card.gameObject.activeInHierarchy
                && card.State == MemoryCardMatch.CardState.FaceDown;
        }

        private void SetCardScale(int index, bool selected)
        {
            if (index < 0 || index >= _cards.Count) return;
            var card = _cards[index];
            if (card != null && card.gameObject.activeInHierarchy)
                card.transform.localScale = selected ? _selectedScale : Vector3.one;
        }

        private void HandleVictory()
        {
            int score = memoryManager != null ? memoryManager.Score : 0;
            Medal medal;
            if (score >= goldScore)        medal = Medal.Gold;
            else if (score >= silverScore) medal = Medal.Silver;
            else                           medal = Medal.Bronze;

            // Ẩn panel kết quả của MemoryCardMatch, dùng MinigameResultPanel thay thế
            var memoryUI = FindFirstObjectByType<MemoryCardMatch.MemoryMatchUIManager>();
            if (memoryUI != null) memoryUI.HideAllPanels();

            StartCoroutine(FinishAfterDelay(medal, score, 0f));
        }

        private void HandleGameOver()
        {
            // Ẩn panel kết quả của MemoryCardMatch, dùng MinigameResultPanel thay thế
            var memoryUI = FindFirstObjectByType<MemoryCardMatch.MemoryMatchUIManager>();
            if (memoryUI != null) memoryUI.HideAllPanels();

            StartCoroutine(FinishAfterDelay(Medal.None, 0, 0f));
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
            if (memoryManager != null)
            {
                memoryManager.OnVictory?.RemoveListener(HandleVictory);
                memoryManager.OnGameOver?.RemoveListener(HandleGameOver);
            }
        }
    }
}
