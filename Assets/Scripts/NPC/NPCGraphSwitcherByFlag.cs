using UnityEngine;
using FPTSim.Core;
using FPTSim.Dialogue;

namespace FPTSim.NPC
{
    public class NPCGraphSwitcherByFlag : MonoBehaviour
    {
        [SerializeField] private string requiredFlag = "A_DONE";
        [SerializeField] private NPCDialogueInteractable_Graph npc;
        [SerializeField] private DialogueGraphSO lockedGraph;
        [SerializeField] private DialogueGraphSO unlockedGraph;

        private void Awake()
        {
            if (npc == null) npc = GetComponent<NPCDialogueInteractable_Graph>();
        }

        private void Update()
        {
            if (npc == null || GameManager.I == null) return;

            bool ok = GameManager.I.HasFlag(requiredFlag);
            npc.SetGraph(ok ? unlockedGraph : lockedGraph);
        }
    }
}