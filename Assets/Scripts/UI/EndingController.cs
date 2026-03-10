using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using FPTSim.Core;

namespace FPTSim.UI
{
    public class EndingController : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text silverText;
        [SerializeField] private TMP_Text bronzeText;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite happyEndingSprite;
        [SerializeField] private Sprite badEndingSprite;
        [SerializeField] private Sprite goHomeEndingSprite;

        [Header("Button")]
        [SerializeField] private Button continueButton;

        [Header("After Credit")]
        [SerializeField] private bool useAfterCredit = true;
        [SerializeField] private string afterCreditSceneName = "AfterCredit";

        private void Start()
        {
            // ✅ mở chuột để bấm UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (continueButton)
                continueButton.onClick.AddListener(OnContinue);

            RefreshUI();
        }

        private void OnDestroy()
        {
            if (continueButton)
                continueButton.onClick.RemoveListener(OnContinue);
        }

        private void RefreshUI()
        {
            if (GameManager.I == null)
            {
                Debug.LogError("[EndingController] GameManager.I is null.");
                return;
            }

            var state = GameManager.I.State;
            string endingKey = GameManager.I.DecideEnding();

            if (titleText)
                titleText.text = "ENDING";

            if (subtitleText)
            {
                switch (endingKey)
                {
                    case "HAPPY_END":
                        subtitleText.text = "Happy Ending";
                        break;

                    case "BAD_TIME_OUT":
                        subtitleText.text = "Bad Ending - Hết thời gian";
                        break;

                    case "GO_HOME":
                        subtitleText.text = "Tôi đi về";
                        break;

                    default:
                        subtitleText.text = endingKey;
                        break;
                }
            }

            if (goldText) goldText.text = $"Gold: {state.gold}";
            if (silverText) silverText.text = $"Silver: {state.silver}";
            if (bronzeText) bronzeText.text = $"Bronze: {state.bronze}";

            if (backgroundImage)
            {
                switch (endingKey)
                {
                    case "HAPPY_END":
                        backgroundImage.sprite = happyEndingSprite;
                        break;

                    case "BAD_TIME_OUT":
                        backgroundImage.sprite = badEndingSprite;
                        break;

                    case "GO_HOME":
                        backgroundImage.sprite = goHomeEndingSprite;
                        break;
                }
            }
        }

        private void OnContinue()
        {
            if (useAfterCredit)
                SceneManager.LoadScene(afterCreditSceneName);
            else
                GoToMainMenu();
        }

        private void GoToMainMenu()
        {
            if (GameManager.I != null)
                GameManager.I.GoMainMenu();
            else
                SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}