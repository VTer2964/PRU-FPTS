using UnityEngine;

namespace FPTSim.Dialogue
{
    public enum DialogueSpeakerType { NPC, Player, System }

    public enum DialogueActionType
    {
        None,
        StartMinigameScene,     // actionParam = sceneName
        BuyTimeWithBronze,      // dùng hệ của bạn
        BuyTimeWithSilver,
        BuyTimeWithGold,
        SubmitToWin,            // nộp huy chương thắng
        CloseDialogue,           // đóng hộp thoại
        SetFlag
    }

    [CreateAssetMenu(menuName = "FPT Sim/Dialogue/Node", fileName = "DialogueNode")]
    public class DialogueNodeSO : ScriptableObject
    {
        [Header("Speaker")]
        public DialogueSpeakerType speakerType = DialogueSpeakerType.NPC;
        public string speakerName = "NPC";

        [Header("Text")]
        [TextArea(3, 8)]
        public string text;

        [Header("Choices (Nếu có choices thì bắt buộc chọn, không click/Space next)")]
        public DialogueChoice[] choices;

        [Header("Auto Next (Dùng cho cutscene: nếu KHÔNG có choices, click/Space để qua nextAuto)")]
        public bool autoAdvance = false;
        public DialogueNodeSO nextAuto;

        [Header("Action (tùy chọn)")]
        public bool triggerAction = false;
        public DialogueActionType actionType = DialogueActionType.None;
        public string actionParam; // ví dụ: tên scene minigame, hoặc param khác
        public int actionValue;    // ví dụ: số giây cộng thêm...
    }
}