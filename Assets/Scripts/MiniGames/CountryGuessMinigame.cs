using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using FPTSim.Core;
using CountryGuessGame.Core;
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

        // ── Keyboard navigation ────────────────────────────────────────────────
        private CountryMatchingController _matchingController;
        private AnswerButton[] _answerButtons = new AnswerButton[0];
        private int _selectedAnswerIndex;

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

            // Đăng ký keyboard nav trước khi load level (để bắt OnQuestionStarted đầu tiên)
            _matchingController = FindFirstObjectByType<CountryMatchingController>();
            if (_matchingController != null)
                _matchingController.OnQuestionStarted += OnCountryQuestionStarted;

            // Chờ 1 frame để CountryLevelController.Start() chạy LoadLevel(0) trước,
            // rồi override lên Level 2
            StartCoroutine(LoadLevelNextFrame(2));
        }

        protected override void Update()
        {
            base.Update();
            if (_answerButtons == null || _answerButtons.Length == 0 || finished) return;
            HandleCountryKeyboard();
        }

        private void HandleCountryKeyboard()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            int move = 0;
            if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)  move = -1;
            if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) move =  1;
            if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)    move = -1;
            if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)  move =  1;

            if (move != 0)
            {
                SetAnswerHighlight(_selectedAnswerIndex, false);
                _selectedAnswerIndex = (_selectedAnswerIndex + move + _answerButtons.Length) % _answerButtons.Length;
                SetAnswerHighlight(_selectedAnswerIndex, true);
            }

            if (kb.spaceKey.wasPressedThisFrame)
            {
                if (_selectedAnswerIndex >= 0 && _selectedAnswerIndex < _answerButtons.Length)
                    _answerButtons[_selectedAnswerIndex].SimulateClick();
            }
        }

        private void OnCountryQuestionStarted()
        {
            // Buttons được setup trong cùng frame → chờ 1 frame rồi refresh
            StartCoroutine(RefreshCountryButtonsNextFrame());
        }

        private IEnumerator RefreshCountryButtonsNextFrame()
        {
            yield return null;
            _answerButtons = FindObjectsByType<AnswerButton>(FindObjectsSortMode.None);
            System.Array.Sort(_answerButtons,
                (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            _selectedAnswerIndex = 0;
            SetAnswerHighlight(_selectedAnswerIndex, true);
        }

        private void SetAnswerHighlight(int index, bool on)
        {
            if (_answerButtons == null || index < 0 || index >= _answerButtons.Length) return;
            if (_answerButtons[index] != null)
                _answerButtons[index].SetCursorHighlight(on);
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
            if (_matchingController != null)
                _matchingController.OnQuestionStarted -= OnCountryQuestionStarted;
        }
    }
}
