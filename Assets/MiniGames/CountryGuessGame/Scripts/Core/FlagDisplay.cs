using UnityEngine;
using UnityEngine.UI;

namespace CountryGuessGame.Core
{
    /// <summary>
    /// Component to display the country flag
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class FlagDisplay : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image flagImage;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (flagImage == null)
            {
                flagImage = GetComponent<Image>();
            }

            // Add CanvasGroup for fade animation
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Set the flag sprite and animate it in
        /// </summary>
        public void SetFlag(Sprite flagSprite)
        {
            if (flagSprite == null)
            {
                Debug.LogWarning("Flag sprite is null!");
                return;
            }

            flagImage.sprite = flagSprite;
            
            // Fade in animation
            StartCoroutine(FadeIn());
        }

        /// <summary>
        /// Clear the flag display
        /// </summary>
        public void ClearFlag()
        {
            flagImage.sprite = null;
            canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Fade in animation
        /// </summary>
        private System.Collections.IEnumerator FadeIn()
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }
    }
}
