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

        public System.Action OnStateChanged;

        private float hudTick; // giảm spam UI update

        private void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // Chỉ đếm thời gian khi đang ở Campus (1 ngày diễn ra ở đây)
            if (Phase != GamePhase.Campus) return;
            if (config == null) return;

            if (State.dayTimeLeft > 0f)
            {
                State.dayTimeLeft -= Time.deltaTime;
                if (State.dayTimeLeft < 0f) State.dayTimeLeft = 0f;

                // Update HUD mỗi 0.2s cho mượt mà, đỡ Invoke liên tục
                hudTick += Time.deltaTime;
                if (hudTick >= 0.2f)
                {
                    hudTick = 0f;
                    OnStateChanged?.Invoke();
                }

                // Hết giờ => tự end day
                if (State.dayTimeLeft <= 0f)
                {
                    EndDay();
                }
            }
        }

        public void Init(GameConfigSO cfg)
        {
            if (config == null) config = cfg;
            Phase = GamePhase.Boot;
            OnStateChanged?.Invoke();
        }

        public void NewGame(bool deleteSave = true)
        {
            if (deleteSave) SaveSystem.Delete();

            State.Reset(config.dayDurationSeconds);
            SaveSystem.Save(State);

            Phase = GamePhase.Campus;
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.Campus);
        }

        public void LoadOrNew(GameConfigSO cfg)
        {
            Init(cfg);

            if (SaveSystem.TryLoad(out var loaded))
                State = loaded;
            else
            {
                State.Reset(config.dayDurationSeconds);
                SaveSystem.Save(State);
            }

            // Nếu save cũ không có dayTimeLeft (bị 0), ta set lại cho an toàn
            if (State.dayTimeLeft <= 0f)
                State.dayTimeLeft = config.dayDurationSeconds;

            Phase = GamePhase.Campus;
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.Campus);
        }

        // Bỏ giới hạn số mini-game/ngày => chỉ check còn trong 7 ngày
        public bool CanPlayMinigame()
        {
            return State.currentDay <= config.totalDays && State.dayTimeLeft > 0f;
        }

        public void RegisterMinigameResult(MinigameResult result)
        {
            // cộng medals
            switch (result.medal)
            {
                case Medal.Gold: State.gold++; break;
                case Medal.Silver: State.silver++; break;
                case Medal.Bronze: State.bronze++; break;
            }

            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
        }

        public void EndDay()
        {
            // chống gọi nhiều lần khi timer vừa chạm 0
            if (Phase == GamePhase.DayEnd || Phase == GamePhase.Ending) return;

            Phase = GamePhase.DayEnd;
            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.DayEnd);
        }

        public void NextDayOrFinish()
        {
            State.currentDay++;
            SaveSystem.Save(State);
            OnStateChanged?.Invoke();

            if (State.currentDay > config.totalDays)
            {
                FinishGame();
            }
            else
            {
                // Reset timer ngày mới
                State.StartNewDay(config.dayDurationSeconds);
                SaveSystem.Save(State);

                Phase = GamePhase.Campus;
                OnStateChanged?.Invoke();
                SceneManager.LoadScene(SceneNames.Campus);
            }
        }

        public void FinishGame()
        {
            Phase = GamePhase.Ending;
            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.Ending);
        }
    }
}