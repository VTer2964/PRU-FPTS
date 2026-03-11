using UnityEngine;
using FPTSim.Core;
using FPTSim.Dialogue;

namespace FPTSim.NPC
{
    public class NPCGraphSwitcherByFlags : MonoBehaviour
    {
        [System.Serializable]
        public class GraphRule
        {
            [Header("Nếu có flag này thì dùng graph bên dưới")]
            public string requiredFlag;

            [Header("Graph sẽ dùng khi flag đúng")]
            public DialogueGraphSO graph;
        }

        [Header("NPC target")]
        [SerializeField] private NPCDialogueInteractable_Graph npc;

        [Header("Graph mặc định khi chưa match rule nào")]
        [SerializeField] private DialogueGraphSO defaultGraph;

        [Header("Rules (ưu tiên từ trên xuống dưới)")]
        [SerializeField] private GraphRule[] rules;

        [Header("Auto update")]
        [SerializeField] private bool updateEveryFrame = true;

        private DialogueGraphSO lastApplied;

        private void Awake()
        {
            if (npc == null)
                npc = GetComponent<NPCDialogueInteractable_Graph>();
        }

        private void Start()
        {
            RefreshGraph();
        }

        private void Update()
        {
            if (updateEveryFrame)
                RefreshGraph();
        }

        public void RefreshGraph()
        {
            if (npc == null) return;

            DialogueGraphSO target = ResolveGraph();

            if (target == null) return;
            if (target == lastApplied) return;

            npc.SetGraph(target);
            lastApplied = target;
        }

        private DialogueGraphSO ResolveGraph()
        {
            if (GameManager.I == null)
                return defaultGraph;

            if (rules != null)
            {
                for (int i = 0; i < rules.Length; i++)
                {
                    var rule = rules[i];
                    if (rule == null) continue;
                    if (string.IsNullOrWhiteSpace(rule.requiredFlag)) continue;
                    if (rule.graph == null) continue;

                    if (GameManager.I.HasFlag(rule.requiredFlag))
                        return rule.graph;
                }
            }

            return defaultGraph;
        }
    }
}