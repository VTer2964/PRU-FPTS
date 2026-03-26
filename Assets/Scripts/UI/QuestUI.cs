using UnityEngine;
using TMPro;
using FPTSim.Core;

namespace FPTSim.UI
{
    public class QuestUI : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI questTitleText;
        public TextMeshProUGUI questDescriptionText;

        [Header("Settings")]
        public string noQuestText = "Không có nhiệm vụ";

        private void Start()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestListChanged += UpdateUI;
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestListChanged -= UpdateUI;
            }
        }

        private void UpdateUI()
        {
            if (QuestManager.Instance == null || QuestManager.Instance.activeQuests == null || QuestManager.Instance.activeQuests.Count == 0)
            {
                // KHÔNG có nhiệm vụ nào
                if (questTitleText != null) questTitleText.text = noQuestText;
                if (questDescriptionText != null) questDescriptionText.text = "";
            }
            else
            {
                // CÓ 1 HOẶC NHIỀU nhiệm vụ -> Nối chuỗi tất cả lại để hiển thị
                if (questTitleText != null) 
                    questTitleText.text = $"Nhiệm vụ ({QuestManager.Instance.activeQuests.Count})";

                if (questDescriptionText != null)
                {
                    string fullText = "";
                    for (int i = 0; i < QuestManager.Instance.activeQuests.Count; i++)
                    {
                        var q = QuestManager.Instance.activeQuests[i];
                        // Sử dụng (i + 1) để tạo số thứ tự bắt đầu từ 1
                        fullText += $"<b>{i + 1}. {q.questName}</b>\n{q.description}";
                        
                        // Thêm dấu xuống dòng nếu không phải nhiệm vụ cuối cùng
                        if (i < QuestManager.Instance.activeQuests.Count - 1)
                            fullText += "\n\n";
                    }
                    questDescriptionText.text = fullText;
                }
            }
        }
    }
}