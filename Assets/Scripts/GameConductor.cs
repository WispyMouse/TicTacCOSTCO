using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
    public CascadeText CascadeText;
    public TMP_Text WinnerPanel;
    public Image WinnerIcon;

    [Range(2, 5)]
    public int InARowToSolve = 3;

    public Dictionary<Vector2Int, Cell> PositionsToCells { get; set; } = new Dictionary<Vector2Int, Cell>();
    public Dictionary<Vector2Int, List<CellsSolution>> AcceptedSolutions { get; set; } = new Dictionary<Vector2Int, List<CellsSolution>>();

    public LineRenderer LineRendererPF;
    private List<LineRenderer> SolutionLineRenderers { get; set; } = new List<LineRenderer>();

    public int Height { get; set; }
    public int Width { get; set; }

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
        this.AcceptedSolutions.Clear();
        this.WinnerPanel.transform.parent.gameObject.SetActive(false);
        this.CascadeText.transform.parent.gameObject.SetActive(false);

        this.Width = Mathf.RoundToInt(this.GridPainter.WidthSlider.value);
        this.Height = Mathf.RoundToInt(this.GridPainter.HeightSlider.value);
        this.CurrentGameState = new GameState(this.Width, this.Height);

        this.PositionsToCells = this.GridPainter.Paint();

        this.TurnOrderHolder.ResetGame();
        this.CurrentTurnWheel.ResetGame();
    }

    public void Update()
    {
        if (this.CurrentGameState.CurrentGameState == GameState.GameStateEnum.End)
        {
            return;
        }

        if (this.TurnOrderHolder.PlayerIsHuman(this.TurnOrderHolder.CurrentPlayerIndex))
        {
            this.HandleLeftClick();
        }
    }

    public void ChooseCell(Vector2Int toChoose)
    {
        ChooseCell(PositionsToCells[toChoose]);
    }

    public void ChooseCell(Cell toChoose)
    {
        toChoose.SetSide(TurnOrderHolder.CurrentTurnIconHolder.sprite, TurnOrderHolder.CurrentPlayerIndex);
        this.CurrentGameState.SpotToSideOwnership[toChoose.Position] = TurnOrderHolder.CurrentPlayerIndex;

        foreach (Cell curCell in PositionsToCells.Values)
        {
            curCell.SetHighlightStatus(false);
        }

        List<CellsSolution> solutions = GetUntrackedSolutions();

        foreach (CellsSolution solution in solutions)
        {
            foreach (Cell curCell in solution.Cells)
            {
                if (!this.AcceptedSolutions.TryGetValue(curCell.Position, out List<CellsSolution> cellSolutionsForHere))
                {
                    cellSolutionsForHere = new List<CellsSolution>();
                    this.AcceptedSolutions.Add(curCell.Position, cellSolutionsForHere);
                }

                cellSolutionsForHere.Add(solution);
                curCell.SetHighlightStatus(true);
            }

            this.DrawLineBetween(solution.Root, solution.Tail);
        }

        if (solutions.Any())
        {
            this.CurrentGameState.CurrentGameState = GameState.GameStateEnum.Cascade;
            this.CascadeText.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            // If there were no new solutions, and we're in cascade state,
            // the current player should be knocked out
            if (this.CurrentGameState.CurrentGameState == GameState.GameStateEnum.Cascade)
            {
                this.KnockoutPlayer(this.TurnOrderHolder.CurrentPlayerIndex);
            }
        }

        TurnOrderHolder.NextPlayerIcon();
    }

    void HandleLeftClick()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        RaycastHit2D raycast = Physics2D.Raycast(this.ClickCamera.ScreenToWorldPoint(Mouse.current.position.value), Vector2.zero);
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

        for (int xx = 0; xx < this.Width; xx++)
        {
            for (int yy = 0; yy < this.Height; yy++)
            {
                Vector2Int position = new Vector2Int(xx, yy);
                this.PositionsToCells.TryGetValue(position, out Cell currentCell);

                if (!currentCell.AlreadyPlaced)
                {
                    // This cell isn't claimed
                    continue;
                }

                if (currentCell.SideIndex != this.TurnOrderHolder.CurrentPlayerIndex)
                {
                    // This cell isn't ours
                    continue;
                }

                solutions.AddRange(GetUntrackedSolutionsFromCell(currentCell));
            }
        }

        return solutions;
    }

    public List<CellsSolution> GetUntrackedSolutionsFromCell(Cell cell)
    {
        List<CellsSolution> solutions = new List<CellsSolution>();

        if (TryGetUntrackedSolutionsFromCellAlongDirection(cell, Vector2Int.right, out CellsSolution rightSolution))
        {
            solutions.Add(rightSolution);
        }

        if (TryGetUntrackedSolutionsFromCellAlongDirection(cell, Vector2Int.down, out CellsSolution downSolution))
        {
            solutions.Add(downSolution);
        }

        if (TryGetUntrackedSolutionsFromCellAlongDirection(cell, Vector2Int.right + Vector2Int.down, out CellsSolution downRightSolution))
        {
            solutions.Add(downRightSolution);
        }

        if (TryGetUntrackedSolutionsFromCellAlongDirection(cell, Vector2Int.left + Vector2Int.down, out CellsSolution downLeftSolution))
        {
            solutions.Add(downLeftSolution);
        }

        return solutions;
    }

    bool TryGetUntrackedSolutionsFromCellAlongDirection(Cell cell, Vector2Int offset, out CellsSolution solution)
    {
        if (!this.TryGetAllSolutionsFromCellAlongDirection(cell, offset, out solution))
        {
            return false;
        }
        
        foreach (Cell curCell in solution.Cells)
        {
            // If there aren't any accepted solutions with this as a root, continue
            if (!this.AcceptedSolutions.TryGetValue(curCell.Position, out List<CellsSolution> solutionsAtRoot))
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

    bool TryGetAllSolutionsFromCellAlongDirection(Cell cell, Vector2Int offset, out CellsSolution solution)
    {
        // If we're too close to the end direction this offset is going in, don't consider this at all
        if (cell.Position.x + offset.x * InARowToSolve > this.Width)
        {
            solution = null;
            return false;
        }

        if (cell.Position.x + offset.x * (InARowToSolve - 1) < 0)
        {
            solution = null;
            return false;
        }

        if (cell.Position.y + offset.y * InARowToSolve > this.Height)
        {
            solution = null;
            return false;
        }

        if (cell.Position.y + offset.y * (InARowToSolve - 1) < 0)
        {
            solution = null;
            return false;
        }

        List<CellsSolution> solutions = new List<CellsSolution>();

        bool valid = true;

        for (int ii = 0; ii < InARowToSolve; ii++)
        {
            Vector2Int position = cell.Position + offset * ii;

            if (!this.PositionsToCells.TryGetValue(position, out Cell currentCell))
            {
                Debug.LogError($"Attempted to gather cell information for {position}, but that's not in the map. This shouldn't be querying out of bounds places!");
                valid = false;
                break;
            }

            // If there isn't a mark here, we can't have scored yet
            if (!currentCell.AlreadyPlaced)
            {
                valid = false;
                break;
            }

            // This cell isn't ours
            if (currentCell.SideIndex != this.TurnOrderHolder.CurrentPlayerIndex)
            {
                valid = false;
                break;
            }
        }

        if (!valid)
        {
            solution = null;
            return false;
        }

        // If still valid, this must have been a solve
        List<Cell> solutionCells = new List<Cell>();
        for (int ii = 0; ii < InARowToSolve; ii++)
        {
            Vector2Int position = cell.Position + offset * ii;
            this.PositionsToCells.TryGetValue(position, out Cell currentCell);
            solutionCells.Add(currentCell);
        }
        solution = new CellsSolution(solutionCells, offset);
        return true;
    }

    public void DrawLineBetween(Cell cellA, Cell cellB)
    {
        LineRenderer newRenderer = Instantiate(this.LineRendererPF);
        newRenderer.SetPosition(0, cellA.transform.position + Vector3.back * 5f);
        newRenderer.SetPosition(1, cellB.transform.position + Vector3.back * 5f);
        newRenderer.startColor = this.TurnOrderHolder.KnockoutColorsForTurns[this.TurnOrderHolder.CurrentPlayerIndex];
        newRenderer.endColor = this.TurnOrderHolder.KnockoutColorsForTurns[this.TurnOrderHolder.CurrentPlayerIndex];
        this.SolutionLineRenderers.Add(newRenderer);
    }

    public void KnockoutPlayer(int index)
    {
        this.TurnOrderHolder.KnockOutPlayer(index);
        this.CurrentTurnWheel.KnockOutPlayer(index);

        if (this.TurnOrderHolder.SideIndexesStillInGame.Count == 1)
        {
            this.CascadeText.transform.parent.gameObject.SetActive(false);
            this.CurrentGameState.CurrentGameState = GameState.GameStateEnum.End;
            this.WinnerPanel.transform.parent.gameObject.SetActive(true);
            this.WinnerIcon.sprite = this.TurnOrderHolder.SpritesForTurns[this.TurnOrderHolder.CurrentPlayerIndex];
        }
    }
}
