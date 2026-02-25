using System;
using UnityEngine;

namespace FPTSim.Dialogue
{
    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public DialogueNodeSO next;

        public bool requireCondition = false;
        public string conditionKey;
        public int conditionValue;
    }
}