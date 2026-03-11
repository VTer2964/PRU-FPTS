using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace CaroGame
{
    public class CaroCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image cellBackground;
        [SerializeField] private Image pieceImage;
        [SerializeField] private Image highlightBorder;
        [SerializeField] private Button cellButton;
        [SerializeField] private TextMeshProUGUI pieceText;

        private Vector2Int gridPosition;
        private CellState currentState = CellState.Empty;
        private bool isHovered;

        public Vector2Int GridPosition => gridPosition;
        public CellState CurrentState => currentState;

        public event Action<Vector2Int> OnCellClicked;

        // Color palette
        private static readonly Color EmptyColor    = new Color(0.94f, 0.91f, 0.83f);        // warm parchment
        private static readonly Color OccupiedColor = new Color(0.98f, 0.97f, 0.95f);        // near-white
        private static readonly Color HoverColor    = new Color(0.78f, 0.90f, 1.00f);        // soft blue
        private static readonly Color PlayerColor   = new Color(0.13f, 0.38f, 0.85f);        // rich blue
        private static readonly Color AIColor       = new Color(0.85f, 0.15f, 0.15f);        // rich red
        private static readonly Color WinColor      = new Color(0.15f, 0.90f, 0.35f, 0.75f); // green

        public void Initialize(int x, int y)
        {
            gridPosition  = new Vector2Int(x, y);
            currentState  = CellState.Empty;

            if (cellButton != null)
            {
                cellButton.onClick.AddListener(HandleClick);

                // Disable Button's built-in hover tint so our colours take priority
                var cb = cellButton.colors;
                cb.highlightedColor = Color.white;
                cb.pressedColor     = new Color(0.85f, 0.85f, 0.85f);
                cb.selectedColor    = Color.white;
                cellButton.colors   = cb;
            }

            UpdateVisual();
            SetHighlight(false);
        }

        private void HandleClick()
        {
            if (currentState != CellState.Empty) return;
            OnCellClicked?.Invoke(gridPosition);
        }

        // ── Hover ────────────────────────────────────────────────────────────

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentState != CellState.Empty) return;
            isHovered = true;
            if (cellBackground != null) cellBackground.color = HoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            if (currentState == CellState.Empty && cellBackground != null)
                cellBackground.color = EmptyColor;
        }

        // ── State ────────────────────────────────────────────────────────────

        public void SetState(CellState state)
        {
            currentState = state;
            isHovered    = false;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            switch (currentState)
            {
                case CellState.Empty:
                    if (cellBackground != null) cellBackground.color = EmptyColor;
                    if (pieceText  != null) pieceText.text = "";
                    if (pieceImage != null) pieceImage.enabled = false;
                    break;

                case CellState.Player:
                    if (cellBackground != null) cellBackground.color = OccupiedColor;
                    if (pieceText != null)
                    {
                        pieceText.text  = "X";
                        pieceText.color = PlayerColor;
                    }
                    if (pieceImage != null)
                    {
                        pieceImage.enabled = true;
                        pieceImage.color   = PlayerColor;
                    }
                    break;

                case CellState.AI:
                    if (cellBackground != null) cellBackground.color = OccupiedColor;
                    if (pieceText != null)
                    {
                        pieceText.text  = "O";
                        pieceText.color = AIColor;
                    }
                    if (pieceImage != null)
                    {
                        pieceImage.enabled = true;
                        pieceImage.color   = AIColor;
                    }
                    break;
            }
        }

        // ── Highlight ─────────────────────────────────────────────────────────

        public void SetHighlight(bool isHighlighted)
        {
            if (highlightBorder != null)
                highlightBorder.enabled = isHighlighted;
        }

        public void SetWinHighlight(bool isHighlighted)
        {
            if (highlightBorder != null)
            {
                highlightBorder.enabled = isHighlighted;
                highlightBorder.color   = WinColor;
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
            isHovered    = false;
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
