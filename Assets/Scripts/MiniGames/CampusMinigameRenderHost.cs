using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPTSim.Minigames
{
    public class CampusMinigameRenderHost : MonoBehaviour
    {
        [Header("Render")]
        [SerializeField] private RenderTexture minigameRenderTexture;
        [SerializeField] private string outputCameraName = "MinigameOutputCamera";

        [Header("Canvas Routing")]
        [SerializeField] private bool routeOverlayCanvasToOutputCamera = true;
        [SerializeField] private float outputCanvasPlaneDistance = 1f;

        [Header("Cinemachine (optional)")]
        [SerializeField] private FPTSim.Dialogue.DialogueCameraController dialogueCameraController;
        [SerializeField] private string tabletCameraKey = "";
        [SerializeField] private string exitCameraKey = "";

        [Header("Tablet Focus (optional)")]
        [SerializeField] private Transform tabletLookTarget;
        [SerializeField] private string tabletLookTargetName = "ipad";
        [SerializeField] private bool rotatePlayerTowardTabletOnStart = true;
        [SerializeField] private GameObject tabletVisualObject;
        [SerializeField] private string tabletVisualObjectName = "ipad";
        [SerializeField] private bool hideTabletWhenIdle = true;

        [Header("Player Lock")]
        [SerializeField] private FPTSim.Player.MouseLook mouseLook;
        [SerializeField] private MonoBehaviour playerMovement;
        [SerializeField] private FPTSim.Player.Interactor playerInteractor;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private string holdTabletBool = "IsUsingTablet";

        private string loadedMinigameScene;
        private bool transitionBusy;
        private Transform activePlayerTransform;
        private FPTSim.Audio.AudioManager.AudioSourceState campusMusicState;
        private FPTSim.Audio.AudioManager.AudioSourceState campusAmbientState;
        private readonly HashSet<int> playingAudioBeforeMinigame = new HashSet<int>();
        private readonly List<AudioSource> pausedCampusAudioSources = new List<AudioSource>();
        private readonly List<FPTSim.Audio.BandMusicPlayer> pausedBandMusicPlayers = new List<FPTSim.Audio.BandMusicPlayer>();

        public bool IsRunning => !string.IsNullOrWhiteSpace(loadedMinigameScene);

        private void Awake()
        {
            TryAutoBindPlayerRefs();
            if (dialogueCameraController == null)
                dialogueCameraController = FindFirstObjectByType<FPTSim.Dialogue.DialogueCameraController>();

            if (tabletLookTarget == null && !string.IsNullOrWhiteSpace(tabletLookTargetName))
            {
                var tabletObj = GameObject.Find(tabletLookTargetName);
                if (tabletObj != null) tabletLookTarget = tabletObj.transform;
            }

            if (tabletVisualObject == null && !string.IsNullOrWhiteSpace(tabletVisualObjectName))
                tabletVisualObject = GameObject.Find(tabletVisualObjectName);

            if (hideTabletWhenIdle)
                SetTabletVisible(false);
        }

        public bool StartMinigame(string sceneName, Transform playerTransform = null)
        {
            if (transitionBusy || IsRunning)
            {
                Debug.LogWarning("[CampusMinigameRenderHost] Minigame is already running or transitioning.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[CampusMinigameRenderHost] sceneName is empty.");
                return false;
            }

            activePlayerTransform = playerTransform;

            if (playerTransform != null)
            {
                if (mouseLook == null) mouseLook = playerTransform.GetComponentInChildren<FPTSim.Player.MouseLook>(true);
                if (playerMovement == null) playerMovement = playerTransform.GetComponent<FPTSim.Player.FirstPersonController>();
                if (playerInteractor == null) playerInteractor = playerTransform.GetComponent<FPTSim.Player.Interactor>();
                if (playerAnimator == null) playerAnimator = playerTransform.GetComponentInChildren<Animator>(true);
            }

            StartCoroutine(StartMinigameRoutine(sceneName));
            return true;
        }

        public void ExitMinigame()
        {
            if (transitionBusy || !IsRunning) return;
            StartCoroutine(ExitMinigameRoutine());
        }

        private IEnumerator StartMinigameRoutine(string sceneName)
        {
            transitionBusy = true;
            CapturePlayingAudioSnapshot();
            CaptureCampusEnvironmentState();
            PauseCampusAudioSources();
            SetTabletVisible(true);
            RotatePlayerTowardTablet();
            SetPlayerLock(true);
            FocusTabletCamera();

            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOp == null)
            {
                Debug.LogError($"[CampusMinigameRenderHost] Cannot load minigame scene '{sceneName}'.");
                RestoreCampusEnvironmentState();
                ResumeCampusAudioSources();
                RestoreCampusCamera();
                SetTabletVisible(false);
                SetPlayerLock(false);
                transitionBusy = false;
                yield break;
            }

            while (!loadOp.isDone) yield return null;

            loadedMinigameScene = sceneName;
            BindRenderTargetsAndCanvas(sceneName);
            transitionBusy = false;
        }

        private IEnumerator ExitMinigameRoutine()
        {
            transitionBusy = true;

            string sceneToUnload = loadedMinigameScene;
            var scene = SceneManager.GetSceneByName(sceneToUnload);
            if (scene.IsValid() && scene.isLoaded)
            {
                StopAllAudioSourcesInScene(scene);
                var unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
                if (unloadOp != null)
                {
                    while (!unloadOp.isDone) yield return null;
                }
            }

            StopNewAudioStartedDuringMinigame();
            RestoreCampusEnvironmentState();
            ResumeCampusAudioSources();

            loadedMinigameScene = null;
            activePlayerTransform = null;
            RestoreCampusCamera();
            if (hideTabletWhenIdle) SetTabletVisible(false);
            SetPlayerLock(false);
            transitionBusy = false;
        }

        private void BindRenderTargetsAndCanvas(string sceneName)
        {
            if (minigameRenderTexture == null)
            {
                Debug.LogWarning("[CampusMinigameRenderHost] minigameRenderTexture is not assigned.");
                return;
            }

            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogWarning($"[CampusMinigameRenderHost] Scene '{sceneName}' is not loaded.");
                return;
            }

            Camera targetCam = null;
            var roots = scene.GetRootGameObjects();

            foreach (var root in roots)
            {
                var cams = root.GetComponentsInChildren<Camera>(true);
                foreach (var cam in cams)
                {
                    if (!string.IsNullOrWhiteSpace(outputCameraName) && cam.name == outputCameraName)
                    {
                        targetCam = cam;
                        break;
                    }
                }
                if (targetCam != null) break;
            }

            if (targetCam == null)
            {
                foreach (var root in roots)
                {
                    var cams = root.GetComponentsInChildren<Camera>(true);
                    if (cams.Length > 0)
                    {
                        targetCam = cams[0];
                        break;
                    }
                }
            }

            if (targetCam == null)
            {
                Debug.LogWarning($"[CampusMinigameRenderHost] No camera found in scene '{sceneName}'.");
                return;
            }

            foreach (var root in roots)
            {
                var cams = root.GetComponentsInChildren<Camera>(true);
                foreach (var cam in cams)
                {
                    cam.targetTexture = minigameRenderTexture;
                    cam.enabled = cam == targetCam;
                }

                var listeners = root.GetComponentsInChildren<AudioListener>(true);
                foreach (var listener in listeners)
                {
                    listener.enabled = false;
                }

                if (!routeOverlayCanvasToOutputCamera) continue;

                var canvases = root.GetComponentsInChildren<Canvas>(true);
                foreach (var canvas in canvases)
                {
                    if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = targetCam;
                    canvas.planeDistance = outputCanvasPlaneDistance;
                }
            }
        }

        private void RotatePlayerTowardTablet()
        {
            if (!rotatePlayerTowardTabletOnStart) return;
            if (activePlayerTransform == null || tabletLookTarget == null) return;

            Vector3 toTarget = tabletLookTarget.position - activePlayerTransform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            activePlayerTransform.rotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        }

        private void FocusTabletCamera()
        {
            if (dialogueCameraController == null) return;
            if (string.IsNullOrWhiteSpace(tabletCameraKey)) return;

            bool ok = dialogueCameraController.Focus(tabletCameraKey);
            if (!ok)
                Debug.LogWarning($"[CampusMinigameRenderHost] Cannot focus tablet camera key '{tabletCameraKey}'.");
        }

        private void RestoreCampusCamera()
        {
            if (dialogueCameraController == null) return;

            if (!string.IsNullOrWhiteSpace(exitCameraKey))
            {
                bool ok = dialogueCameraController.Focus(exitCameraKey);
                if (!ok)
                {
                    Debug.LogWarning($"[CampusMinigameRenderHost] Cannot focus exit camera key '{exitCameraKey}'. Using Clear().");
                    dialogueCameraController.Clear();
                }
                return;
            }

            dialogueCameraController.Clear();
        }

        private void SetPlayerLock(bool locked)
        {
            if (mouseLook != null) mouseLook.LockCursor(!locked);
            if (playerMovement != null) playerMovement.enabled = !locked;
            if (playerInteractor != null) playerInteractor.enabled = !locked;

            if (playerAnimator != null && !string.IsNullOrWhiteSpace(holdTabletBool))
            {
                playerAnimator.SetBool(holdTabletBool, locked);
            }
        }

        private void TryAutoBindPlayerRefs()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) return;

            if (mouseLook == null) mouseLook = playerObj.GetComponentInChildren<FPTSim.Player.MouseLook>(true);
            if (playerMovement == null) playerMovement = playerObj.GetComponent<FPTSim.Player.FirstPersonController>();
            if (playerInteractor == null) playerInteractor = playerObj.GetComponent<FPTSim.Player.Interactor>();
            if (playerAnimator == null) playerAnimator = playerObj.GetComponentInChildren<Animator>(true);
        }

        private void CapturePlayingAudioSnapshot()
        {
            playingAudioBeforeMinigame.Clear();
            var allSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var src in allSources)
            {
                if (src == null) continue;
                if (!src.isPlaying) continue;
                playingAudioBeforeMinigame.Add(src.GetInstanceID());
            }
        }

        private void CaptureCampusEnvironmentState()
        {
            if (FPTSim.Audio.AudioManager.I == null) return;

            campusMusicState = FPTSim.Audio.AudioManager.I.CaptureMusicState();
            campusAmbientState = FPTSim.Audio.AudioManager.I.CaptureAmbientState();
            FPTSim.Audio.AudioManager.I.StopMusic();
            FPTSim.Audio.AudioManager.I.StopAmbient();
        }

        private void RestoreCampusEnvironmentState()
        {
            if (FPTSim.Audio.AudioManager.I == null) return;

            FPTSim.Audio.AudioManager.I.RestoreMusicState(campusMusicState);
            FPTSim.Audio.AudioManager.I.RestoreAmbientState(campusAmbientState);
        }

        private void StopAllAudioSourcesInScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var sources = root.GetComponentsInChildren<AudioSource>(true);
                foreach (var src in sources)
                {
                    if (src == null) continue;
                    src.Stop();
                }
            }
        }

        private void StopNewAudioStartedDuringMinigame()
        {
            var allSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var src in allSources)
            {
                if (src == null) continue;
                if (!src.isPlaying) continue;

                int id = src.GetInstanceID();
                if (playingAudioBeforeMinigame.Contains(id)) continue;
                src.Stop();
            }

            playingAudioBeforeMinigame.Clear();
        }

        private void PauseCampusAudioSources()
        {
            pausedCampusAudioSources.Clear();
            pausedBandMusicPlayers.Clear();

            var audioManager = FPTSim.Audio.AudioManager.I;
            var handledSourceIds = new HashSet<int>();
            var bandPlayers = FindObjectsByType<FPTSim.Audio.BandMusicPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var player in bandPlayers)
            {
                if (player == null) continue;

                var src = player.ManagedAudioSource;
                if (src != null)
                    handledSourceIds.Add(src.GetInstanceID());

                if (src == null || (!src.isPlaying && src.time <= 0f)) continue;

                player.PauseMusic();
                pausedBandMusicPlayers.Add(player);
            }

            var allSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var src in allSources)
            {
                if (src == null || !src.isPlaying) continue;
                if (audioManager != null && src.transform.IsChildOf(audioManager.transform)) continue;
                if (handledSourceIds.Contains(src.GetInstanceID())) continue;

                pausedCampusAudioSources.Add(src);
                src.Pause();
            }
        }

        private void ResumeCampusAudioSources()
        {
            foreach (var player in pausedBandMusicPlayers)
            {
                if (player == null) continue;
                player.ResumeMusic();
            }

            pausedBandMusicPlayers.Clear();

            foreach (var src in pausedCampusAudioSources)
            {
                if (src == null) continue;
                if (src.isPlaying) continue;
                if (src.clip == null) continue;
                src.UnPause();
            }

            pausedCampusAudioSources.Clear();
        }

        private void SetTabletVisible(bool visible)
        {
            if (tabletVisualObject != null)
                tabletVisualObject.SetActive(visible);
        }
    }
}
