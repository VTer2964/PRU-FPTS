using UnityEngine;

namespace FPTSim.Dialogue
{
    public enum DialogueSpeakerType { NPC, Player, System }

    [CreateAssetMenu(menuName = "FPT Sim/Dialogue/Node", fileName = "DialogueNode")]
    public class DialogueNodeSO : ScriptableObject
    {
        public DialogueSpeakerType speakerType = DialogueSpeakerType.NPC;
        public string speakerName = "NPC";

        [TextArea(3, 8)]
        public string text;

        [Header("Choices (hiện nút)")]
        public DialogueChoice[] choices;

        [Header("Action (tùy chọn)")]
        public bool triggerAction = false;

        public DialogueActionType actionType = DialogueActionType.None;
        public string actionParam; // ví dụ: tên scene minigame, hoặc "bronze"
        public int actionValue;    // ví dụ: số giây cộng thêm...
    }

    public enum DialogueActionType
    {
        None,
        StartMinigameScene,     // actionParam = sceneName
        BuyTimeWithBronze,      // dùng hệ của bạn
        BuyTimeWithSilver,
        BuyTimeWithGold,
        SubmitToWin,            // nộp huy chương thắng
        CloseDialogue           // đóng hộp thoại
    }
}