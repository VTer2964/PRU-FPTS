using UnityEngine;

namespace FPTSim.Dialogue
{
    public enum DialogueSpeakerType { NPC, Player, System }

    public enum DialogueActionType
    {
        None,
        StartMinigameScene,
        BuyTimeWithBronze,
        BuyTimeWithSilver,
        BuyTimeWithGold,
        SubmitToWin,
        CloseDialogue,
        SetFlag,
        SetCamera // ✅ thêm action này
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

        [Header("Choices (Nếu có choices thì bắt buộc chọn)")]
        public DialogueChoice[] choices;

        [Header("Auto Next (Không choices, autoAdvance=true => Space/Click qua nextAuto)")]
        public bool autoAdvance = false;
        public DialogueNodeSO nextAuto;

        [Header("Action (tùy chọn)")]
        public bool triggerAction = false;
        public DialogueActionType actionType = DialogueActionType.None;
        public string actionParam; // SetCamera: cameraKey | StartMinigameScene: sceneName | SetFlag: flagName
        public int actionValue;
    }
}