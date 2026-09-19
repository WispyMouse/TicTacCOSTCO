using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridTests
{
    public class PlacementCausesSolve_DataSource_Object
    {
        public List<Vector2Int> Placements;
        public int Width;
        public int Height;
        public int ExpectedSolutions;

        public PlacementCausesSolve_DataSource_Object(List<Vector2Int> placements, int width, int height, int expectedSolutions)
        {
            this.Placements = placements;
            this.Width = width;
            this.Height = height;
            this.ExpectedSolutions = expectedSolutions;
        }

        public override string ToString()
        {
            return $"({string.Join(", ", Placements.Select(x => x.ToString()))}) ({this.Width}x{this.Height}) ({this.ExpectedSolutions} expected)";
        }
    }

    public static PlacementCausesSolve_DataSource_Object[] PlacementCausesSolve_DataSource =
    {
        // Simple single test cases
        new PlacementCausesSolve_DataSource_Object(
            new List<Vector2Int>() { Vector2Int.right, Vector2Int.right + Vector2Int.right, Vector2Int.zero, },
            3, 3, 1),
        new PlacementCausesSolve_DataSource_Object(
            new List<Vector2Int>() { Vector2Int.right, Vector2Int.zero, Vector2Int.right + Vector2Int.right },
            3, 3, 1),
        new PlacementCausesSolve_DataSource_Object(
            new List<Vector2Int>() { Vector2Int.zero, Vector2Int.up, Vector2Int.up * 2 },
            3, 3, 1),
        new PlacementCausesSolve_DataSource_Object(
            new List<Vector2Int>() { Vector2Int.right, Vector2Int.right + Vector2Int.right + Vector2Int.up, Vector2Int.up * 2 + Vector2Int.right * 3 },
            5, 5, 1),

        // This shouldn't be a score
        new PlacementCausesSolve_DataSource_Object(
            new List<Vector2Int>() { Vector2Int.right + Vector2Int.up, Vector2Int.right + Vector2Int.up + Vector2Int.up, Vector2Int.zero },
            5, 5, 0),

        // Double cascade
        new PlacementCausesSolve_DataSource_Object(
            new List<Vector2Int>() { Vector2Int.right * 2, Vector2Int.right, Vector2Int.up, Vector2Int.up * 2, Vector2Int.zero },
            5, 5, 2),

        // 4-in-a-row should count as one solution
        new PlacementCausesSolve_DataSource_Object(
            new List<Vector2Int>() { Vector2Int.zero, Vector2Int.right * 2, Vector2Int.right * 3, Vector2Int.right },
            5, 5, 1),

        // Shouldn't count as a new solution if you continue an old 3-of
        new PlacementCausesSolve_DataSource_Object(
            new List<Vector2Int>() { Vector2Int.right, Vector2Int.right * 2, Vector2Int.right * 3, Vector2Int.right * 4 },
            5, 5, 0),
    };

    [Test]
    [TestCaseSource(nameof(PlacementCausesSolve_DataSource))]
    public void PlacingThirdWouldResultInRow(PlacementCausesSolve_DataSource_Object plan)
    {
        int lastIndex = plan.Placements.Count - 1;
        GameState testState = new GameState(plan.Width, plan.Height);

        for (int ii = 0; ii < lastIndex; ii++)
        {
            testState.SetSideOwnership(plan.Placements[ii], 0, out _);
        }

        List<CellsSolution> solutions = HypotheticalSolutionTool.GetSolutionsFromClaimingTile(testState, plan.Placements[lastIndex], 0);
        Assert.AreEqual(plan.ExpectedSolutions, solutions.Count, $"Expecting a specific amount of solutions from placing the last tile");

        testState.SetSideOwnership(plan.Placements[lastIndex], 0, out List<CellsSolution> newSolutions);
        Assert.AreEqual(plan.ExpectedSolutions, newSolutions.Count, $"Especting a specific amount of solutions total");

        if (plan.ExpectedSolutions > 0)
        {
            foreach (Vector2Int position in plan.Placements)
            {
                if (!testState.AcceptedSolutions.TryGetValue(position, out List<CellsSolution> positionSolutions))
                {
                    Assert.Fail($"There should be an accepted solution for {position}.");
                }
            }
        }
    }
}
