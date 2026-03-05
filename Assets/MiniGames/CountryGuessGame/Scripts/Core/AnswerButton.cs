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

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color correctColor = Color.green;
        [SerializeField] private Color wrongColor = Color.red;

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

            buttonImage = GetComponent<Image>();

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
    }
}
