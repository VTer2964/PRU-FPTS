using UnityEngine;
using FPTSim.Core;
using FPTSim.Dialogue;

namespace FPTSim.NPC
{
    public class NPCDialogueInteractable_Graph : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "NPC";
        [SerializeField] private DialogueGraphSO dialogueGraph;
        [SerializeField] private DialogueRunner runner;

        public string GetPromptText() => $"Nói chuyện với {npcName}";

        public void Interact()
        {
            if (dialogueGraph == null || runner == null) return;
            runner.StartDialogue(dialogueGraph);
        }
    }
}