using UnityEngine;
using TMPro;
using UnityEngine.UI;
using FPTSim.Core;
using FPTSim.Player;

namespace FPTSim.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text dayText;
        [SerializeField] private TMP_Text playsText;
        [SerializeField] private TMP_Text scoreText;

        [Header("Buttons")]
        [SerializeField] private Button openMinigameButton;
        [SerializeField] private Button endDayButton;

        [Header("Panels")]
        [SerializeField] private GameObject minigameSelectPanel;
        [SerializeField] private MouseLook mouseLook;

        [SerializeField] private MonoBehaviour playerMovement;
        private void OnEnable()
        {
            if (GameManager.I != null)
                GameManager.I.OnStateChanged += Refresh;

            Refresh();

            if (openMinigameButton != null)
                openMinigameButton.onClick.AddListener(OpenMinigamePanel);

            if (endDayButton != null)
                endDayButton.onClick.AddListener(EndDay);
        }

        private void OnDisable()
        {
            if (GameManager.I != null)
                GameManager.I.OnStateChanged -= Refresh;

            if (openMinigameButton != null)
                openMinigameButton.onClick.RemoveListener(OpenMinigamePanel);

            if (endDayButton != null)
                endDayButton.onClick.RemoveListener(EndDay);
        }

        private void Refresh()
        {
            if (GameManager.I == null) return;

            var s = GameManager.I.State;
            var c = GameManager.I.Config;

            if (dayText) dayText.text = $"Day: {s.currentDay}/{c.totalDays}";
            if (playsText) playsText.text = $"Minigames: {s.playedMinigamesToday}/{c.maxMinigamesPerDay}";
            if (scoreText) scoreText.text =
                $"Score: {s.totalScore}  |  G:{s.gold} S:{s.silver} B:{s.bronze}  |  Stress:{s.stress}";

            if (openMinigameButton) openMinigameButton.interactable = GameManager.I.CanPlayMinigame();
            if (endDayButton) endDayButton.interactable = true; // cho phép end day bất kỳ lúc nào
        }

        private void OpenMinigamePanel()
        {
            if (minigameSelectPanel)
                minigameSelectPanel.SetActive(true);

            if (mouseLook)
                mouseLook.LockCursor(false);
        }

        public void CloseMinigamePanel()
        {
            if (minigameSelectPanel)
                minigameSelectPanel.SetActive(false);

            if (mouseLook)
                mouseLook.LockCursor(true);
        }

        private void EndDay()
        {
            GameManager.I.EndDay();
        }

        public void OpenPanelFromWorld()
        {
            // Mở panel chọn minigame
            if (minigameSelectPanel) minigameSelectPanel.SetActive(true);

            // Nếu bạn có quản lý FPS lock/unlock chuột
            if (mouseLook) mouseLook.LockCursor(false);

            // Nếu bạn có tắt movement khi mở UI
            if (playerMovement) playerMovement.enabled = false;
        }
    }
}