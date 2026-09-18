using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameState
{
    public enum GameStateEnum
    {
        NotStarted = 0,
        Playing = 1,
        Cascade = 2,
        End = 3
    }

    public GameStateEnum CurrentGameState = GameStateEnum.NotStarted;

    public IReadOnlyList<Vector2Int> directionalities = new Vector2Int[]
    {
        Vector2Int.right,
        Vector2Int.right + Vector2Int.down,
        Vector2Int.down,
        Vector2Int.left + Vector2Int.down,
    };

    public readonly Dictionary<Vector2Int, int?> SpotToSideOwnership = new Dictionary<Vector2Int, int?>();

    public int LastCascade { get; set; } = 0;


    public readonly int Height;
    public readonly int Width;

    [Range(2, 5)]
    public int InARowToSolve = 3;

    public GameState(int width, int height)
    {
        this.Width = width;
        this.Height = height;

        for (int xx = 0; xx < width; xx++)
        {
            for (int yy = 0; yy < height; yy++)
            {
                this.SpotToSideOwnership.Add(new Vector2Int(xx, yy), null);
            }
        }
    }

    public void SetSideOwnership(Vector2Int position, int side)
    {
        this.SpotToSideOwnership[position] = side;
    }

    public IReadOnlyList<Vector2Int> GetEmptySpots()
    {
        List<Vector2Int> emptySpots = new List<Vector2Int>();

        foreach (Vector2Int position in SpotToSideOwnership.Keys)
        {
            int? ownership = SpotToSideOwnership[position];

            if (ownership == null)
            {
                emptySpots.Add(position);
            }
        }

        return emptySpots;
    }

    public bool AnyEmptySpots()
    {
        foreach (Vector2Int position in SpotToSideOwnership.Keys)
        {
            int? ownership = SpotToSideOwnership[position];

            if (ownership == null)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetAllSolutionsFromCell(int sideIndex, Vector2Int cell, out List<CellsSolution> solutionsInvolvingCell)
    {
        solutionsInvolvingCell = new List<CellsSolution>();

        foreach (CellsSolution solution in GetAllSolutions(sideIndex, cell))
        {
            if (solution.Cells.Contains(cell))
            {
                solutionsInvolvingCell.Add(solution);
            }
        }

        return solutionsInvolvingCell.Any();
    }

    public bool TryGetAllSolutionsFromCellAlongDirection(int sideIndex, Vector2Int cell, Vector2Int offset, out CellsSolution solution)
    {
        // If we're too close to the end direction this offset is going in, don't consider this at all
        if (cell.x + offset.x * InARowToSolve > this.Width)
        {
            solution = null;
            return false;
        }

        if (cell.x + offset.x * (InARowToSolve - 1) < 0)
        {
            solution = null;
            return false;
        }

        if (cell.y + offset.y * InARowToSolve > this.Height)
        {
            solution = null;
            return false;
        }

        if (cell.y + offset.y * (InARowToSolve - 1) < 0)
        {
            solution = null;
            return false;
        }

        List<CellsSolution> solutions = new List<CellsSolution>();

        bool valid = true;

        for (int ii = 1; ii < InARowToSolve; ii++)
        {
            Vector2Int position = cell + offset * ii;

            int? ownership = this.SpotToSideOwnership[position];

            // This cell isn't ours
            if (ownership != sideIndex)
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
        List<Vector2Int> solutionCells = new List<Vector2Int>();
        for (int ii = 0; ii < InARowToSolve; ii++)
        {
            Vector2Int position = cell + offset * ii;
            solutionCells.Add(position);
        }
        solution = new CellsSolution(solutionCells, offset);
        return true;
    }


    public List<CellsSolution> GetAllSolutions()
    {
        List<CellsSolution> solutions = new List<CellsSolution>();

        for (int xx = 0; xx < this.Width; xx++)
        {
            for (int yy = 0; yy < this.Height; yy++)
            {
                Vector2Int position = new Vector2Int(xx, yy);

                if (!SpotToSideOwnership[position].HasValue)
                {
                    // This cell isn't claimed
                    continue;
                }

                foreach (Vector2Int direction in directionalities)
                {
                    if (TryGetAllSolutionsFromCellAlongDirection(SpotToSideOwnership[position].Value, position, direction, out CellsSolution cellSolutions))
                    {
                        solutions.Add(cellSolutions);
                    }
                }
                
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


    public List<CellsSolution> GetAllSolutions(int sideIndex, Vector2Int hypotheticalPosition)
    {
        List<CellsSolution> solutions = new List<CellsSolution>();

        for (int xx = 0; xx < this.Width; xx++)
        {
            for (int yy = 0; yy < this.Height; yy++)
            {
                Vector2Int position = new Vector2Int(xx, yy);

                if (hypotheticalPosition != position && !SpotToSideOwnership[position].HasValue)
                {
                    // This cell isn't claimed
                    continue;
                }

                foreach (Vector2Int direction in directionalities)
                {
                    if (TryGetAllSolutionsFromCellAlongDirection(sideIndex, position, direction, out CellsSolution cellSolutions))
                    {
                        solutions.Add(cellSolutions);
                    }
                }

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
}
