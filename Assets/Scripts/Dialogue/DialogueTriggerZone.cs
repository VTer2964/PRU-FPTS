using UnityEngine;
using FPTSim.Core;

namespace FPTSim.Dialogue
{
    [RequireComponent(typeof(Collider))]
    public class DialogueTriggerZone : MonoBehaviour
    {
        [Header("Dialogue")]
        [SerializeField] private DialogueRunner runner;
        [SerializeField] private DialogueGraphSO graph;

        [Header("Gate (optional)")]
        [Tooltip("Chỉ cho trigger nếu có flag này. Ví dụ: A_DONE")]
        [SerializeField] private string requiredFlag = "";

        [Header("Complete Flag (important)")]
        [Tooltip("Nếu flag này đã tồn tại => zone sẽ không trigger nữa. Ví dụ: ZONE_A1_DONE")]
        [SerializeField] private string completedFlag = "ZONE_A1_DONE";

        private void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (GameManager.I == null) return;

            // 1) Nếu đã hoàn thành => không trigger nữa
            if (!string.IsNullOrWhiteSpace(completedFlag))
            {
                var done = completedFlag.Trim();
                if (GameManager.I.HasFlag(done)) return;
            }

            // 2) Nếu cần điều kiện mở khóa => check requiredFlag
            if (!string.IsNullOrWhiteSpace(requiredFlag))
            {
                var req = requiredFlag.Trim();
                if (!GameManager.I.HasFlag(req)) return;
            }

            if (runner == null) runner = FindFirstObjectByType<DialogueRunner>();
            if (runner == null || graph == null) return;

            // Nếu đang chạy dialogue rồi thì không mở thêm (tránh chồng)
            if (runner.IsRunning) return;

            runner.StartDialogue(graph);
        }
    }
}