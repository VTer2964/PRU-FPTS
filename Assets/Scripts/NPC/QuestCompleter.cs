using UnityEngine;
using UnityEngine.Events;
using FPTSim.Core;

namespace FPTSim.NPC
{
    public class QuestCompleter : MonoBehaviour, IInteractable
    {
        public string npcName = "NPC";
        public QuestSO questToComplete;
        public UnityEvent onQuestCompleted;

        public string GetPromptText()
        {
            if (QuestManager.Instance == null) return "";

            // Chỉ hiện thông báo nhận nhiệm vụ nếu NGƯỜI CHƠI ĐANG SỞ HỮU nhiệm vụ này trong danh sách
            if (questToComplete != null && QuestManager.Instance.HasQuest(questToComplete))
            {
                return $"Nói chuyện với {npcName}";
            }
                
            return ""; 
        }

        public void Interact()
        {
            if (QuestManager.Instance == null) return;

            // Nếu người chơi đang giữ nhiệm vụ này trong danh sách
            if (questToComplete != null && QuestManager.Instance.HasQuest(questToComplete))
            {
                // Trả nhiệm vụ (sẽ tự động xóa khỏi danh sách mà ko ảnh hưởng các nhiệm vụ khác)
                QuestManager.Instance.CompleteQuest(questToComplete);
                onQuestCompleted?.Invoke();
                Debug.Log($"Đã hoàn thành: {questToComplete.questName}");
            }
        }
    }
}