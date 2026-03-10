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

        [Header("Face to face / stop & resume")]
        [SerializeField] private NPCBrain brain;               // gắn NPCBrain lên NPC
        [SerializeField] private Transform player;             // nếu null sẽ tìm theo tag Player
        [SerializeField] private string playerTag = "Player";  // tag của player

        public string GetPromptText() => $"Nói chuyện với {npcName}";

        public void SetGraph(DialogueGraphSO g) => dialogueGraph = g;

        private void Awake()
        {
            if (!brain) brain = GetComponent<NPCBrain>();

            if (!runner)
                runner = FindFirstObjectByType<DialogueRunner>();

            if (!player)
            {
                var p = GameObject.FindGameObjectWithTag(playerTag);
                if (p) player = p.transform;
            }
        }

        public void Interact()
        {
            if (dialogueGraph == null || runner == null) return;

            // Nếu NPC đang talk rồi thì không mở chồng
            if (brain != null && brain.IsTalking) return;

            // 1) NPC stop + face to player
            if (brain != null && player != null)
                brain.EnterTalk(player);

            // 2) Khi dialogue dừng -> resume NPC
            void ResumeNpc()
            {
                if (brain != null) brain.ExitTalk();
                runner.OnDialogueStopped -= ResumeNpc;
            }

            runner.OnDialogueStopped += ResumeNpc;

            // 3) Start dialogue
            runner.StartDialogue(dialogueGraph);
        }
    }
}