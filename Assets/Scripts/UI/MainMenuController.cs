using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FPTSim.Core;

namespace FPTSim.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel; // bật/tắt panel settings

        private void Start()
        {
            // Continue chỉ hiện khi có save
            if (continueButton)
                continueButton.gameObject.SetActive(SaveSystem.HasSave());

            if (newGameButton) newGameButton.onClick.AddListener(OnNewGame);
            if (continueButton) continueButton.onClick.AddListener(OnContinue);
            if (settingsButton) settingsButton.onClick.AddListener(OpenSettings);
            if (quitButton) quitButton.onClick.AddListener(OnQuit);

            if (settingsPanel) settingsPanel.SetActive(false);
        }

        private void OnNewGame()
        {
            // đảm bảo có GameManager
            if (GameManager.I == null)
            {
                Debug.LogError("GameManager chưa có. Hãy chạy từ Boot hoặc có AutoBootstrap.");
                return;
            }

            GameManager.I.NewGame(deleteSave: true);
        }

        private void OnContinue()
        {
            if (GameManager.I == null)
            {
                Debug.LogError("GameManager chưa có. Hãy chạy từ Boot hoặc có AutoBootstrap.");
                return;
            }

            GameManager.I.LoadOrNew(GameManager.I.Config);
        }

        private void OpenSettings()
        {
            if (settingsPanel) settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            if (settingsPanel) settingsPanel.SetActive(false);
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}