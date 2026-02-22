using UnityEngine;
using UnityEngine.SceneManagement;
using FPTSim.Core;
using FPTSim.UI;

namespace FPTSim.NPC
{
    public class NPCDialogueInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "Mr. Sus";
        [TextArea][SerializeField] private string firstLine = "Chào bạn. Bạn muốn làm gì?";
        [SerializeField] private DialogueUI dialogueUI;

        [Header("Minigame fallback")]
        [SerializeField] private bool openMinigamePanel = true;
        [SerializeField] private string directMinigameScene = "Minigame_FCode";

        public string GetPromptText() => $"Nói chuyện với {npcName}";

        public void Interact()
        {
            if (dialogueUI == null)
            {
                Debug.LogError("NPCDialogueInteractable: dialogueUI is missing!");
                return;
            }

            dialogueUI.Open(
                speaker: npcName,
                body: firstLine,
                talkAction: OnTalk,
                playMinigameAction: OnPlayMinigame
            );
        }

        private void OnTalk()
        {
            dialogueUI.SetBody("Hôm nay bạn muốn luyện gì? Chúc bạn đạt huy chương Vàng!");
        }

        private void OnPlayMinigame()
        {
            dialogueUI.Close();

            if (openMinigamePanel)
            {
               
                return;
            }

            SceneManager.LoadScene(directMinigameScene);
        }
    }
}