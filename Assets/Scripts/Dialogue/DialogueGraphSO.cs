using UnityEngine;

namespace FPTSim.Dialogue
{
    [CreateAssetMenu(menuName = "FPT Sim/Dialogue/Graph", fileName = "DialogueGraph")]
    public class DialogueGraphSO : ScriptableObject
    {
        public string graphId = "MrSus";
        public DialogueNodeSO entryNode;
    }
}