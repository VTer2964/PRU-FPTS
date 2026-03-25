using UnityEngine;
using FPTSim.Core;

namespace FPTSim.NPC
{
    public class QuestGiver : MonoBehaviour, IInteractable
    {
        public string npcName = "NPC";
        public QuestSO questToAssign;

        public string GetPromptText()
        {
            if (QuestManager.Instance == null) return "";

            // Nếu đã nhận nhiệm vụ này rồi HOẶC đã làm xong rồi thì ẩn prompt đi
            if (questToAssign != null && (QuestManager.Instance.HasQuest(questToAssign) || QuestManager.Instance.IsQuestCompleted(questToAssign)))
                return ""; 
                
            return $"Nói chuyện với {npcName}";
        }

        public void Interact()
        {
            if (QuestManager.Instance == null) return;

            // Chỉ cho nhận nếu chưa có trong danh sách nhận VÀ chưa hoàn thành
            if (questToAssign != null && !QuestManager.Instance.HasQuest(questToAssign) && !QuestManager.Instance.IsQuestCompleted(questToAssign))
            {
                QuestManager.Instance.AddQuest(questToAssign);
                Debug.Log($"Đã nhận thêm nhiệm vụ: {questToAssign.questName}");
            }
        }
    }
}