using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
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
        [SerializeField] private MonoBehaviour playerMovement;  // kéo script movement bạn đang dùng (FirstPersonController)
        [SerializeField] private Interactor playerInteractor;   // ✅ đúng type Interactor

        private System.Action onTalk;
        private System.Action onPlayMinigame;

        private bool isOpen;
        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (talkButton) talkButton.onClick.AddListener(() => onTalk?.Invoke());
            if (playMinigameButton) playMinigameButton.onClick.AddListener(() => onPlayMinigame?.Invoke());
            if (exitButton) exitButton.onClick.AddListener(Close);

            if (panel) panel.SetActive(false);
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

            // đảm bảo panel nằm trên cùng (tránh bị UI khác che)
            panel.transform.SetAsLastSibling();

            // clear selection để tránh input UI frame đầu
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            // ✅ khóa gameplay + hiện chuột
            if (mouseLook) mouseLook.LockCursor(false);
            if (playerMovement) playerMovement.enabled = false;
            if (playerInteractor) playerInteractor.enabled = false;
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

            // ✅ bật lại gameplay + khóa chuột
            if (mouseLook) mouseLook.LockCursor(true);
            if (playerMovement) playerMovement.enabled = true;
            if (playerInteractor) playerInteractor.enabled = true;

            onTalk = null;
            onPlayMinigame = null;
        }
     
    }
}