using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CaroGame
{
    public class CaroResultsPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI resultDescriptionText;
        [SerializeField] private TextMeshProUGUI moveCountText;
        [SerializeField] private Image medalIcon;
        [SerializeField] private TextMeshProUGUI medalText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        [Header("Medal Colors")]
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f);
        [SerializeField] private Color noneColor = new Color(0.4f, 0.4f, 0.4f);

        private Action onRestart;
        private Action onMenu;

        /// <summary>
        /// Must be called before showing results. Wires button callbacks directly.
        /// </summary>
        public void Init(Action restartCallback, Action menuCallback)
        {
            onRestart = restartCallback;
            onMenu = menuCallback;

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() =>
                {
                    Debug.Log("[CaroGame] Restart clicked");
                    onRestart?.Invoke();
                });
            }
            if (menuButton != null)
            {
                menuButton.onClick.RemoveAllListeners();
                menuButton.onClick.AddListener(() =>
                {
                    Debug.Log("[CaroGame] Menu clicked");
                    onMenu?.Invoke();
                });
            }
        }

        public void ShowResults(int winner, int playerMoves, CaroMedalType medal, bool timedOut)
        {
            gameObject.SetActive(true);

            if (resultTitleText != null)
            {
                if (winner == 1)
                    resultTitleText.text = "YOU WIN!";
                else if (winner == -1)
                    resultTitleText.text = "DRAW!";
                else if (timedOut)
                    resultTitleText.text = "TIME'S UP!";
                else
                    resultTitleText.text = "AI WINS!";
            }

            if (resultDescriptionText != null)
            {
                if (winner == 1)
                    resultDescriptionText.text = $"Congratulations! You won in {playerMoves} moves!";
                else if (winner == -1)
                    resultDescriptionText.text = "The board is full. It's a draw!";
                else if (timedOut)
                    resultDescriptionText.text = "You ran out of time!";
                else
                    resultDescriptionText.text = "Better luck next time!";
            }

            if (moveCountText != null)
                moveCountText.text = $"Total Moves: {playerMoves}";

            SetMedalDisplay(medal);
        }

        private void SetMedalDisplay(CaroMedalType medal)
        {
            if (medalIcon != null)
            {
                medalIcon.enabled = medal != CaroMedalType.None;
                medalIcon.color = GetMedalColor(medal);
            }

            if (medalText != null)
            {
                switch (medal)
                {
                    case CaroMedalType.Gold:
                        medalText.text = "GOLD MEDAL";
                        medalText.color = goldColor;
                        break;
                    case CaroMedalType.Silver:
                        medalText.text = "SILVER MEDAL";
                        medalText.color = silverColor;
                        break;
                    case CaroMedalType.Bronze:
                        medalText.text = "BRONZE MEDAL";
                        medalText.color = bronzeColor;
                        break;
                    default:
                        medalText.text = "NO MEDAL";
                        medalText.color = noneColor;
                        break;
                }
            }
        }

        private Color GetMedalColor(CaroMedalType medal)
        {
            switch (medal)
            {
                case CaroMedalType.Gold: return goldColor;
                case CaroMedalType.Silver: return silverColor;
                case CaroMedalType.Bronze: return bronzeColor;
                default: return noneColor;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (restartButton != null)
                restartButton.onClick.RemoveAllListeners();
            if (menuButton != null)
                menuButton.onClick.RemoveAllListeners();
        }
    }
}
