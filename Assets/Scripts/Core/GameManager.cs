using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPTSim.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        [SerializeField] private GameConfigSO config;
        public GameConfigSO Config => config;

        public GameState State { get; private set; } = new GameState();
        public GamePhase Phase { get; private set; } = GamePhase.Boot;

        public System.Action OnStateChanged;

        private float uiTick;

        private void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // Timer chạy khi đang ở Campus (bạn có thể đổi để chạy cả trong minigame nếu muốn)
            if (Phase != GamePhase.Campus) return;
            if (config == null) return;
            if (State.isGameOver) return;

            if (State.timeLeft > 0f)
            {
                State.timeLeft -= Time.deltaTime;
                if (State.timeLeft < 0f) State.timeLeft = 0f;

                uiTick += Time.deltaTime;
                if (uiTick >= 0.2f)
                {
                    uiTick = 0f;
                    OnStateChanged?.Invoke();
                }

                if (State.timeLeft <= 0f)
                {
                    Lose();
                }
            }
        }

        public void LoadOrNew(GameConfigSO cfg)
        {
            config = cfg;

            // Bạn đang dùng SaveSystem? Nếu muốn, giữ load.
            // Ở đây mình cho chạy "new run" cho đơn giản:
            State.Reset(config.runDurationSeconds);
            SaveSystem.Save(State);

            Phase = GamePhase.Campus;
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.Campus);
        }

        public void NewRun(bool deleteSave = true)
        {
            if (deleteSave) SaveSystem.Delete();
            State.Reset(config.runDurationSeconds);
            SaveSystem.Save(State);

            Phase = GamePhase.Campus;
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.Campus);
        }

        public void RegisterMinigameResult(MinigameResult result)
        {
            if (State.isGameOver) return;

            switch (result.medal)
            {
                case Medal.Gold: State.gold++; break;
                case Medal.Silver: State.silver++; break;
                case Medal.Bronze: State.bronze++; break;
            }

            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
        }

        // ===== Buy Time =====
        public bool TryBuyTimeWithBronze()
        {
            if (State.bronze <= 0) return false;
            State.bronze--;
            AddTime(config.addSecondsBronze);
            return true;
        }

        public bool TryBuyTimeWithSilver()
        {
            if (State.silver <= 0) return false;
            State.silver--;
            AddTime(config.addSecondsSilver);
            return true;
        }

        public bool TryBuyTimeWithGold()
        {
            if (State.gold <= 0) return false;
            State.gold--;
            AddTime(config.addSecondsGold);
            return true;
        }

        private void AddTime(int seconds)
        {
            State.timeLeft += seconds;

            if (config.maxTimeCapSeconds > 0)
                State.timeLeft = Mathf.Min(State.timeLeft, config.maxTimeCapSeconds);

            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
        }

        // ===== Submit Medals to Win =====
        public bool CanSubmitToWin()
        {
            return State.gold >= config.requiredGold
                && State.silver >= config.requiredSilver
                && State.bronze >= config.requiredBronze;
        }

        public bool TrySubmitToWin()
        {
            if (!CanSubmitToWin()) return false;

            // trừ đủ huy chương yêu cầu
            State.gold -= config.requiredGold;
            State.silver -= config.requiredSilver;
            State.bronze -= config.requiredBronze;

            Win();
            return true;
        }

        private void Win()
        {
            if (State.isGameOver) return;

            State.isGameOver = true;
            State.isWin = true;

            SaveSystem.Save(State);
            OnStateChanged?.Invoke();

            Phase = GamePhase.Ending;
            SceneManager.LoadScene(SceneNames.Ending);
        }

        private void Lose()
        {
            if (State.isGameOver) return;

            State.isGameOver = true;
            State.isWin = false;

            SaveSystem.Save(State);
            OnStateChanged?.Invoke();

            Phase = GamePhase.Ending;
            SceneManager.LoadScene(SceneNames.Ending);
        }

        public string DecideEnding()
        {
            return State.isWin ? "HAPPY_END" : "BAD_TIME_OUT";
        }
    }
}