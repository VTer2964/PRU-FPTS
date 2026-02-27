using System;
using UnityEngine;
using UnityEngine.UI;

namespace CaroGame
{
    public class CaroBoardController : MonoBehaviour
    {
        [SerializeField] private CaroGameConfig config;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private Transform boardContainer;
        [SerializeField] private GridLayoutGroup gridLayout;

        private CaroCell[,] cells;
        private int[,] boardState;

        public int BoardSize => config.boardSize;

        public event Action<Vector2Int> OnCellClicked;

        public void InitializeBoard()
        {
            int size = config.boardSize;
            cells = new CaroCell[size, size];
            boardState = new int[size, size];

            // Clear existing children
            foreach (Transform child in boardContainer)
            {
                Destroy(child.gameObject);
            }

            // Configure grid layout
            if (gridLayout != null)
            {
                gridLayout.constraintCount = size;
                gridLayout.cellSize = new Vector2(config.cellSize, config.cellSize);
                gridLayout.spacing = new Vector2(config.cellSpacing, config.cellSpacing);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
                gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
                gridLayout.childAlignment = TextAnchor.MiddleCenter;
            }

            // Create cells
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    GameObject cellObj = Instantiate(cellPrefab, boardContainer);
                    cellObj.name = $"Cell_{row}_{col}";

                    CaroCell cell = cellObj.GetComponent<CaroCell>();
                    if (cell != null)
                    {
                        cell.Initialize(row, col);
                        cell.OnCellClicked += HandleCellClicked;
                        cells[row, col] = cell;
                    }

                    boardState[row, col] = 0;
                }
            }
        }

        private void HandleCellClicked(Vector2Int position)
        {
            OnCellClicked?.Invoke(position);
        }

        public bool IsCellEmpty(int row, int col)
        {
            if (!IsInBounds(row, col)) return false;
            return boardState[row, col] == 0;
        }

        public void PlacePiece(int row, int col, int player)
        {
            if (!IsInBounds(row, col)) return;
            boardState[row, col] = player;
            if (cells[row, col] != null)
            {
                cells[row, col].SetState(player == 1 ? CellState.Player : CellState.AI);
                cells[row, col].SetInteractable(false);
            }
        }

        public void ClearBoard()
        {
            if (cells == null) return;
            int size = config.boardSize;
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    boardState[row, col] = 0;
                    if (cells[row, col] != null)
                        cells[row, col].ResetCell();
                }
            }
        }

        public int[,] GetBoardStateCopy()
        {
            int size = config.boardSize;
            int[,] copy = new int[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    copy[row, col] = boardState[row, col];
                }
            }
            return copy;
        }

        public int GetBoardState(int row, int col)
        {
            if (!IsInBounds(row, col)) return -1;
            return boardState[row, col];
        }

        public bool IsInBounds(int row, int col)
        {
            return row >= 0 && row < config.boardSize && col >= 0 && col < config.boardSize;
        }

        public void SetCellHighlight(int row, int col, bool highlighted)
        {
            if (!IsInBounds(row, col)) return;
            if (cells[row, col] != null)
                cells[row, col].SetHighlight(highlighted);
        }

        public void ClearAllHighlights()
        {
            if (cells == null) return;
            int size = config.boardSize;
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (cells[row, col] != null)
                        cells[row, col].SetHighlight(false);
                }
            }
        }

        public void SetAllInteractable(bool interactable)
        {
            if (cells == null) return;
            int size = config.boardSize;
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (cells[row, col] != null && boardState[row, col] == 0)
                        cells[row, col].SetInteractable(interactable);
                }
            }
        }

        private void OnDestroy()
        {
            if (cells == null) return;
            int size = config.boardSize;
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (cells[row, col] != null)
                        cells[row, col].OnCellClicked -= HandleCellClicked;
                }
            }
        }
    }
}
