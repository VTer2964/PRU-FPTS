using UnityEngine;

namespace FPTSim.NPC
{
    [CreateAssetMenu(menuName = "FPT Sim/NPC Profile", fileName = "NPCProfile")]
    public class NPCProfileSO : ScriptableObject
    {
        [Header("Identity")]
        public string npcId = "MrSus";
        public string displayName = "Mr. Sus";

        [Header("Prompt shown when aiming")]
        public string interactPrompt = "Nói chuyện";

        [Header("Dialogue")]
        [TextArea] public string firstLine = "Chào bạn. Bạn muốn làm gì?";
        [TextArea] public string[] talkLines;

        [Header("Minigame")]
        [Tooltip("Tên scene minigame mà NPC này dẫn tới. Ví dụ: Minigame_FCode")]
        public string minigameSceneName = "Minigame_FCode";
    }
}