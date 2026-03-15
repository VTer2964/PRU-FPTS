using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Player;
using UnityEngine.SceneManagement;
using FPTSim.Dialogue;
using FPTSim.Minigames;

namespace FPTSim.UI
{
    public class SettingsUIController : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject settingsRoot;

        [Header("Tabs Buttons")]
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button controlsTabButton;
        [SerializeField] private Button helpTabButton;

        [Header("Panels")]
        [SerializeField] private GameObject audioPanel;
        [SerializeField] private GameObject controlsPanel;
        [SerializeField] private GameObject helpPanel;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        [Header("Navigation Actions")]
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitGameButton;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Confirmation UI")]
        [SerializeField] private GameObject confirmRoot;
        [SerializeField] private TMP_Text confirmTitleText;
        [SerializeField] private TMP_Text confirmMessageText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        [Header("Optional: Lock gameplay while open")]
        [SerializeField] private MouseLook mouseLook;
        [SerializeField] private MonoBehaviour playerMovement;
        [SerializeField] private FPTSim.Player.Interactor playerInteractor;
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private CampusMinigameRenderHost minigameHost;

        private enum PendingAction
        {
            None,
            MainMenu,
            Quit
        }

        private PendingAction pendingAction;

        private void Awake()
        {
            AutoBindActionButtons();
            AutoBindConfirmationUI();
            TryAutoBindStateRefs();

            if (audioTabButton) audioTabButton.onClick.AddListener(() => ShowTab("audio"));
            if (controlsTabButton) controlsTabButton.onClick.AddListener(() => ShowTab("controls"));
            if (helpTabButton) helpTabButton.onClick.AddListener(() => ShowTab("help"));
            if (closeButton) closeButton.onClick.AddListener(Close);

            if (mainMenuButton) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            if (quitGameButton) quitGameButton.onClick.AddListener(OnQuitClicked);
            if (confirmYesButton) confirmYesButton.onClick.AddListener(ConfirmActionInternal);
            if (confirmNoButton) confirmNoButton.onClick.AddListener(CancelConfirmation);

            if (settingsRoot) settingsRoot.SetActive(false);
            if (confirmRoot) confirmRoot.SetActive(false);
        }

        public bool IsOpen => settingsRoot != null && settingsRoot.activeSelf;

        public void Open()
        {
            if (settingsRoot) settingsRoot.SetActive(true);

            ShowTab("audio");
            ApplyOverlayLock();
        }

        public void Close()
        {
            CancelConfirmation();
            if (settingsRoot) settingsRoot.SetActive(false);

            RestoreGameplayState();
        }

        public void OnMainMenuClicked()
        {
            ShowConfirmation(
                PendingAction.MainMenu,
                "Confirm Main Menu",
                "Are you sure you want to return to Main Menu?"
            );
        }

        public void OnQuitClicked()
        {
            ShowConfirmation(
                PendingAction.Quit,
                "Confirm Quit",
                "Are you sure you want to quit the game?"
            );
        }

        private void ShowTab(string tab)
        {
            if (audioPanel) audioPanel.SetActive(tab == "audio");
            if (controlsPanel) controlsPanel.SetActive(tab == "controls");
            if (helpPanel) helpPanel.SetActive(tab == "help");
        }

        private void ShowConfirmation(PendingAction action, string title, string message)
        {
            if (!HasConfirmationUI())
            {
                Debug.LogWarning("[SettingsUIController] Missing confirmation panel UI. Create your own confirm panel and assign Yes/No buttons.");
                pendingAction = PendingAction.None;
                return;
            }

            pendingAction = action;

            if (confirmTitleText) confirmTitleText.text = title;
            if (confirmMessageText) confirmMessageText.text = message;
            confirmRoot.SetActive(true);
        }

        private void ConfirmActionInternal()
        {
            PendingAction action = pendingAction;
            CancelConfirmation();

            if (action == PendingAction.MainMenu)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(mainMenuSceneName);
                return;
            }

            if (action == PendingAction.Quit)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        private void CancelConfirmation()
        {
            pendingAction = PendingAction.None;
            if (confirmRoot) confirmRoot.SetActive(false);
        }

        private void AutoBindActionButtons()
        {
            if (settingsRoot == null) return;

            if (mainMenuButton == null)
                mainMenuButton = FindButtonByNames("MainMenuButton", "BtnMainMenu", "MainMenu", "BackToMenuButton");

            if (quitGameButton == null)
                quitGameButton = FindButtonByNames("QuitButton", "BtnQuit", "ExitButton", "QuitGameButton");
        }

        private void AutoBindConfirmationUI()
        {
            if (settingsRoot == null) return;

            if (confirmRoot == null)
                confirmRoot = FindChildByNames("ConfirmPanel", "ConfirmationPanel", "ConfirmRoot", "QuitConfirmPanel");

            if (confirmRoot == null) return;

            if (confirmYesButton == null)
                confirmYesButton = FindButtonInRoot(confirmRoot, "YesButton", "BtnYes", "ConfirmYesButton", "Yes");

            if (confirmNoButton == null)
                confirmNoButton = FindButtonInRoot(confirmRoot, "NoButton", "BtnNo", "ConfirmNoButton", "No", "CancelButton");

            if (confirmTitleText == null)
                confirmTitleText = FindTextInRoot(confirmRoot, "TitleText", "ConfirmTitle", "HeaderText", "Title");

            if (confirmMessageText == null)
                confirmMessageText = FindTextInRoot(confirmRoot, "MessageText", "ConfirmMessage", "BodyText", "Message");
        }

        private Button FindButtonByNames(params string[] names)
        {
            Button[] buttons = settingsRoot.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                string buttonName = buttons[i].name;
                for (int j = 0; j < names.Length; j++)
                {
                    if (buttonName.Equals(names[j], System.StringComparison.OrdinalIgnoreCase))
                        return buttons[i];
                }
            }
            return null;
        }

        private GameObject FindChildByNames(params string[] names)
        {
            Transform[] transforms = settingsRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                string objectName = transforms[i].name;
                for (int j = 0; j < names.Length; j++)
                {
                    if (objectName.Equals(names[j], System.StringComparison.OrdinalIgnoreCase))
                        return transforms[i].gameObject;
                }
            }
            return null;
        }

        private Button FindButtonInRoot(GameObject root, params string[] names)
        {
            if (root == null) return null;

            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                string buttonName = buttons[i].name;
                for (int j = 0; j < names.Length; j++)
                {
                    if (buttonName.Equals(names[j], System.StringComparison.OrdinalIgnoreCase))
                        return buttons[i];
                }
            }

            return null;
        }

        private TMP_Text FindTextInRoot(GameObject root, params string[] names)
        {
            if (root == null) return null;

            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                string textName = texts[i].name;
                for (int j = 0; j < names.Length; j++)
                {
                    if (textName.Equals(names[j], System.StringComparison.OrdinalIgnoreCase))
                        return texts[i];
                }
            }

            return null;
        }

        private bool HasConfirmationUI()
        {
            return confirmRoot != null &&
                   confirmYesButton != null &&
                   confirmNoButton != null;
        }

        private void TryAutoBindStateRefs()
        {
            if (dialogueRunner == null)
                dialogueRunner = FindFirstObjectByType<DialogueRunner>();

            if (minigameHost == null)
                minigameHost = FindFirstObjectByType<CampusMinigameRenderHost>();
        }

        private void ApplyOverlayLock()
        {
            if (mouseLook) mouseLook.LockCursor(false);
            if (playerMovement) playerMovement.enabled = false;
            if (playerInteractor) playerInteractor.enabled = false;
        }

        private void RestoreGameplayState()
        {
            TryAutoBindStateRefs();

            if (IsOverlayGameplayModeActive())
            {
                ApplyOverlayLock();
                return;
            }

            if (mouseLook) mouseLook.LockCursor(true);
            if (playerMovement) playerMovement.enabled = true;
            if (playerInteractor) playerInteractor.enabled = true;
        }

        private bool IsOverlayGameplayModeActive()
        {
            return (dialogueRunner != null && dialogueRunner.IsRunning) ||
                   (minigameHost != null && minigameHost.IsRunning);
        }

        private void OnDestroy()
        {
            if (mainMenuButton) mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            if (quitGameButton) quitGameButton.onClick.RemoveListener(OnQuitClicked);
            if (confirmYesButton) confirmYesButton.onClick.RemoveListener(ConfirmActionInternal);
            if (confirmNoButton) confirmNoButton.onClick.RemoveListener(CancelConfirmation);
        }
    }
}
