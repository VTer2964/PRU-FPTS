using UnityEngine;

namespace FPTSim.Core
{
    [CreateAssetMenu(fileName = "New Quest", menuName = "FPTSim/Quest")]
    public class QuestSO : ScriptableObject
    {
        public string questName; // Tên nhiệm vụ
        
        [TextArea(3, 10)]
        public string description; // Mô tả chi tiết nhiệm vụ
    }
}