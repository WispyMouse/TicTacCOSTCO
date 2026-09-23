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

            this.CurrentGameState = new GameState(Mathf.RoundToInt(this.GridPainter.WidthSlider.value), Mathf.RoundToInt(this.GridPainter.HeightSlider.value), (int)this.TurnOrderHolder.PlayerCountSlider.value);

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
            int previousCascade = this.CurrentGameState.LastCascade;

            toChoose.SetSide(TurnOrderHolder.CurrentTurnIconHolder.sprite, this.CurrentGameState.CurrentPlayerIndex);
            this.CurrentGameState.SetSideOwnership(toChoose.Position, this.CurrentGameState.CurrentPlayerIndex, out List<CellsSolution> newSolutions);

            foreach (CellsSolution solution in newSolutions)
            {
                this.DrawLineBetween(solution.Root, solution.Tail, takingTurn);

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

            if (this.CurrentGameState.SideIndexesStillInGame.Count == 1)
            {
                DeclareCurrentPlayerVictorious();
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

        public List<CellsSolution> GetUntrackedSolutions()
        {
            List<CellsSolution> solutions = new List<CellsSolution>();

            for (int xx = 0; xx < this.CurrentGameState.Width; xx++)
            {
                for (int yy = 0; yy < this.CurrentGameState.Height; yy++)
                {
                    Coordinate position = new Coordinate(xx, yy);
                    this.PositionsToCells.TryGetValue(position, out Cell currentCell);

                    if (!currentCell.AlreadyPlaced)
                    {
                        // This cell isn't claimed
                        continue;
                    }

                    if (currentCell.SideIndex != this.CurrentGameState.CurrentPlayerIndex)
                    {
                        // This cell isn't ours
                        continue;
                    }

                    solutions.AddRange(GetUntrackedSolutionsFromCell(currentCell.Position));
                }
            }

            // Check if any new solutions should be banded together; 4-in-a-row is the same value as a 3-in-a-row
            // Any solutions that have the same directionality *must* be bandable
            bool anyDiscarded = false;

            do
            {
                anyDiscarded = false;
                for (int leftSolutionIndex = solutions.Count - 2; leftSolutionIndex >= 0; leftSolutionIndex--)
                {
                    bool discardLeftSolution = false;
                    CellsSolution leftCellsSolution = solutions[leftSolutionIndex];
                    for (int rightSolutionIndex = solutions.Count - 1; rightSolutionIndex > leftSolutionIndex; rightSolutionIndex--)
                    {
                        CellsSolution rightCellsSolution = solutions[rightSolutionIndex];

                        if (leftCellsSolution.Directionality == rightCellsSolution.Directionality)
                        {
                            // Add a new composite solution to the end of the list, which won't be evaluated again
                            CellsSolution compositeSolution = new CellsSolution(leftCellsSolution.Cells.Union(rightCellsSolution.Cells).ToList(), leftCellsSolution.Directionality);
                            solutions.Add(compositeSolution);
                            discardLeftSolution = true;
                            anyDiscarded = true;

                            // We can immediately discard this rightSolution
                            solutions.RemoveAt(rightSolutionIndex);
                        }
                    }

                    if (discardLeftSolution)
                    {
                        solutions.RemoveAt(leftSolutionIndex);
                    }
                }
            } while (anyDiscarded);

            return solutions;
        }

        public List<CellsSolution> GetUntrackedSolutionsFromCell(Coordinate cell)
        {
            List<CellsSolution> solutions = new List<CellsSolution>();

            if (TryGetUntrackedSolutionsFromCellAlongDirection(cell, Coordinate.right, out CellsSolution rightSolution))
            {
                solutions.Add(rightSolution);
            }

            if (TryGetUntrackedSolutionsFromCellAlongDirection(cell, Coordinate.down, out CellsSolution downSolution))
            {
                solutions.Add(downSolution);
            }

            if (TryGetUntrackedSolutionsFromCellAlongDirection(cell, Coordinate.right + Coordinate.down, out CellsSolution downRightSolution))
            {
                solutions.Add(downRightSolution);
            }

            if (TryGetUntrackedSolutionsFromCellAlongDirection(cell, Coordinate.left + Coordinate.down, out CellsSolution downLeftSolution))
            {
                solutions.Add(downLeftSolution);
            }

            return solutions;
        }

        bool TryGetUntrackedSolutionsFromCellAlongDirection(Coordinate cell, Coordinate offset, out CellsSolution solution)
        {
            if (!this.CurrentGameState.TryGetAllSolutionsFromCellAlongDirection(this.CurrentGameState.CurrentPlayerIndex, cell, offset, out solution))
            {
                return false;
            }

            foreach (Coordinate curCell in solution.Cells)
            {
                // If there aren't any accepted solutions with this as a root, continue
                if (!this.CurrentGameState.AcceptedSolutions.TryGetValue(curCell, out List<CellsSolution> solutionsAtRoot))
                {
                    continue;
                }

                // We only care about a solution with the same directionality (right, downright, down, downleft)
                // If there is the same, then this shouldn't be part of a new solution. it's already tracked
                foreach (CellsSolution alreadySolvedSolutions in solutionsAtRoot)
                {
                    if (alreadySolvedSolutions.Directionality == offset)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void DrawLineBetween(Coordinate cellA, Coordinate cellB, int factionIndex)
        {
            LineRenderer newRenderer = Instantiate(this.LineRendererPF);
            newRenderer.SetPosition(0, this.PositionsToCells[cellA].transform.position + Vector3.back * 5f);
            newRenderer.SetPosition(1, this.PositionsToCells[cellB].transform.position + Vector3.back * 5f);
            newRenderer.startColor = this.TurnOrderHolder.KnockoutColorsForTurns[factionIndex];
            newRenderer.endColor = this.TurnOrderHolder.KnockoutColorsForTurns[factionIndex];
            this.SolutionLineRenderers.Add(newRenderer);
        }

        public void DeclareCurrentPlayerVictorious()
        {
            this.TurnOrderHolder.UpdateTurn(this.CurrentGameState.CurrentPlayerIndex);
            this.CascadeText.transform.parent.gameObject.SetActive(false);
            this.CurrentGameState.CurrentGameState = GameState.GameStateEnum.End;
            this.WinnerPanel.transform.parent.gameObject.SetActive(true);
            this.WinnerIcon.sprite = this.TurnOrderHolder.SpritesForTurns[this.CurrentGameState.CurrentPlayerIndex];
            this.CurrentGameState.CurrentGameState = GameState.GameStateEnum.End;

            string winnerName = this.TurnOrderHolder.PlayerNames[this.CurrentGameState.CurrentPlayerIndex];
            if (string.IsNullOrEmpty(winnerName))
            {
                this.WinnerText.text = winnerName;
                this.WinnerText.gameObject.SetActive(true);
                this.ScoreBoard.AddToScoreboard(this.CurrentGameState.CurrentPlayerIndex);
            }
            else
            {
                this.WinnerText.gameObject.SetActive(false);
            }
        }
    }
}