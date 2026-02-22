using UnityEngine;
using UnityEngine.SceneManagement;
using FPTSim.Events;

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

        // for UI to subscribe
        public System.Action OnStateChanged;

        private void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
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
            State.Reset();
            SaveSystem.Save(State);
            Phase = GamePhase.Campus;
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.Campus);
        }

        public void LoadOrNew(GameConfigSO cfg)
        {
            Init(cfg);
            if (SaveSystem.TryLoad(out var loaded))
            {
                State = loaded;
            }
            else
            {
                State.Reset();
                SaveSystem.Save(State);
            }

            Phase = GamePhase.Campus;
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.Campus);
        }

        public bool CanPlayMinigame()
        {
            return State.playedMinigamesToday < config.maxMinigamesPerDay
                   && State.currentDay <= config.totalDays;
        }

        public int PointsFor(Medal medal)
        {
            return medal switch
            {
                Medal.Gold => config.goldPoints,
                Medal.Silver => config.silverPoints,
                Medal.Bronze => config.bronzePoints,
                _ => 0
            };
        }

        public void RegisterMinigameResult(MinigameResult result)
        {
            // increment medal counts
            switch (result.medal)
            {
                case Medal.Gold: State.gold++; break;
                case Medal.Silver: State.silver++; break;
                case Medal.Bronze: State.bronze++; break;
            }

            State.totalScore += result.scoreAwarded;
            State.playedMinigamesToday++;
            State.history.Add($"{State.currentDay}:{result.minigameId}:{result.medal}:{result.scoreAwarded}");

            // Random event roll after each minigame
            RandomEventManager.TryRollAfterMinigame(config, State);

            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
        }

        public void EndDay()
        {
            Phase = GamePhase.DayEnd;
            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
            SceneManager.LoadScene(SceneNames.DayEnd);
        }

        public void NextDayOrFinish()
        {
            State.currentDay++;
            State.playedMinigamesToday = 0;

            SaveSystem.Save(State);
            OnStateChanged?.Invoke();

            if (State.currentDay > config.totalDays)
            {
                FinishGame();
            }
            else
            {
                Phase = GamePhase.Campus;
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

        public string DecideEnding()
        {
            // Priority as described
            if (State.disappeared) return "BAD_3_DISAPPEARED";
            if (State.usedToolCheat) return "BAD_1_CHEAT";
            if (State.stress >= config.stressThreshold) return "BAD_2_STRESS";
            return "GOOD_OR_NORMAL";
        }

        // Helpers for narrative choices
        public void SetUsedToolCheat(bool value)
        {
            State.usedToolCheat = value;
            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
        }

        public void AddStress(int amount)
        {
            State.stress += amount;
            if (State.stress < 0) State.stress = 0;
            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
        }

        public void SetDisappeared(bool value)
        {
            State.disappeared = value;
            SaveSystem.Save(State);
            OnStateChanged?.Invoke();
        }
    }
}