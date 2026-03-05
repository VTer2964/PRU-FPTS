using UnityEngine;
using TMPro;

namespace CaroGame
{
    public class CaroMoveCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI moveText;

        public void UpdateMoveCount(int moves)
        {
            if (moveText != null)
                moveText.text = $"Moves: {moves}";
        }

        public void ResetCounter()
        {
            if (moveText != null)
                moveText.text = "Moves: 0";
        }
    }
}
