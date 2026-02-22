using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Player;

namespace FPTSim.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject panel;

        [Header("Texts")]
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text bodyText;

        [Header("Buttons")]
        [SerializeField] private Button talkButton;
        [SerializeField] private Button playMinigameButton;
        [SerializeField] private Button exitButton;

        [Header("Player Control")]
        [SerializeField] private MouseLook mouseLook;
        [SerializeField] private MonoBehaviour playerMovement; // FirstPersonController

        // Optional: nếu bạn muốn nút "Chơi minigame" mở panel chọn mini-game
        [SerializeField] private HUDController hudController;

        private System.Action onTalk;
        private System.Action onPlayMinigame;

        private bool isOpen;

        private void Awake()
        {
            if (talkButton) talkButton.onClick.AddListener(() => onTalk?.Invoke());
            if (playMinigameButton) playMinigameButton.onClick.AddListener(() => onPlayMinigame?.Invoke());
            if (exitButton) exitButton.onClick.AddListener(Close);
        }

        public void Open(
            string speaker,
            string body,
            System.Action talkAction,
            System.Action playMinigameAction)
        {
            isOpen = true;

            onTalk = talkAction;
            onPlayMinigame = playMinigameAction;

            if (speakerText) speakerText.text = speaker;
            if (bodyText) bodyText.text = body;

            if (panel) panel.SetActive(true);

            // mở chuột + freeze movement
            if (mouseLook) mouseLook.LockCursor(false);
            if (playerMovement) playerMovement.enabled = false;
        }

        public void SetBody(string body)
        {
            if (bodyText) bodyText.text = body;
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;

            if (panel) panel.SetActive(false);

            // lock lại + bật movement
            if (mouseLook) mouseLook.LockCursor(true);
            if (playerMovement) playerMovement.enabled = true;

            onTalk = null;
            onPlayMinigame = null;
        }

    }
}