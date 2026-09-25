namespace TicTacCOSTCO.Unity.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using TicTacCOSTCO.DataStructures;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
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

        public Dictionary<Coordinate, Cell> PositionsToCells { get; set; } = new Dictionary<Coordinate, Cell>();

        public LineRenderer LineRendererPF;
        private List<LineRenderer> SolutionLineRenderers { get; set; } = new List<LineRenderer>();

        public GameState CurrentGameState { get; set; }

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
            this.WinnerPanel.transform.parent.gameObject.SetActive(false);
            this.CascadeText.transform.parent.gameObject.SetActive(false);
            this.NoMoreMovesPanel.SetActive(false);

            this.CurrentGameState = new GameState(
                PersistentGameConfiguration.Singleton.Width, 
                PersistentGameConfiguration.Singleton.Height, 
                PersistentGameConfiguration.Singleton.Players.Count);

            this.PositionsToCells = this.GridPainter.Paint();
            this.TurnOrderHolder.ResetGame();
            this.CurrentTurnWheel.ResetGame();
        }

        public void Update()
        {
            if (this.CurrentGameState != null && this.CurrentGameState.CurrentGameState == GameState.GameStateEnum.End)
            {
                return;
            }

            if (this.TurnOrderHolder.PlayerIsHuman(this.CurrentGameState.CurrentPlayerIndex))
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
            int takingTurn = this.CurrentGameState.CurrentPlayerIndex;
            PlayerProfile player = PersistentGameConfiguration.Singleton.Players[takingTurn];
            int previousCascade = this.CurrentGameState.LastCascade;

            toChoose.SetSide(TurnOrderHolder.CurrentTurnIconHolder.sprite, this.CurrentGameState.CurrentPlayerIndex);
            this.CurrentGameState.SetSideOwnership(toChoose.Position, this.CurrentGameState.CurrentPlayerIndex, out List<CellsConnection> newSolutions);

            foreach (CellsConnection solution in newSolutions)
            {
                this.DrawLineBetween(solution.Root, solution.Tail, player);

                foreach (Coordinate curCell in solution.Cells)
                {
                    PositionsToCells[curCell].SetHighlightStatus(false);
                }
            }

            int solutionsCount = newSolutions.Count;
            if (solutionsCount > previousCascade)
            {
                this.CascadeText.transform.parent.gameObject.SetActive(true);
                switch (solutionsCount)
                {
                    case 1:
                        this.CascadeText.TextString = "ROW";
                        break;
                    default:
                        this.CascadeText.TextString = $"CASCADE x{solutionsCount}";
                        break;
                }
            }

            if (this.CurrentGameState.CurrentGameState == GameState.GameStateEnum.End)
            {
                if (!this.CurrentGameState.Winner.HasValue)
                {
                    this.NoMoreMovesPanel.gameObject.SetActive(true);
                }
                else
                {
                    DeclareCurrentPlayerVictorious();
                }
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

        public void DrawLineBetween(Coordinate cellA, Coordinate cellB, PlayerProfile faction)
        {
            LineRenderer newRenderer = Instantiate(this.LineRendererPF);
            newRenderer.SetPosition(0, this.PositionsToCells[cellA].transform.position + Vector3.back * 5f);
            newRenderer.SetPosition(1, this.PositionsToCells[cellB].transform.position + Vector3.back * 5f);
            newRenderer.startColor = faction.KnockoutColor;
            newRenderer.endColor = faction.KnockoutColor;
            this.SolutionLineRenderers.Add(newRenderer);
        }

        public void DeclareCurrentPlayerVictorious()
        {
            PlayerProfile player = PersistentGameConfiguration.Singleton.Players[this.CurrentGameState.CurrentPlayerIndex];

            this.TurnOrderHolder.UpdateTurn(this.CurrentGameState.CurrentPlayerIndex);
            this.CascadeText.transform.parent.gameObject.SetActive(false);
            this.WinnerPanel.transform.parent.gameObject.SetActive(true);
            this.WinnerIcon.sprite = player.RepresenterSprite;

            string winnerName = player.PlayerName;
            if (string.IsNullOrEmpty(winnerName))
            {
                this.WinnerText.text = winnerName;
                this.WinnerText.gameObject.SetActive(true);
            }
            else
            {
                this.WinnerText.gameObject.SetActive(false);
            }

            this.ScoreBoard.AddToScoreboard(player);
        }
    }
}