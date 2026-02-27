using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StackTower
{
    public enum GameState
    {
        MainMenu,
        LevelSelect,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    /// <summary>
    /// Central game manager for Stack Tower. Singleton.
    /// Coordinates BlockSpawner, TowerBuilder, Camera, and UI.
    /// </summary>
    public class StackTowerGameManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────
        public static StackTowerGameManager Instance { get; private set; }

        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Level Configuration")]
        [SerializeField] private List<LevelData> levels;
        [SerializeField] private StackTowerSettings settings;

        [Header("References")]
        [SerializeField] private BlockSpawner blockSpawner;
        [SerializeField] private TowerBuilder towerBuilder;
        [SerializeField] private TowerCameraController cameraController;
        [SerializeField] private StackTowerUIManager uiManager;
        [SerializeField] private PerfectEffect perfectEffect;

        [Header("Base Block")]
        [SerializeField] private GameObject baseBlockPrefab;

        // ── State ────────────────────────────────────────────────────────────
        private GameState _state = GameState.MainMenu;
        private LevelData _currentLevel;
        private int _currentLevelIndex;
        private int _currentFloor;        // 0 = base block placed, 1+ = floors built
        private int _perfectCount;
        private int _totalPlacements;
        private float _currentBlockSizeX;
        private float _currentBlockSizeZ;

        private GameObject _baseBlockGO;

        // ── Public Properties ────────────────────────────────────────────────
        public GameState State => _state;
        public int CurrentFloor => _currentFloor;
        public int TargetFloors => _currentLevel != null ? _currentLevel.targetFloors : 0;
        public int PerfectCount => _perfectCount;
        public int TotalPlacements => _totalPlacements;
        public LevelData CurrentLevel => _currentLevel;

        // ── Events ───────────────────────────────────────────────────────────
        public System.Action<GameState> OnStateChanged;
        public System.Action<int, int> OnFloorChanged;     // (current, target)
        public System.Action<int> OnPerfectComboChanged;   // combo count
        public System.Action<int> OnVictory;               // stars earned
        public System.Action OnGameOver;

        // ── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            AutoWireReferences();

            if (blockSpawner != null)
                blockSpawner.OnBlockDropped += HandleBlockDropped;
        }

        private void AutoWireReferences()
        {
            if (blockSpawner == null) blockSpawner = FindObjectOfType<BlockSpawner>();
            if (towerBuilder == null) towerBuilder = FindObjectOfType<TowerBuilder>();
            if (cameraController == null) cameraController = FindObjectOfType<TowerCameraController>();
            if (uiManager == null) uiManager = FindObjectOfType<StackTowerUIManager>();
            if (perfectEffect == null) perfectEffect = FindObjectOfType<PerfectEffect>();
            if (settings == null) settings = Resources.Load<StackTowerSettings>("StackTower/StackTowerSettings");
            if (levels == null || levels.Count == 0)
            {
                var loaded = Resources.LoadAll<LevelData>("StackTower");
                if (loaded != null && loaded.Length > 0)
                {
                    levels = new List<LevelData>(loaded);
                    levels.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));
                }
            }
        }

        private void OnDestroy()
        {
            if (blockSpawner != null)
                blockSpawner.OnBlockDropped -= HandleBlockDropped;
        }

        private void Update()
        {
            if (_state != GameState.Playing) return;

            // Input: tap / click / space
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                PlayerTap();
            }
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void StartGame(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= levels.Count)
            {
                Debug.LogWarning($"[StackTower] Invalid level index: {levelIndex}");
                return;
            }

            _currentLevelIndex = levelIndex;
            _currentLevel = levels[levelIndex];

            ResetGameState();
            CreateBaseBlock();
            SpawnNextBlock();

            ChangeState(GameState.Playing);
            uiManager?.ShowHUD();
            uiManager?.UpdateFloorDisplay(_currentFloor, _currentLevel.targetFloors);
        }

        public void PauseGame()
        {
            if (_state != GameState.Playing) return;
            Time.timeScale = 0f;
            ChangeState(GameState.Paused);
            uiManager?.ShowPausePanel();
        }

        public void ResumeGame()
        {
            if (_state != GameState.Paused) return;
            Time.timeScale = 1f;
            ChangeState(GameState.Playing);
            uiManager?.HidePausePanel();
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            CleanupTower();
            StartGame(_currentLevelIndex);
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            CleanupTower();
            ChangeState(GameState.LevelSelect);
            uiManager?.ShowLevelSelect();
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            CleanupTower();
            ChangeState(GameState.MainMenu);
            uiManager?.ShowMainMenu();
        }

        // ── Private: Game Flow ───────────────────────────────────────────────

        private void PlayerTap()
        {
            if (blockSpawner == null || !blockSpawner.IsMoving) return;

            blockSpawner.DropBlock(towerBuilder.GetTopBlock(), _currentLevel.perfectThreshold);
        }

        private void HandleBlockDropped(BlockSlicer.SliceResult result)
        {
            switch (result.type)
            {
                case BlockSlicer.SliceResultType.Miss:
                    HandleMiss();
                    break;

                case BlockSlicer.SliceResultType.Perfect:
                    HandlePerfect(result);
                    break;

                case BlockSlicer.SliceResultType.Partial:
                    HandlePartial(result);
                    break;
            }
        }

        private void HandleMiss()
        {
            _totalPlacements++;
            TriggerGameOver();
        }

        private void HandlePerfect(BlockSlicer.SliceResult result)
        {
            _perfectCount++;
            _totalPlacements++;
            _currentFloor++;

            towerBuilder.AddBlock(result.keptBlock.transform);
            PostPlacement();
        }

        private void HandlePartial(BlockSlicer.SliceResult result)
        {
            _totalPlacements++;
            _currentFloor++;

            towerBuilder.AddBlock(result.keptBlock.transform);

            // Shrink the next block to match the kept size
            Transform kept = result.keptBlock.transform;
            bool movingOnX = (_currentFloor % 2 != 0); // floor was just incremented

            if (movingOnX)
                _currentBlockSizeX = result.newSize;
            else
                _currentBlockSizeZ = result.newSize;

            PostPlacement();
        }

        private void PostPlacement()
        {
            uiManager?.UpdateFloorDisplay(_currentFloor, _currentLevel.targetFloors);

            cameraController?.UpdateTarget(towerBuilder.GetNextBlockY(), _currentFloor);

            OnFloorChanged?.Invoke(_currentFloor, _currentLevel.targetFloors);

            // targetFloors <= 0 means endless mode — never trigger victory
            bool isEndless = _currentLevel.targetFloors <= 0;
            if (!isEndless && _currentFloor >= _currentLevel.targetFloors)
            {
                StartCoroutine(DelayedVictory(0.5f));
                return;
            }

            // Spawn next block after short delay
            StartCoroutine(DelayedSpawnNextBlock(0.15f));
        }

        private void TriggerGameOver()
        {
            ChangeState(GameState.GameOver);
            StartCoroutine(GameOverSequence());
        }

        private IEnumerator GameOverSequence()
        {
            if (cameraController != null)
                cameraController.TriggerShake(0.3f, 0.2f);

            yield return new WaitForSeconds(0.8f);

            uiManager?.ShowGameOverPanel(_currentFloor, _currentLevel.targetFloors);
            OnGameOver?.Invoke();
        }

        private IEnumerator DelayedVictory(float delay)
        {
            ChangeState(GameState.Victory);
            yield return new WaitForSeconds(delay);

            int stars = CalculateStars();
            SaveProgress(stars);

            uiManager?.ShowVictoryPanel(_currentFloor, stars, _perfectCount, _totalPlacements);
            OnVictory?.Invoke(stars);
        }

        private IEnumerator DelayedSpawnNextBlock(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_state == GameState.Playing)
                SpawnNextBlock();
        }

        // ── Private: Helpers ─────────────────────────────────────────────────

        private void SpawnNextBlock()
        {
            if (_currentLevel == null || towerBuilder == null || blockSpawner == null) return;

            float speed = _currentLevel.GetSpeedAtFloor(_currentFloor);
            Color color = Random.ColorHSV(0f, 1f, 0.65f, 1f, 0.75f, 1f);
            float nextY = towerBuilder.GetNextBlockY();

            blockSpawner.SpawnBlock(
                topBlock: towerBuilder.GetTopBlock(),
                floorIndex: _currentFloor,
                blockSizeX: _currentBlockSizeX,
                blockSizeZ: _currentBlockSizeZ,
                speed: speed,
                blockY: nextY,
                color: color);
        }

        private void CreateBaseBlock()
        {
            float blockH = settings != null ? settings.blockHeight : 0.4f;
            float size = _currentLevel.initialBlockSize;

            if (_baseBlockGO != null)
                Destroy(_baseBlockGO);

            _baseBlockGO = baseBlockPrefab != null
                ? Instantiate(baseBlockPrefab)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            _baseBlockGO.name = "BaseBlock";
            _baseBlockGO.transform.position = Vector3.zero;
            _baseBlockGO.transform.localScale = new Vector3(size, blockH, size);

            // Color the base with a random vibrant color
            var rend = _baseBlockGO.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = Random.ColorHSV(0f, 1f, 0.65f, 1f, 0.75f, 1f);

            towerBuilder.Initialize(blockH);
            towerBuilder.SetBaseBlock(_baseBlockGO.transform);
        }

        private void ResetGameState()
        {
            _currentFloor = 0;
            _perfectCount = 0;
            _totalPlacements = 0;
            _currentBlockSizeX = _currentLevel.initialBlockSize;
            _currentBlockSizeZ = _currentLevel.initialBlockSize;

            blockSpawner?.DestroyCurrentBlock();
        }

        private void CleanupTower()
        {
            blockSpawner?.DestroyCurrentBlock();
            towerBuilder?.ClearTower();
            if (_baseBlockGO != null)
            {
                Destroy(_baseBlockGO);
                _baseBlockGO = null;
            }
        }

        private void UpdateBlockSizes(float sizeX, float sizeZ)
        {
            _currentBlockSizeX = sizeX;
            _currentBlockSizeZ = sizeZ;
        }

        private int CalculateStars()
        {
            if (_totalPlacements == 0) return 1;
            float ratio = (float)_perfectCount / _totalPlacements;

            if (ratio >= _currentLevel.threeStarPerfectRatio) return 3;
            if (ratio >= _currentLevel.twoStarPerfectRatio) return 2;
            return 1;
        }

        private void SaveProgress(int stars)
        {
            if (settings == null) return;
            string key = settings.GetStarsKey(_currentLevel.levelNumber);
            int saved = PlayerPrefs.GetInt(key, 0);
            if (stars > saved)
                PlayerPrefs.SetInt(key, stars);

            PlayerPrefs.Save();
        }

        private void ChangeState(GameState newState)
        {
            _state = newState;
            OnStateChanged?.Invoke(newState);
        }

        // ── Public: Level Accessors for UI ───────────────────────────────────

        public int GetSavedStars(int levelIndex)
        {
            if (settings == null || levelIndex < 0 || levelIndex >= levels.Count) return 0;
            string key = settings.GetStarsKey(levels[levelIndex].levelNumber);
            return PlayerPrefs.GetInt(key, 0);
        }

        public int LevelCount => levels != null ? levels.Count : 0;
        public LevelData GetLevelData(int index) => (index >= 0 && index < levels.Count) ? levels[index] : null;
    }
}
