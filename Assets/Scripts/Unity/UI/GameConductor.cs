namespace TicTacCOSTCO.Unity.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using TicTacCOSTCO.DataStructures;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine.Profiling;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class GameConductor : MonoBehaviour
    {
        public Camera ClickCamera;

        public TurnOrderHolder TurnOrderHolder;
        public GridPainter GridPainter;
        public CurrentTurnWheel CurrentTurnWheel;
        public ScoreBoard ScoreBoard;
        public CascadeText CascadeText;
        public TMP_Text WinnerPanel;
        public TMP_Text WinnerText;
        public Image WinnerIcon;
        public GameObject NoMoreMovesPanel;
        public CascadeBanner CascadeBanner;

        public Dictionary<Coordinate, Cell> PositionsToCells { get; set; } = new Dictionary<Coordinate, Cell>();

        public LineRenderer LineRendererPF;
        private List<LineRenderer> SolutionLineRenderers { get; set; } = new List<LineRenderer>();
        private Dictionary<CellsConnection, LineRenderer> connectionToRenderer { get; set; } = new Dictionary<CellsConnection, LineRenderer>();

        public BoardStateHolder CurrentGameState { get; set; }

        public GameObject RewindButtonHolder;

        public void Start()
        {
            this.ResetGame();
        }

        public void ResetGame()
        {
            for (int ii = this.SolutionLineRenderers.Count - 1; ii >= 0; ii--)
            {
                Destroy(this.SolutionLineRenderers[ii].gameObject);
            }
            this.SolutionLineRenderers.Clear();
            this.connectionToRenderer.Clear();

            this.WinnerPanel.transform.parent.gameObject.SetActive(false);
            this.CascadeText.transform.parent.gameObject.SetActive(false);
            this.RewindButtonHolder.SetActive(false);
            this.NoMoreMovesPanel.SetActive(false);

            this.CurrentGameState = new BoardStateHolder(
                PersistentGameConfiguration.Singleton.Width, 
                PersistentGameConfiguration.Singleton.Height, 
                PersistentGameConfiguration.Singleton.Players.Count);

            this.PositionsToCells = this.GridPainter.Paint();
            this.TurnOrderHolder.ResetGame();
            this.CurrentTurnWheel.ResetGame();
            this.CascadeBanner.ResetGame();
        }

        public void Update()
        {
            if (this.CurrentGameState != null && this.CurrentGameState.CurrentBoardState.CurrentGameState == BoardState.GameStateEnum.End)
            {
                return;
            }

            if (this.TurnOrderHolder.PlayerIsHuman(this.CurrentGameState.CurrentBoardState.CurrentPlayerIndex))
            {
                this.HandleLeftClick();
            }
        }

        public void ChooseCell(Coordinate toChoose)
        {
            ChooseCell(PositionsToCells[toChoose]);
        }

        public void ChooseCell(Cell toChoose)
        {
            foreach (Cell curCell in this.PositionsToCells.Values)
            {
                curCell.SetHighlightStatus(false);
            }

            AudioPlayer.Singleton.PlayBlip();

            int takingTurn = this.CurrentGameState.CurrentBoardState.CurrentPlayerIndex;
            PlayerProfile player = PersistentGameConfiguration.Singleton.Players[takingTurn];
            int previousCascade = this.CurrentGameState.CurrentBoardState.CurrentCascadeLevel;

            toChoose.SetSide(TurnOrderHolder.CurrentTurnIconHolder.sprite, this.CurrentGameState.CurrentBoardState.CurrentPlayerIndex);
            MoveCommand command = this.CurrentGameState.CurrentBoardState.GenerateCommandFromMove(this.CurrentGameState.CurrentBoardState.CurrentPlayerIndex, toChoose.Position);
            this.CurrentGameState.ApplyMoveCommand(command);

            foreach (CellsConnection solution in command.ConnectionsMade)
            {
                this.DrawLineBetween(solution, player);

                foreach (Coordinate curCell in solution.Cells)
                {
                    PositionsToCells[curCell].SetHighlightStatus(true);
                }
            }

            SetCascadeVisuals(command);

            if (this.CurrentGameState.CurrentBoardState.CurrentGameState == BoardState.GameStateEnum.End)
            {
                if (!this.CurrentGameState.CurrentBoardState.Winner.HasValue)
                {
                    this.NoMoreMovesPanel.gameObject.SetActive(true);
                }
                else
                {
                    DeclareCurrentPlayerVictorious();
                }
            }
            else
            {
                this.RewindButtonHolder.SetActive(this.CurrentGameState != null && this.CurrentGameState.MoveCommandsApplied.Count > 0);
            }

            toChoose.SetHighlightStatus(true);
        }

        void SetCascadeVisuals(MoveCommand fromCommand)
        {
            int cascadeLevel = fromCommand == null ? 0 : Mathf.Max(fromCommand.PreviousCascadeLevel, fromCommand.ConnectionsMade.Count);
            if (cascadeLevel > 0)
            {
                this.CascadeText.transform.parent.gameObject.SetActive(true);
                switch (cascadeLevel)
                {
                    case 1:
                        this.CascadeText.TextString = "ROW";
                        break;
                    default:
                        this.CascadeText.TextString = $"CASCADE x{cascadeLevel}";
                        break;
                }
            }
            else
            {
                this.CascadeText.transform.parent.gameObject.SetActive(false);
            }
        }

        void HandleLeftClick()
        {
            Vector2 raycastPoint;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                raycastPoint = Mouse.current.position.value;
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                raycastPoint = Touchscreen.current.primaryTouch.position.value;
            }
            else
            {
                return;
            }

            RaycastHit2D raycast = Physics2D.Raycast(this.ClickCamera.ScreenToWorldPoint(raycastPoint), Vector2.zero);
            if (raycast.collider == null)
            {
                return;
            }

            Cell getCell = raycast.collider.gameObject.GetComponent<Cell>();

            if (getCell == null)
            {
                return;
            }

            if (getCell.AlreadyPlaced)
            {
                return;
            }

            this.ChooseCell(getCell);
        }

        public void DrawLineBetween(CellsConnection connection, PlayerProfile faction)
        {
            LineRenderer newRenderer = Instantiate(this.LineRendererPF);
            newRenderer.SetPosition(0, this.PositionsToCells[connection.Root].transform.position + Vector3.back * 5f);
            newRenderer.SetPosition(1, this.PositionsToCells[connection.Tail].transform.position + Vector3.back * 5f);
            newRenderer.startColor = faction.KnockoutColor;
            newRenderer.endColor = faction.KnockoutColor;
            this.SolutionLineRenderers.Add(newRenderer);
            this.connectionToRenderer.Add(connection, newRenderer);
        }

        public void DeclareCurrentPlayerVictorious()
        {
            PlayerProfile player = PersistentGameConfiguration.Singleton.Players[this.CurrentGameState.CurrentBoardState.CurrentPlayerIndex];

            this.TurnOrderHolder.UpdateTurn(this.CurrentGameState.CurrentBoardState.CurrentPlayerIndex);
            this.CascadeText.transform.parent.gameObject.SetActive(false);
            this.WinnerPanel.transform.parent.gameObject.SetActive(true);
            this.WinnerIcon.sprite = player.SpriteRepresentation;

            string winnerName = player.PlayerName;
            if (!string.IsNullOrEmpty(winnerName))
            {
                this.WinnerText.gameObject.SetActive(true);
                this.WinnerText.text = winnerName;
            }
            else
            {
                this.WinnerText.gameObject.SetActive(false);
            }

                this.ScoreBoard.AddToScoreboard(player);
        }

        public void MainMenu()
        {
            SceneManager.LoadScene(0);
        }

        public void Rewind()
        {
            // If this undid a win, we should remove the "recent wint" entry because this game has been undone
            if (this.CurrentGameState.CurrentBoardState.CurrentGameState == BoardState.GameStateEnum.End && this.CurrentGameState.CurrentBoardState.Winner.HasValue)
            {
                this.ScoreBoard.RemoveRecentWin();
            }

            // Rewind until we reach the last human move, or the beginning of the game.
            while (this.CurrentGameState.MoveCommandsApplied.Count > 0)
            {
                MoveCommand undoneCommand = this.CurrentGameState.ReversePreviousMoveCommand();
                
                foreach (CellsConnection connectionToRemove in undoneCommand.ConnectionsMade)
                {
                    LineRenderer renderer = connectionToRenderer[connectionToRemove];
                    this.connectionToRenderer.Remove(connectionToRemove);
                    this.SolutionLineRenderers.Remove(renderer);
                    Destroy(renderer.gameObject);
                }

                this.PositionsToCells[undoneCommand.Position].Clear();

                // When we reach a player, stop
                if (!PersistentGameConfiguration.Singleton.Players[this.CurrentGameState.CurrentBoardState.CurrentPlayerIndex].IsAI)
                {
                    break;
                }
            }

            // We have to have more moves, right?
            // Can't have a winner anymore either
            this.NoMoreMovesPanel.SetActive(false);
            this.WinnerPanel.transform.parent.gameObject.SetActive(false);

            foreach (Cell curCell in this.PositionsToCells.Values)
            {
                curCell.SetHighlightStatus(false);
            }

            MoveCommand latestCommand = this.CurrentGameState.MoveCommandsApplied.Count == 0 ? null : this.CurrentGameState.MoveCommandsApplied[this.CurrentGameState.MoveCommandsApplied.Count - 1];
            if (latestCommand != null)
            {
                this.SetCascadeVisuals(latestCommand);

                this.PositionsToCells[latestCommand.Position].SetHighlightStatus(true);

                foreach (Coordinate curCoordinate in latestCommand.ConnectionsMade.SelectMany(x => x.Cells))
                {
                    this.PositionsToCells[curCoordinate].SetHighlightStatus(true);
                }
            }
        }
    }
}