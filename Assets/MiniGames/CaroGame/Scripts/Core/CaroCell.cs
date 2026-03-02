using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CaroGame
{
    public class CaroCell : MonoBehaviour
    {
        [SerializeField] private Image cellBackground;
        [SerializeField] private Image pieceImage;
        [SerializeField] private Image highlightBorder;
        [SerializeField] private Button cellButton;
        [SerializeField] private TextMeshProUGUI pieceText;

        private Vector2Int gridPosition;
        private CellState currentState = CellState.Empty;

        public Vector2Int GridPosition => gridPosition;
        public CellState CurrentState => currentState;

        public event Action<Vector2Int> OnCellClicked;

        // Colors
        private static readonly Color EmptyColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color PlayerColor = new Color(0.2f, 0.5f, 0.9f, 1f);
        private static readonly Color AIColor = new Color(0.9f, 0.25f, 0.25f, 1f);
        private static readonly Color HighlightColor = new Color(1f, 0.85f, 0.2f, 0.8f);
        private static readonly Color HoverColor = new Color(0.85f, 0.9f, 1f, 1f);

        public void Initialize(int x, int y)
        {
            gridPosition = new Vector2Int(x, y);
            currentState = CellState.Empty;

            if (cellButton != null)
                cellButton.onClick.AddListener(HandleClick);

            UpdateVisual();
            SetHighlight(false);
        }

        private void HandleClick()
        {
            if (currentState != CellState.Empty) return;
            OnCellClicked?.Invoke(gridPosition);
        }

        public void SetState(CellState state)
        {
            currentState = state;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            switch (currentState)
            {
                case CellState.Empty:
                    if (cellBackground != null) cellBackground.color = EmptyColor;
                    if (pieceText != null) pieceText.text = "";
                    if (pieceImage != null) pieceImage.enabled = false;
                    break;

                case CellState.Player:
                    if (cellBackground != null) cellBackground.color = Color.white;
                    if (pieceText != null)
                    {
                        pieceText.text = "X";
                        pieceText.color = PlayerColor;
                    }
                    if (pieceImage != null)
                    {
                        pieceImage.enabled = true;
                        pieceImage.color = PlayerColor;
                    }
                    break;

                case CellState.AI:
                    if (cellBackground != null) cellBackground.color = Color.white;
                    if (pieceText != null)
                    {
                        pieceText.text = "O";
                        pieceText.color = AIColor;
                    }
                    if (pieceImage != null)
                    {
                        pieceImage.enabled = true;
                        pieceImage.color = AIColor;
                    }
                    break;
            }
        }

        public void SetHighlight(bool isHighlighted)
        {
            if (highlightBorder != null)
            {
                highlightBorder.enabled = isHighlighted;
                highlightBorder.color = HighlightColor;
            }
        }

        public void SetInteractable(bool canClick)
        {
            if (cellButton != null)
                cellButton.interactable = canClick;
        }

        public void ResetCell()
        {
            currentState = CellState.Empty;
            UpdateVisual();
            SetHighlight(false);
            SetInteractable(true);
        }

        private void OnDestroy()
        {
            if (cellButton != null)
                cellButton.onClick.RemoveListener(HandleClick);
        }
    }
}
