using UnityEngine;
using System;
using System.Collections.Generic;

namespace FPTSim.Core
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        // Danh sách các nhiệm vụ đang nhận
        public List<QuestSO> activeQuests = new List<QuestSO>();
        // Danh sách các nhiệm vụ đã hoàn thành
        public List<QuestSO> completedQuests = new List<QuestSO>();

        // Sự kiện gọi khi danh sách nhiệm vụ thay đổi (thêm/bớt)
        public event Action OnQuestListChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); 
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            OnQuestListChanged?.Invoke();
        }

        // Kiểm tra xem người chơi đã nhận nhiệm vụ này chưa
        public bool HasQuest(QuestSO quest)
        {
            return activeQuests != null && activeQuests.Contains(quest);
        }

        // Kiểm tra xem nhiệm vụ đã hoàn thành chưa
        public bool IsQuestCompleted(QuestSO quest)
        {
            return completedQuests != null && completedQuests.Contains(quest);
        }

        // Thêm nhiệm vụ mới vào danh sách
        public void AddQuest(QuestSO newQuest)
        {
            if (newQuest != null && !HasQuest(newQuest) && !IsQuestCompleted(newQuest))
            {
                activeQuests.Add(newQuest);
                OnQuestListChanged?.Invoke();
            }
        }

        // Hoàn thành và xóa nhiệm vụ khỏi danh sách
        public void CompleteQuest(QuestSO questToComplete)
        {
            if (HasQuest(questToComplete))
            {
                activeQuests.Remove(questToComplete);
                if (!completedQuests.Contains(questToComplete))
                {
                    completedQuests.Add(questToComplete);
                }
                OnQuestListChanged?.Invoke();
            }
        }
    }
}