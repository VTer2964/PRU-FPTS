using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CountryGuessGame.Core
{
    /// <summary>
    /// Individual answer button for country selection
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AnswerButton : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;

        // Colors are hardcoded to prevent Inspector overrides causing wrong button colors
        private readonly Color normalColor  = new Color(0.13f, 0.35f, 0.70f, 1f);   // vivid blue
        private readonly Color correctColor = new Color(0.12f, 0.76f, 0.32f, 1f);   // green
        private readonly Color wrongColor   = new Color(0.84f, 0.18f, 0.18f, 1f);   // red

        private bool isCorrectAnswer = false;
        private System.Action<bool> onAnswerSelected;
        private Image buttonImage;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (buttonText == null)
            {
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
            }

            buttonImage = button.targetGraphic as Image;
            if (buttonImage == null) buttonImage = GetComponentInChildren<Image>();

            // Disable Button's built-in ColorTint transition so it never fights with our manual color management
            button.transition = Selectable.Transition.None;

            // Reset to normal color
            if (buttonImage != null) buttonImage.color = normalColor;

            // Add click listener
            button.onClick.AddListener(OnButtonClicked);
        }

        /// <summary>
        /// Setup the answer button
        /// </summary>
        public void SetupButton(string countryName, bool isCorrect, System.Action<bool> callback)
        {
            Debug.Log($"[AnswerButton] Setting up button: {countryName}, isCorrect: {isCorrect}");

            if (buttonText != null)
            {
                buttonText.text = countryName;
            }
            else
            {
                Debug.LogWarning($"[AnswerButton] buttonText is null for {gameObject.name}");
            }

            isCorrectAnswer = isCorrect;
            onAnswerSelected = callback;

            // Reset visual state
            ResetVisual();
            button.interactable = true;
        }

        /// <summary>
        /// Handle button click
        /// </summary>
        private void OnButtonClicked()
        {
            Debug.Log($"[AnswerButton] Button clicked: {gameObject.name}, isCorrect: {isCorrectAnswer}");

            // Disable all buttons after selection
            button.interactable = false;

            // Show visual feedback
            ShowFeedback();

            // Invoke callback
            if (onAnswerSelected != null)
            {
                onAnswerSelected.Invoke(isCorrectAnswer);
            }
            else
            {
                Debug.LogWarning($"[AnswerButton] onAnswerSelected callback is null!");
            }
        }

        /// <summary>
        /// Show visual feedback for correct/wrong answer
        /// </summary>
        private void ShowFeedback()
        {
            if (buttonImage != null)
            {
                buttonImage.color = isCorrectAnswer ? correctColor : wrongColor;
            }
        }

        /// <summary>
        /// Highlight as correct answer (for showing correct answer when wrong is selected)
        /// </summary>
        public void HighlightCorrect()
        {
            if (isCorrectAnswer && buttonImage != null)
            {
                buttonImage.color = correctColor;
            }
        }

        /// <summary>
        /// Reset visual state
        /// </summary>
        private void ResetVisual()
        {
            if (buttonImage != null)
            {
                buttonImage.color = normalColor;
            }
        }

        /// <summary>
        /// Disable button interaction
        /// </summary>
        public void DisableButton()
        {
            button.interactable = false;
        }

        /// <summary>
        /// Highlight/unhighlight this button as the keyboard cursor selection.
        /// </summary>
        public void SetCursorHighlight(bool on)
        {
            if (buttonImage == null || !button.interactable) return;

            buttonImage.color = on ? new Color(1f, 0.80f, 0.10f, 1f) : normalColor; // gold / normal blue

            // Bold text khi được chọn bằng bàn phím
            if (buttonText != null)
                buttonText.fontStyle = on ? TMPro.FontStyles.Bold : TMPro.FontStyles.Normal;
        }

        /// <summary>
        /// Simulate a button click via keyboard (only fires if interactable).
        /// </summary>
        public void SimulateClick()
        {
            if (button != null && button.interactable)
                button.onClick.Invoke();
        }
    }
}
