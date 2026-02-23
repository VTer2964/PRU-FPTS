using UnityEngine;
using UnityEngine.SceneManagement;
using FPTSim.Core;
using FPTSim.UI;

namespace FPTSim.NPC
{
    // NPC này dùng ProfileSO để có thoại & minigame riêng
    public class NPCDialogueInteractable_Data : MonoBehaviour, IInteractable
    {
        [Header("Data")]
        [SerializeField] private NPCProfileSO profile;

        [Header("UI")]
        [SerializeField] private DialogueUI dialogueUI;

        private int talkIndex = 0;

        public string GetPromptText()
        {
            if (profile == null) return "Nói chuyện";

            // Ví dụ hiển thị: "Nói chuyện với Mr. Sus"
            if (string.IsNullOrWhiteSpace(profile.interactPrompt))
                return $"Nói chuyện với {profile.displayName}";

            return $"{profile.interactPrompt} {profile.displayName}";
        }

        public void Interact()
        {
            if (profile == null)
            {
                Debug.LogError($"{name}: Missing NPCProfileSO!");
                return;
            }

            if (dialogueUI == null)
            {
                Debug.LogError($"{name}: Missing DialogueUI reference!");
                return;
            }

            // Nếu hết giờ ngày (timer = 0) thì không cho chơi nữa (tuỳ bạn)
            if (GameManager.I != null && !GameManager.I.CanPlayMinigame())
            {
                dialogueUI.Open(
                    speaker: profile.displayName,
                    body: "Hôm nay hết giờ rồi. Mai quay lại nhé!",
                    talkAction: OnTalk,
                    playMinigameAction: OnPlayMinigame // bấm cũng sẽ không vào minigame (mình chặn bên dưới)
                );
                return;
            }

            dialogueUI.Open(
                speaker: profile.displayName,
                body: profile.firstLine,
                talkAction: OnTalk,
                playMinigameAction: OnPlayMinigame
            );
        }

        private void OnTalk()
        {
            if (profile == null || dialogueUI == null) return;

            if (profile.talkLines == null || profile.talkLines.Length == 0)
            {
                dialogueUI.SetBody("...");
                return;
            }

            dialogueUI.SetBody(profile.talkLines[talkIndex]);
            talkIndex = (talkIndex + 1) % profile.talkLines.Length;
        }

        private void OnPlayMinigame()
        {
            if (profile == null || dialogueUI == null) return;

            // Nếu hết giờ / không cho chơi -> chỉ đổi text
            if (GameManager.I != null && !GameManager.I.CanPlayMinigame())
            {
                dialogueUI.SetBody("Hết giờ trong ngày rồi, không thể chơi minigame nữa!");
                return;
            }

            dialogueUI.Close();

            if (string.IsNullOrWhiteSpace(profile.minigameSceneName))
            {
                Debug.LogError($"{name}: profile.minigameSceneName is empty!");
                return;
            }

            SceneManager.LoadScene(profile.minigameSceneName);
        }
    }
}