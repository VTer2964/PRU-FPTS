using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MemoryCardMatch
{
    public enum CardState
    {
        FaceDown,
        FaceUp,
        Matched
    }

    public class CardController : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [Tooltip("Image component displaying the card back sprite")]
        [SerializeField] private Image cardBackImage;

        [Tooltip("Image component displaying the card front sprite")]
        [SerializeField] private Image cardFrontImage;

        [Tooltip("Optional border/glow overlay shown when matched (should NOT cover the sprite)")]
        [SerializeField] private GameObject matchEffectObject;

        [Tooltip("Background image of the card (root Image), tinted green on match")]
        [SerializeField] private Image cardBackgroundImage;

        [Header("Settings")]
        [Tooltip("Duration of the flip animation half (full flip = 2x this)")]
        [SerializeField] private float flipHalfDuration = 0.15f;

        [Tooltip("Scale of the pulse on match effect")]
        [SerializeField] private float matchPulseScale = 1.15f;

        [Tooltip("Duration of one pulse cycle on match")]
        [SerializeField] private float matchPulseDuration = 0.15f;

        // Card data
        public CardData Data { get; private set; }
        public CardState State { get; private set; } = CardState.FaceDown;

        private bool isAnimating = false;

        // -----------------------------------------------------------------------
        // Initialization
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initialize this card with the given CardData.
        /// </summary>
        public void Initialize(CardData cardData, float flipDuration)
        {
            Data = cardData;
            flipHalfDuration = flipDuration / 2f;

            if (cardFrontImage != null && cardData.cardSprite != null)
                cardFrontImage.sprite = cardData.cardSprite;

            SetFaceDown(instant: true);
        }

        // -----------------------------------------------------------------------
        // IPointerClickHandler
        // -----------------------------------------------------------------------

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isAnimating) return;
            if (State != CardState.FaceDown) return;

            MemoryMatchGameManager.Instance?.OnCardClicked(this);
        }

        // -----------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------

        /// <summary>
        /// Flip the card to show its front face.
        /// </summary>
        public void FlipFaceUp()
        {
            if (isAnimating || State == CardState.FaceUp || State == CardState.Matched) return;
            StartCoroutine(FlipAnimation(faceUp: true));
        }

        /// <summary>
        /// Flip the card back to the back face (used on mismatch).
        /// </summary>
        public void FlipFaceDown()
        {
            if (isAnimating || State == CardState.FaceDown) return;
            StartCoroutine(FlipAnimation(faceUp: false));
        }

        /// <summary>
        /// Mark this card as matched and play the match effect.
        /// </summary>
        public void SetMatched()
        {
            State = CardState.Matched;
            StartCoroutine(MatchEffect());
        }

        // -----------------------------------------------------------------------
        // Coroutines – core learning material
        // -----------------------------------------------------------------------

        /// <summary>
        /// 3D-style flip animation using X-scale lerp:
        /// Phase 1: scale X 1 → 0 (card "turns away")
        /// Phase 2: swap sprite, then scale X 0 → 1 (card "faces new direction")
        /// </summary>
        private IEnumerator FlipAnimation(bool faceUp)
        {
            isAnimating = true;

            // Phase 1: shrink X to 0
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 midScale = new Vector3(0f, startScale.y, startScale.z);

            while (elapsed < flipHalfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / flipHalfDuration);
                transform.localScale = Vector3.Lerp(startScale, midScale, t);
                yield return null;
            }
            transform.localScale = midScale;

            // Swap visible face
            if (faceUp)
            {
                cardBackImage.gameObject.SetActive(false);
                cardFrontImage.gameObject.SetActive(true);
                State = CardState.FaceUp;
            }
            else
            {
                cardFrontImage.gameObject.SetActive(false);
                cardBackImage.gameObject.SetActive(true);
                State = CardState.FaceDown;
            }

            // Phase 2: grow X back to original
            elapsed = 0f;
            Vector3 endScale = startScale;

            while (elapsed < flipHalfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / flipHalfDuration);
                transform.localScale = Vector3.Lerp(midScale, endScale, t);
                yield return null;
            }
            transform.localScale = endScale;

            isAnimating = false;
        }

        /// <summary>
        /// Match effect: pulse scale → fade out toàn bộ card → ẩn hoàn toàn.
        /// </summary>
        private IEnumerator MatchEffect()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 bigScale = originalScale * matchPulseScale;

            // --- Pulse scale out ---
            float elapsed = 0f;
            while (elapsed < matchPulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / matchPulseDuration);
                transform.localScale = Vector3.Lerp(originalScale, bigScale, t);
                yield return null;
            }

            // --- Pulse scale back ---
            elapsed = 0f;
            while (elapsed < matchPulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / matchPulseDuration);
                transform.localScale = Vector3.Lerp(bigScale, originalScale, t);
                yield return null;
            }
            transform.localScale = originalScale;

            // --- Fade out: giảm alpha của tất cả Image con về 0 ---
            float fadeDuration = 0.35f;
            Image[] images = GetComponentsInChildren<Image>(includeInactive: true);

            // Lưu màu gốc
            Color[] startColors = new Color[images.Length];
            for (int i = 0; i < images.Length; i++)
                startColors[i] = images[i].color;

            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                for (int i = 0; i < images.Length; i++)
                {
                    Color c = startColors[i];
                    c.a = Mathf.Lerp(startColors[i].a, 0f, t);
                    images[i].color = c;
                }
                yield return null;
            }

            // Ẩn hoàn toàn
            gameObject.SetActive(false);
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private void SetFaceDown(bool instant)
        {
            State = CardState.FaceDown;
            cardBackImage?.gameObject.SetActive(true);
            cardFrontImage?.gameObject.SetActive(false);
            if (matchEffectObject != null) matchEffectObject.SetActive(false);
            // Reset any tints
            if (cardBackgroundImage != null) cardBackgroundImage.color = Color.clear;
            if (cardFrontImage != null)      cardFrontImage.color = Color.white;
        }
    }
}
