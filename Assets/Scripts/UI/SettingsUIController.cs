using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Player;

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

        [Header("Optional: Lock gameplay while open")]
        [SerializeField] private MouseLook mouseLook;
        [SerializeField] private MonoBehaviour playerMovement;
        [SerializeField] private FPTSim.Player.Interactor playerInteractor;

        private void Awake()
        {
            if (audioTabButton) audioTabButton.onClick.AddListener(() => ShowTab("audio"));
            if (controlsTabButton) controlsTabButton.onClick.AddListener(() => ShowTab("controls"));
            if (helpTabButton) helpTabButton.onClick.AddListener(() => ShowTab("help"));
            if (closeButton) closeButton.onClick.AddListener(Close);

            if (settingsRoot) settingsRoot.SetActive(false);
        }

        public bool IsOpen => settingsRoot != null && settingsRoot.activeSelf;
        public void Open()
        {
            if (settingsRoot) settingsRoot.SetActive(true);

            // mặc định mở Audio
            ShowTab("audio");

            // khóa gameplay + hiện chuột
            LockGameplay(true);
        }

        public void Close()
        {
            if (settingsRoot) settingsRoot.SetActive(false);

            // mở lại gameplay + khóa chuột
            LockGameplay(false);
        }

        private void ShowTab(string tab)
        {
            if (audioPanel) audioPanel.SetActive(tab == "audio");
            if (controlsPanel) controlsPanel.SetActive(tab == "controls");
            if (helpPanel) helpPanel.SetActive(tab == "help");
        }

        private void LockGameplay(bool locked)
        {
            // locked=true => mở chuột + tắt movement/interact
            if (mouseLook) mouseLook.LockCursor(!locked);
            if (playerMovement) playerMovement.enabled = !locked;
            if (playerInteractor) playerInteractor.enabled = !locked;
        }
    }
}