using UnityEngine;
using FPTSim.Core;

namespace FPTSim.World
{
    public class EndDayGateInteractable : MonoBehaviour, IInteractable
    {
        [Header("Prompt")]
        [SerializeField] private string prompt = "Về ký túc xá / Kết thúc ngày";

        [Header("Range (optional)")]
        [SerializeField] private bool requireInsideZone = true;

        private bool playerInZone;

        public string GetPromptText()
        {
            if (requireInsideZone && !playerInZone) return ""; // không hiện prompt nếu chưa đứng trong zone
            return prompt;
        }

        public void Interact()
        {
            if (requireInsideZone && !playerInZone) return;
            if (GameManager.I == null) return;

            GameManager.I.EndDay();
        }

        // Trigger zone detection
        private void OnTriggerEnter(Collider other)
        {
            if (!requireInsideZone) return;
            if (other.CompareTag("Player")) playerInZone = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!requireInsideZone) return;
            if (other.CompareTag("Player")) playerInZone = false;
        }
    }
}