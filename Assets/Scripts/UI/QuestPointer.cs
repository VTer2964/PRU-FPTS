using UnityEngine;
using FPTSim.Core;
using FPTSim.NPC;
using System.Collections.Generic;

namespace FPTSim.UI
{
    public class QuestPointer : MonoBehaviour
    {
        [Header("Pointer Settings")]
        [SerializeField] private GameObject arrowPrefab; 
        [SerializeField] private float yOffset = -0.9f; 
        [SerializeField] private float distanceFromPlayer = 1.2f; 
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private Vector3 arrowScale = Vector3.one;

        [Header("Model Fix")]
        [Tooltip("Xoay X để ép nằm bẹp. Xoay Y để chỉnh mũi nhọn chỉ đúng hướng.")]
        [SerializeField] private Vector3 modelRotationFix = new Vector3(90, 0, 0); 

        private Transform player;
        private Transform targetNPC;
        private GameObject currentArrow;

        private void Start()
        {
            FindPlayer();
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestListChanged += RefreshTarget;
                RefreshTarget();
            }
        }

        private void FindPlayer()
        {
            var pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj) player = pObj.transform;
        }

        private void OnDestroy()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestListChanged -= RefreshTarget;
        }

        private void RefreshTarget()
        {
            targetNPC = null;

            if (QuestManager.Instance == null || QuestManager.Instance.activeQuests == null || QuestManager.Instance.activeQuests.Count == 0)
            {
                if (currentArrow) currentArrow.SetActive(false);
                return;
            }

            // Lấy tất cả NPC nhận nhiệm vụ trong cảnh
            QuestCompleter[] completers = Object.FindObjectsByType<QuestCompleter>(FindObjectsSortMode.None);
            
            // Tìm NPC tương ứng với nhiệm vụ ĐẦU TIÊN (nhiệm vụ số 1) mà người chơi đang nhận
            // Ta quét từ đầu danh sách (index 0) đi lên
            for (int i = 0; i < QuestManager.Instance.activeQuests.Count; i++)
            {
                QuestSO questToCheck = QuestManager.Instance.activeQuests[i];
                foreach (var c in completers)
                {
                    if (c.questToComplete == questToCheck)
                    {
                        targetNPC = c.transform;
                        break;
                    }
                }
                if (targetNPC != null) break; // Đã tìm thấy điểm trả quest của nhiệm vụ số 1 -> dừng
            }

            if (targetNPC != null)
            {
                if (currentArrow == null && arrowPrefab != null)
                {
                    currentArrow = Instantiate(arrowPrefab, transform);
                }
                
                if (currentArrow) 
                {
                    currentArrow.SetActive(true);
                    currentArrow.transform.localScale = arrowScale;
                }
            }
            else
            {
                if (currentArrow) currentArrow.SetActive(false);
            }
        }

        private void Update()
        {
            if (player == null) FindPlayer();
            if (player == null || targetNPC == null || currentArrow == null || !currentArrow.activeSelf)
                return;

            Vector3 dir = targetNPC.position - player.position;
            dir.y = 0;

            if (dir.sqrMagnitude > 0.01f)
            {
                Vector3 normalizedDir = dir.normalized;
                Vector3 targetPosition = player.position + (Vector3.up * yOffset) + (normalizedDir * distanceFromPlayer);
                currentArrow.transform.position = targetPosition;

                Quaternion lookRot = Quaternion.LookRotation(normalizedDir, Vector3.up);
                currentArrow.transform.rotation = Quaternion.Slerp(currentArrow.transform.rotation, lookRot * Quaternion.Euler(modelRotationFix), Time.deltaTime * rotationSpeed);
            }
        }
    }
}