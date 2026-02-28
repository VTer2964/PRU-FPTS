using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPTSim.Core
{

    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        [Header("Config")]
        [SerializeField] private GameConfigSO config;
        public GameConfigSO Config => config;

        public GameState State { get; private set; } = new GameState();
        public GamePhase Phase { get; private set; } = GamePhase.Boot;

        // UI subscribe
        public System.Action OnStateChanged;

        private float uiTick;

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // Chỉ chạy timer khi đang ở Campus và game chưa kết thúc
            if (Phase != GamePhase.Campus) return;
            if (config == null) return;
            if (State == null) return;
            if (State.isGameOver) return;

            if (State.timeLeft > 0f)
            {
                State.timeLeft -= Time.deltaTime;
                if (State.timeLeft < 0f) State.timeLeft = 0f;

                // refresh UI không cần mỗi frame
                uiTick += Time.deltaTime;
                if (uiTick >= 0.2f)
                {
                    uiTick = 0f;
                    OnStateChanged?.Invoke();
                }

                if (State.timeLeft <= 0f)
                {
                    Lose_TimeOut();
                }
            }
        }

        // ========== INIT / FLOW ==========

        public void Init(GameConfigSO cfg)
        {
            if (config == null) config = cfg;
            Phase = GamePhase.Boot;
            OnStateChanged?.Invoke();
        }

        public void GoMainMenu()
        {
            Phase = GamePhase.MainMenu;
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        public void NewGame(bool deleteSave = true)
        {
            if (config == null)
            {
                Debug.LogError("[GameManager] Missing GameConfigSO!");
                return;
            }

            if (deleteSave) SaveSystem.Delete();

            State.Reset(config.runDurationSeconds);
            SaveSystem.Save(State);

            Phase = GamePhase.Campus;
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.Campus);
        }

        public void LoadOrNew(GameConfigSO cfg)
        {
            if (config == null) config = cfg;

            if (config == null)
            {
                Debug.LogError("[GameManager] Missing GameConfigSO!");
                return;
            }

            if (SaveSystem.TryLoad(out var loaded))
                State = loaded;
            else
            {
                State.Reset(config.runDurationSeconds);
                SaveSystem.Save(State);
            }

            Phase = GamePhase.Campus;
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.Campus);
        }

        // ========== GAMEPLAY API ==========

        public void RegisterMinigameResult(MinigameResult result)
        {
            if (State == null || State.isGameOver) return;

            switch (result.medal)
            {
                case Medal.Gold: State.gold++; break;
                case Medal.Silver: State.silver++; break;
                case Medal.Bronze: State.bronze++; break;
            }

            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
        }

        // ===== Buy time =====
        public bool TryBuyTimeWithBronze()
        {
            if (State == null || State.isGameOver) return false;
            if (State.bronze <= 0) return false;

            State.bronze--;
            AddTimeSeconds(config.addSecondsBronze);
            return true;
        }

        public bool TryBuyTimeWithSilver()
        {
            if (State == null || State.isGameOver) return false;
            if (State.silver <= 0) return false;

            State.silver--;
            AddTimeSeconds(config.addSecondsSilver);
            return true;
        }

        public bool TryBuyTimeWithGold()
        {
            if (State == null || State.isGameOver) return false;
            if (State.gold <= 0) return false;

            State.gold--;
            AddTimeSeconds(config.addSecondsGold);
            return true;
        }

        private void AddTimeSeconds(int seconds)
        {
            if (seconds <= 0) return;

            State.timeLeft += seconds;

            // optional cap
            if (config.maxTimeCapSeconds > 0)
                State.timeLeft = Mathf.Min(State.timeLeft, config.maxTimeCapSeconds);

            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
        }

        // ===== Win by submit medals =====
        public bool CanSubmitToWin()
        {
            if (State == null || config == null) return false;

            return State.gold >= config.requiredGold
                && State.silver >= config.requiredSilver
                && State.bronze >= config.requiredBronze;
        }

        public bool TrySubmitToWin()
        {
            if (State == null || State.isGameOver) return false;
            if (!CanSubmitToWin()) return false;

            State.gold -= config.requiredGold;
            State.silver -= config.requiredSilver;
            State.bronze -= config.requiredBronze;

            Win();
            return true;
        }

        // ===== Story Flags =====
        public bool HasFlag(string flag)
        {
            return State != null && State.HasFlag(flag);
        }

        public void SetFlag(string flag)
        {
            if (State == null) return;

            State.SetFlag(flag);
            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
        }

        // ========== ENDINGS ==========

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

        private void Lose_TimeOut()
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
            // Ending scene đọc chuỗi này để hiển thị text/ảnh
            return (State != null && State.isWin) ? "HAPPY_END" : "BAD_TIME_OUT";
        }
    }
}