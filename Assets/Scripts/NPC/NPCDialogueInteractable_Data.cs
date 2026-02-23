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