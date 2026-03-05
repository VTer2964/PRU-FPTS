using UnityEngine;
using FPTSim.Core;

namespace FPTSim.Dialogue
{
    // Tên class đã đổi thành ConversationZone để không bị trùng
    [RequireComponent(typeof(Collider))]
    public class ConversationZone : MonoBehaviour
    {
        [Header("Dialogue Config")]
        [Tooltip("Kéo file Graph hội thoại vào đây")]
        [SerializeField] private DialogueGraphSO graph;

        [Tooltip("Để trống, game tự tìm")]
        [SerializeField] private DialogueRunner runner;

        [Header("Conditions")]
        [Tooltip("Cờ bắt buộc phải có mới chạy (VD: QUEST_01)")]
        [SerializeField] private string requiredFlag;

        [Tooltip("Cờ đánh dấu đã xong (VD: TALKED_NPC_A)")]
        [SerializeField] private string completedFlag;

        private void Start()
        {
            if (runner == null)
            {
                runner = FindFirstObjectByType<DialogueRunner>();
            }

            // Tự động bật IsTrigger cho Collider
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Chỉ Player mới kích hoạt
            if (!other.CompareTag("Player")) return;

            if (graph == null) return;

            // Tìm lại runner lần nữa cho chắc
            if (runner == null) runner = FindFirstObjectByType<DialogueRunner>();
            if (runner == null) return;

            // Nếu đang nói chuyện thì không kích hoạt chồng
            if (runner.IsRunning) return;

            // Kiểm tra điều kiện cờ (Flags)
            if (GameManager.I != null)
            {
                // Nếu đã xong (có cờ completed) -> Dừng
                if (!string.IsNullOrWhiteSpace(completedFlag) && GameManager.I.HasFlag(completedFlag.Trim())) return;

                // Nếu chưa đủ điều kiện (thiếu cờ required) -> Dừng
                if (!string.IsNullOrWhiteSpace(requiredFlag) && !GameManager.I.HasFlag(requiredFlag.Trim())) return;
            }

            // Chạy hội thoại
            runner.StartDialogue(graph);
        }

        // Vẽ khối màu xanh để dễ nhìn trong Editor
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.4f); // Màu xanh dương nhạt
            var box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else
            {
                Gizmos.DrawCube(transform.position, transform.localScale);
            }
        }
    }
}