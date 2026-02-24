using System;
using UnityEngine;

namespace FPTSim.Dialogue
{
    [Serializable]
    public class DialogueChoice
    {
        public string text;

        [Tooltip("Node tiếp theo. Nếu null => kết thúc hội thoại.")]
        public DialogueNodeSO next;

        // Tương lai: điều kiện (ví dụ cần medal), ẩn/hiện choice
        public bool requireCondition = false;
        public string conditionKey;
        public int conditionValue;
    }
}