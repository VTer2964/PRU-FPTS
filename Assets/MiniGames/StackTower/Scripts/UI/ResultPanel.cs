using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StackTower
{
    /// <summary>
    /// Displays win results: floors built, stars earned, perfect count.
    /// </summary>
    public class ResultPanel : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI floorText;
        [SerializeField] private TextMeshProUGUI perfectText;
        [SerializeField] private TextMeshProUGUI starCountText;

        [Header("Star Images (3 stars)")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Sprite starEmptySprite;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button menuButton;

        [Header("Animation")]
        [SerializeField] private float starRevealDelay = 0.4f;
        [SerializeField] private float starScalePunch = 1.4f;

        // ── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (retryButton != null)
                retryButton.onClick.AddListener(() => StackTowerGameManager.Instance?.RestartLevel());

            if (menuButton != null)
                menuButton.onClick.AddListener(() => StackTowerGameManager.Instance?.ReturnToMenu());

            // Next level: advance to the next level index if possible
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(() =>
                {
                    var gm = StackTowerGameManager.Instance;
                    if (gm == null) return;
                    // currentLevel index + 1
                    int nextIdx = gm.LevelCount > 0 ? Mathf.Min(gm.LevelCount - 1, GetCurrentLevelIndex() + 1) : 0;
                    gm.StartGame(nextIdx);
                });
            }
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void Show(int floors, int stars, int perfects, int total)
        {
            gameObject.SetActive(true);

            if (titleText != null)
                titleText.text = "CHIẾN THẮNG!";

            if (floorText != null)
                floorText.text = $"Đã xây: {floors} tầng";

            if (perfectText != null)
            {
                float ratio = total > 0 ? (float)perfects / total * 100f : 0f;
                perfectText.text = $"Perfect: {perfects} / {total} ({ratio:F0}%)";
            }

            if (starCountText != null)
                starCountText.text = $"{stars} ⭐";

            // Disable next button if on last level
            if (nextLevelButton != null)
            {
                var gm = StackTowerGameManager.Instance;
                bool hasNext = gm != null && GetCurrentLevelIndex() < gm.LevelCount - 1;
                nextLevelButton.interactable = hasNext;
            }

            StartCoroutine(RevealStars(stars));
        }

        // ── Private ──────────────────────────────────────────────────────────

        private IEnumerator RevealStars(int starCount)
        {
            if (starImages == null) yield break;

            // Initially set all to empty
            foreach (var img in starImages)
            {
                if (img != null)
                {
                    img.sprite = starEmptySprite;
                    img.transform.localScale = Vector3.one;
                }
            }

            // Reveal stars one by one
            for (int i = 0; i < starImages.Length; i++)
            {
                yield return new WaitForSeconds(starRevealDelay);

                if (i < starCount && starImages[i] != null)
                {
                    starImages[i].sprite = starFilledSprite;
                    StartCoroutine(PunchScale(starImages[i].transform));
                }
            }
        }

        private IEnumerator PunchScale(Transform t)
        {
            float duration = 0.25f;
            float half = duration * 0.5f;
            float elapsed = 0f;

            while (elapsed < half)
            {
                float s = Mathf.Lerp(1f, starScalePunch, elapsed / half);
                t.localScale = Vector3.one * s;
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < half)
            {
                float s = Mathf.Lerp(starScalePunch, 1f, elapsed / half);
                t.localScale = Vector3.one * s;
                elapsed += Time.deltaTime;
                yield return null;
            }

            t.localScale = Vector3.one;
        }

        private int GetCurrentLevelIndex()
        {
            var gm = StackTowerGameManager.Instance;
            if (gm == null) return 0;
            for (int i = 0; i < gm.LevelCount; i++)
            {
                if (gm.GetLevelData(i) == gm.CurrentLevel)
                    return i;
            }
            return 0;
        }
    }
}
