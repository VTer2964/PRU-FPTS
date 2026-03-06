using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StackTower
{
    /// <summary>
    /// Plays visual feedback when the player achieves a perfect placement.
    /// Attach to a persistent canvas object.
    /// </summary>
    public class PerfectEffect : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI perfectText;
        [SerializeField] private Image flashOverlay;

        [Header("Settings")]
        [SerializeField] private StackTowerSettings settings;
        [SerializeField] private float textFloatHeight = 80f;
        [SerializeField] private float textDuration = 1f;
        [SerializeField] private AnimationCurve textAlphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Combo Colors")]
        [SerializeField] private Color combo1Color = new Color(1f, 1f, 0.6f);
        [SerializeField] private Color combo5Color = new Color(1f, 0.6f, 0.2f);
        [SerializeField] private Color combo10Color = new Color(1f, 0.3f, 0.8f);

        // ── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (settings == null)
                settings = Resources.Load<StackTowerSettings>("StackTower/StackTowerSettings");

            if (perfectText != null)
                perfectText.gameObject.SetActive(false);
            if (flashOverlay != null)
                flashOverlay.gameObject.SetActive(false);
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>Play perfect effects at the given world position with current combo count.</summary>
        public void PlayPerfect(Vector3 worldPos, int combo)
        {
            if (perfectText != null)
                StartCoroutine(AnimatePerfectText(worldPos, combo));

            if (flashOverlay != null)
                StartCoroutine(FlashScreen());
        }

        // ── Private ──────────────────────────────────────────────────────────

        private IEnumerator AnimatePerfectText(Vector3 worldPos, int combo)
        {
            if (perfectText == null) yield break;

            // Position the text above the block in screen space
            Camera cam = Camera.main;
            Vector3 screenPos = cam != null
                ? cam.WorldToScreenPoint(worldPos)
                : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

            perfectText.gameObject.SetActive(true);
            perfectText.text = combo >= 10 ? $"PERFECT x{combo}!!" :
                               combo >= 5  ? $"PERFECT x{combo}!" :
                                             "PERFECT!";
            perfectText.color = GetComboColor(combo);

            var rt = perfectText.GetComponent<RectTransform>();
            Vector3 startPos = screenPos;
            Vector3 endPos   = screenPos + Vector3.up * textFloatHeight;

            float elapsed = 0f;
            while (elapsed < textDuration)
            {
                float t = elapsed / textDuration;
                if (rt != null) rt.position = Vector3.Lerp(startPos, endPos, t);

                Color c = perfectText.color;
                c.a = textAlphaCurve.Evaluate(t);
                perfectText.color = c;

                elapsed += Time.deltaTime;
                yield return null;
            }

            perfectText.gameObject.SetActive(false);
        }

        private IEnumerator FlashScreen()
        {
            if (flashOverlay == null) yield break;

            float duration = settings != null ? settings.perfectFlashDuration : 0.3f;
            Color flashColor = settings != null ? settings.perfectFlashColor : new Color(1f, 1f, 0.6f, 0.5f);

            flashOverlay.gameObject.SetActive(true);
            flashOverlay.color = flashColor;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                Color c = flashColor;
                c.a = Mathf.Lerp(flashColor.a, 0f, t);
                flashOverlay.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }

            flashOverlay.gameObject.SetActive(false);
        }

        private Color GetComboColor(int combo)
        {
            if (combo >= 10) return combo10Color;
            if (combo >= 5)  return combo5Color;
            return combo1Color;
        }
    }
}
