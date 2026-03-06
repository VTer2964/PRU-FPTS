using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace FlappyVocabGame
{
    public class FlappyVocabGameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private float gameDuration = 60f;
        [SerializeField] private int startLives = 3;

        [Header("References")]
        [SerializeField] private BirdController bird;
        [SerializeField] private WordSpawner wordSpawner;

        [Header("UI")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text finalScoreText;

        public UnityEvent<int> OnGameEnd;

        public int Score { get; private set; }
        public int Lives { get; private set; }

        private float timeLeft;
        private bool gameRunning;

        private void Awake()
        {
            if (OnGameEnd == null)
                OnGameEnd = new UnityEvent<int>();
        }

        public void StartGame()
        {
            Score = 0;
            Lives = startLives;
            timeLeft = gameDuration;
            gameRunning = true;

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            bird?.EnableInput(true);
            wordSpawner?.StartSpawning();

            UpdateUI();
        }

        private void Update()
        {
            if (!gameRunning) return;

            timeLeft -= Time.deltaTime;
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(Mathf.Max(0, timeLeft)).ToString();

            if (timeLeft <= 0f)
                EndGame();
        }

        public void AddScore(int pts)
        {
            if (!gameRunning) return;
            Score += pts;
            UpdateUI();
        }

        public void LoseLife()
        {
            if (!gameRunning) return;
            Lives--;
            UpdateUI();

            if (Lives <= 0)
                EndGame();
        }

        private void EndGame()
        {
            if (!gameRunning) return;
            gameRunning = false;

            bird?.EnableInput(false);
            wordSpawner?.StopSpawning();

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
            if (finalScoreText != null)
                finalScoreText.text = $"Score: {Score}";

            OnGameEnd?.Invoke(Score);
        }

        private void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {Score}";
            if (livesText != null)
                livesText.text = $"Lives: {Lives}";
        }
    }
}
