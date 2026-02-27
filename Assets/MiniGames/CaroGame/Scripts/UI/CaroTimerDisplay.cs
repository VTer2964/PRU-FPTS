using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CaroGame
{
    public class CaroTimerDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerBar;
        [SerializeField] private CaroTimerSystem timerSystem;

        [Header("Colors")]
        [SerializeField] private Color safeColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0f);
        [SerializeField] private Color dangerColor = new Color(0.9f, 0.2f, 0.2f);

        [Header("Thresholds")]
        [SerializeField] private float warningThreshold = 0.5f;
        [SerializeField] private float dangerThreshold = 0.17f; // ~10 seconds at 60s

        private bool isPulsing;
        private float pulseTimer;

        private void OnEnable()
        {
            if (timerSystem != null)
                timerSystem.OnTimerUpdate += UpdateDisplay;
        }

        private void OnDisable()
        {
            if (timerSystem != null)
                timerSystem.OnTimerUpdate -= UpdateDisplay;
        }

        private void UpdateDisplay(float remaining, float total)
        {
            // Update text
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(remaining / 60f);
                int seconds = Mathf.FloorToInt(remaining % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }

            // Update bar
            float percentage = Mathf.Clamp01(remaining / total);
            if (timerBar != null)
            {
                timerBar.fillAmount = percentage;
                timerBar.color = GetTimerColor(percentage);
            }

            // Pulse effect when danger
            if (percentage <= dangerThreshold)
            {
                pulseTimer += Time.deltaTime * 4f;
                float alpha = 0.7f + 0.3f * Mathf.Sin(pulseTimer * Mathf.PI);
                if (timerText != null)
                {
                    Color textColor = dangerColor;
                    textColor.a = alpha;
                    timerText.color = textColor;
                }
            }
            else
            {
                pulseTimer = 0f;
                if (timerText != null)
                    timerText.color = Color.white;
            }
        }

        private Color GetTimerColor(float percentage)
        {
            if (percentage <= dangerThreshold)
                return dangerColor;
            if (percentage <= warningThreshold)
                return Color.Lerp(dangerColor, warningColor, (percentage - dangerThreshold) / (warningThreshold - dangerThreshold));
            return Color.Lerp(warningColor, safeColor, (percentage - warningThreshold) / (1f - warningThreshold));
        }

        public void ResetDisplay()
        {
            pulseTimer = 0f;
            if (timerText != null)
            {
                timerText.text = "01:00";
                timerText.color = Color.white;
            }
            if (timerBar != null)
            {
                timerBar.fillAmount = 1f;
                timerBar.color = safeColor;
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
