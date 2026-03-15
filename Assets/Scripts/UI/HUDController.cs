using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Core;
using FPTSim.Dialogue;

namespace FPTSim.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Top Info")]
        [SerializeField] private TMP_Text timeText;

        [Header("Medals Count (numbers next to icons)")]
        [SerializeField] private TMP_Text goldCountText;
        [SerializeField] private TMP_Text silverCountText;
        [SerializeField] private TMP_Text bronzeCountText;

        [Header("Settings")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsUIController settingsUI;

        [Header("Dialogue refs (optional)")]
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private FPTSim.Minigames.CampusMinigameRenderHost minigameHost;

        [Header("Block ESC when other UI panels are open (optional)")]
        [SerializeField] private GameObject[] otherOpenUIPanels;

        private void OnEnable()
        {
            if (GameManager.I != null)
                GameManager.I.OnStateChanged += Refresh;

            if (settingsButton)
                settingsButton.onClick.AddListener(ToggleSettings);

            Refresh();
        }

        private void OnDisable()
        {
            if (GameManager.I != null)
                GameManager.I.OnStateChanged -= Refresh;

            if (settingsButton)
                settingsButton.onClick.RemoveListener(ToggleSettings);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsOtherUIOpen()) return;
                ToggleSettings();
            }
        }

        private void ToggleSettings()
        {
            if (settingsUI == null) return;

            if (IsOtherUIOpen() && !settingsUI.IsOpen) return;

            if (settingsUI.IsOpen) settingsUI.Close();
            else settingsUI.Open();
        }

        private bool IsOtherUIOpen()
        {
            if (otherOpenUIPanels == null) return false;

            for (int i = 0; i < otherOpenUIPanels.Length; i++)
            {
                var go = otherOpenUIPanels[i];
                if (go != null && go.activeInHierarchy)
                    return true;
            }
            return false;
        }

        private void Refresh()
        {
            if (GameManager.I == null) return;

            var s = GameManager.I.State;

            if (timeText)
            {
                int totalSeconds = Mathf.CeilToInt(s.timeLeft);
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                timeText.text = $"{minutes:00}:{seconds:00}";
            }

            if (goldCountText) goldCountText.text = s.gold.ToString();
            if (silverCountText) silverCountText.text = s.silver.ToString();
            if (bronzeCountText) bronzeCountText.text = s.bronze.ToString();
        }
    }
}
