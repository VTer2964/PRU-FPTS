using UnityEngine;
using TMPro;
using FPTSim.Core;

namespace FPTSim.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Top Info")]
        [SerializeField] private TMP_Text timeText;        // "17:59"

        [Header("Medals Count (numbers next to icons)")]
        [SerializeField] private TMP_Text goldCountText;
        [SerializeField] private TMP_Text silverCountText;
        [SerializeField] private TMP_Text bronzeCountText;

        private void OnEnable()
        {
            if (GameManager.I != null)
                GameManager.I.OnStateChanged += Refresh;

            Refresh();
        }

        private void OnDisable()
        {
            if (GameManager.I != null)
                GameManager.I.OnStateChanged -= Refresh;
        }

        private void Refresh()
        {
            if (GameManager.I == null) return;

            var s = GameManager.I.State;
            var c = GameManager.I.Config;

            // Timer còn lại của run
            if (timeText)
            {
                int totalSeconds = Mathf.CeilToInt(s.timeLeft);
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                timeText.text = $"{minutes:00}:{seconds:00}";
            }

            // Medal hiện có
            if (goldCountText) goldCountText.text = s.gold.ToString();
            if (silverCountText) silverCountText.text = s.silver.ToString();
            if (bronzeCountText) bronzeCountText.text = s.bronze.ToString();
        }
    }
}