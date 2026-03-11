using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace FlappyVocabGame
{
    /// <summary>
    /// Central game manager cho FlappyVocab.
    /// Tham khảo: VocabFlappyBird/Scripts/Managers/VocabGameManager.cs
    /// </summary>
    public class FlappyVocabGameManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float gameDuration = 60f;
        [SerializeField] private int   startLives   = 3;
        [SerializeField] private int   baseScore    = 10;

        [Header("References")]
        [SerializeField] private BirdController bird;
        [SerializeField] private WordSpawner    wordSpawner;

        [Header("UI (auto-find theo tên GameObject)")]
        [SerializeField] private TMP_Text   scoreText;
        [SerializeField] private TMP_Text   livesText;
        [SerializeField] private TMP_Text   timerText;
        [SerializeField] private TMP_Text   streakText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text   finalScoreText;

        public UnityEvent<int> OnGameEnd = new UnityEvent<int>();

        public int  Score       { get; private set; }
        public int  Lives       { get; private set; }
        public int  Streak      { get; private set; }
        public bool GameRunning { get; private set; }
        public GameObject GameOverPanel => gameOverPanel;

        private float _timeLeft;
        private bool  _processingAnswer;

        // ──────────────────────────────────────────────────────────
        private void Start()
        {
            if (bird        == null) bird        = FindFirstObjectByType<BirdController>();
            if (wordSpawner == null) wordSpawner = FindFirstObjectByType<WordSpawner>();

            TryFindUI();
            StartCoroutine(AutoStart());
        }

        private void TryFindUI()
        {
            if (scoreText      == null) scoreText      = FindText("ScoreText");
            if (livesText      == null) livesText      = FindText("LivesText");
            if (timerText      == null) timerText      = FindText("TimerText");
            if (streakText     == null) streakText     = FindText("StreakText");
            if (finalScoreText == null) finalScoreText = FindText("FinalScoreText");
            if (gameOverPanel  == null) { var g = GameObject.Find("GameOverPanel"); if (g) gameOverPanel = g; }
        }

        private static TMP_Text FindText(string name)
        {
            var go = GameObject.Find(name);
            return go ? go.GetComponent<TMP_Text>() : null;
        }

        private IEnumerator AutoStart()
        {
            yield return null; // 1 frame: cho adapter gọi StartGame() trước nếu có
            if (!GameRunning) StartGame();
        }

        // ──────────────────────────────────────────────────────────
        public void StartGame()
        {
            Score             = 0;
            Lives             = startLives;
            Streak            = 0;
            _timeLeft         = gameDuration;
            GameRunning       = true;
            _processingAnswer = false;

            if (gameOverPanel) gameOverPanel.SetActive(false);
            bird?.SetActive(true);
            wordSpawner?.StartSpawning();
            RefreshUI();
        }

        private void Update()
        {
            if (!GameRunning) return;
            _timeLeft -= Time.deltaTime;
            if (timerText) timerText.text = Mathf.CeilToInt(Mathf.Max(0, _timeLeft)).ToString();
            if (_timeLeft <= 0f) EndGame();
        }

        // ──────────────────────────────────────────────────────────
        /// <summary>Gọi bởi BirdController khi chạm gate đúng.</summary>
        public void OnCorrectAnswer()
        {
            if (!GameRunning || _processingAnswer) return;
            _processingAnswer = true;

            Streak++;
            Score += baseScore + Streak * 2; // streak bonus
            wordSpawner?.SpawnNextQuestion();
            RefreshUI();

            StartCoroutine(ResetProcessingFlag());
        }

        /// <summary>Gọi bởi BirdController khi chạm gate sai.</summary>
        public void OnWrongAnswer()
        {
            if (!GameRunning || _processingAnswer) return;
            _processingAnswer = true;

            Streak = 0;
            Lives  = Mathf.Max(0, Lives - 1);
            RefreshUI();

            if (Lives == 0) { EndGame(); return; }

            wordSpawner?.SpawnNextQuestion();
            StartCoroutine(ResetProcessingFlag());
        }

        private IEnumerator ResetProcessingFlag()
        {
            yield return null;
            _processingAnswer = false;
        }

        // ──────────────────────────────────────────────────────────
        private void EndGame()
        {
            if (!GameRunning) return;
            GameRunning = false;

            bird?.SetActive(false);
            wordSpawner?.StopSpawning();

            if (gameOverPanel)  gameOverPanel.SetActive(true);
            if (finalScoreText) finalScoreText.text = $"Score: {Score}";

            OnGameEnd?.Invoke(Score);
        }

        private void RefreshUI()
        {
            if (scoreText)  scoreText.text  = $"Score: {Score}";
            if (livesText)  livesText.text  = $"Lives: {Lives}";
            if (streakText) streakText.text = Streak > 1 ? $"Streak x{Streak}!" : "";
        }
    }
}
