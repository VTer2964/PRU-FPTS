using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaroGame
{
    public class CaroGameManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CaroGameConfig config;

        [Header("References")]
        [SerializeField] private CaroBoardController boardController;
        [SerializeField] private CaroAI caroAI;
        [SerializeField] private CaroTimerSystem timerSystem;
        [SerializeField] private CaroUIManager uiManager;

        private CaroGameStateType currentState = CaroGameStateType.MainMenu;
        private CaroWinChecker winChecker;
        private CaroMoveValidator moveValidator;
        private CaroHintSystem hintSystem;

        private int playerMoveCount;
        private int aiMoveCount;
        private bool hintsEnabled;

        // FPTSim integration
        public System.Action<CaroMedalType> OnGameEnded;

        private void Awake()
        {
            winChecker = new CaroWinChecker(config.boardSize, config.winCondition);
            moveValidator = new CaroMoveValidator(config.boardSize);
            hintSystem = new CaroHintSystem(config.boardSize);
            hintsEnabled = config.showHints;
        }

        private void Start()
        {
            // Subscribe to events
            boardController.OnCellClicked += OnCellClicked;
            timerSystem.OnTurnTimeOut += OnTurnTimeout;
            timerSystem.SetTimeLimit(config.turnTimeLimit);

            uiManager.OnPlayClicked += StartGame;
            uiManager.OnRestartClicked += StartGame;
            uiManager.OnMenuClicked += ReturnToMenu;
            uiManager.OnHintToggleClicked += ToggleHints;

            uiManager.SetHintButtonState(hintsEnabled);
            uiManager.ShowMainMenu();
        }

        public void StartGame()
        {
            boardController.InitializeBoard();
            boardController.OnCellClicked += OnCellClicked;

            playerMoveCount = 0;
            aiMoveCount = 0;

            currentState = CaroGameStateType.PlayerTurn;

            uiManager.ShowGamePanel();
            uiManager.SetTurnIndicator("Your Turn (X)");
            uiManager.UpdateMoveCount(0);

            timerSystem.StartTurn();
            boardController.SetAllInteractable(true);

            if (hintsEnabled)
                UpdateHints();
        }

        private void OnCellClicked(Vector2Int position)
        {
            if (currentState != CaroGameStateType.PlayerTurn) return;

            int[,] boardState = boardController.GetBoardStateCopy();
            if (!moveValidator.IsValidMove(boardState, position)) return;

            // Place player piece
            boardController.PlacePiece(position.x, position.y, (int)CellState.Player);
            playerMoveCount++;
            uiManager.UpdateMoveCount(playerMoveCount);

            // Check win condition
            boardState = boardController.GetBoardStateCopy();
            int winner = winChecker.CheckWinner(boardState);
            if (winner != 0)
            {
                EndGame(winner, false);
                return;
            }

            // Switch to AI turn
            timerSystem.StopTurn();
            currentState = CaroGameStateType.AIThinking;
            boardController.SetAllInteractable(false);
            boardController.ClearAllHighlights();
            uiManager.SetTurnIndicator("AI Thinking...");

            StartCoroutine(AITurnCoroutine());
        }

        private IEnumerator AITurnCoroutine()
        {
            // Visual delay for realism
            yield return new WaitForSeconds(config.aiThinkDelay);

            // Get AI move
            int[,] boardState = boardController.GetBoardStateCopy();
            Vector2Int aiMove = caroAI.GetBestMove(boardState);

            // Place AI piece
            boardController.PlacePiece(aiMove.x, aiMove.y, (int)CellState.AI);
            aiMoveCount++;

            // Check win condition
            boardState = boardController.GetBoardStateCopy();
            int winner = winChecker.CheckWinner(boardState);
            if (winner != 0)
            {
                EndGame(winner, false);
                yield break;
            }

            // Back to player turn
            currentState = CaroGameStateType.PlayerTurn;
            boardController.SetAllInteractable(true);
            uiManager.SetTurnIndicator("Your Turn (X)");
            timerSystem.StartTurn();

            if (hintsEnabled)
                UpdateHints();
        }

        private void OnTurnTimeout()
        {
            if (currentState != CaroGameStateType.PlayerTurn) return;
            EndGame(2, true); // AI wins by timeout
        }

        private void EndGame(int winner, bool timedOut)
        {
            currentState = CaroGameStateType.GameOver;
            timerSystem.StopTurn();
            boardController.SetAllInteractable(false);
            boardController.ClearAllHighlights();

            CaroMedalType medal = CalculateMedal(winner, playerMoveCount, timedOut);

            // Highlight winning line if there is one
            int[,] boardState = boardController.GetBoardStateCopy();
            List<Vector2Int> winLine = winChecker.GetWinningLine(boardState);
            if (winLine != null)
            {
                foreach (var pos in winLine)
                {
                    boardController.SetCellHighlight(pos.x, pos.y, true);
                }
            }

            string turnText = winner == 1 ? "You Win!" : winner == -1 ? "Draw!" : timedOut ? "Time's Up!" : "AI Wins!";
            uiManager.SetTurnIndicator(turnText);
            uiManager.ShowResults(winner, playerMoveCount, medal, timedOut);

            OnGameEnded?.Invoke(medal);
        }

        private CaroMedalType CalculateMedal(int winner, int playerMoves, bool timedOut)
        {
            if (winner == 1) // Player won
            {
                if (playerMoves <= config.goldMedalMoves)
                    return CaroMedalType.Gold;
                else
                    return CaroMedalType.Silver;
            }
            else if (winner == -1 || (winner == 2 && !timedOut))
            {
                // Draw or AI won but player didn't time out
                return CaroMedalType.Bronze;
            }
            else
            {
                // Player timed out
                return CaroMedalType.None;
            }
        }

        private void UpdateHints()
        {
            boardController.ClearAllHighlights();

            if (!hintsEnabled) return;

            int[,] boardState = boardController.GetBoardStateCopy();
            List<Vector2Int> dangerCells = hintSystem.GetDangerousCells(boardState, (int)CellState.AI);

            foreach (var cell in dangerCells)
            {
                boardController.SetCellHighlight(cell.x, cell.y, true);
            }
        }

        private void ToggleHints()
        {
            hintsEnabled = !hintsEnabled;
            uiManager.SetHintButtonState(hintsEnabled);

            if (currentState == CaroGameStateType.PlayerTurn)
            {
                if (hintsEnabled)
                    UpdateHints();
                else
                    boardController.ClearAllHighlights();
            }
        }

        private void ReturnToMenu()
        {
            currentState = CaroGameStateType.MainMenu;
            timerSystem.StopTurn();
            uiManager.ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (boardController != null)
                boardController.OnCellClicked -= OnCellClicked;
            if (timerSystem != null)
                timerSystem.OnTurnTimeOut -= OnTurnTimeout;
            if (uiManager != null)
            {
                uiManager.OnPlayClicked -= StartGame;
                uiManager.OnRestartClicked -= StartGame;
                uiManager.OnMenuClicked -= ReturnToMenu;
                uiManager.OnHintToggleClicked -= ToggleHints;
            }
        }
    }
}
