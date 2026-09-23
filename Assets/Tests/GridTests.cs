namespace TicTacCOSTCO.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using TicTacCOSTCO.DataStructures;
    using UnityEngine;
    using TicTacCOSTCO.DataStructures.Tools;

    public class GridTests
    {
        public class PlacementCausesSolve_DataSource_Object
        {
            public List<Coordinate> Placements;
            public int Width;
            public int Height;
            public int ExpectedSolutions;

            public PlacementCausesSolve_DataSource_Object(List<Coordinate> placements, int width, int height, int expectedSolutions)
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
            new List<Coordinate>() { Coordinate.right, Coordinate.right + Coordinate.right, Coordinate.zero, },
            3, 3, 1),
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { Coordinate.right, Coordinate.zero, Coordinate.right + Coordinate.right },
            3, 3, 1),
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { Coordinate.zero, Coordinate.up, Coordinate.up * 2 },
            3, 3, 1),
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { Coordinate.right, Coordinate.right + Coordinate.right + Coordinate.up, Coordinate.up * 2 + Coordinate.right * 3 },
            5, 5, 1),

        // This shouldn't be a score
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { Coordinate.right + Coordinate.up, Coordinate.right + Coordinate.up + Coordinate.up, Coordinate.zero },
            5, 5, 0),

        // Double cascade
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { Coordinate.right * 2, Coordinate.right, Coordinate.up, Coordinate.up * 2, Coordinate.zero },
            5, 5, 2),

        // 4-in-a-row should count as one solution
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { Coordinate.zero, Coordinate.right * 2, Coordinate.right * 3, Coordinate.right },
            5, 5, 1),

        // Shouldn't count as a new solution if you continue an old 3-of
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { Coordinate.right, Coordinate.right * 2, Coordinate.right * 3, Coordinate.right * 4 },
            5, 5, 0),
    };

        [Test]
        [TestCaseSource(nameof(PlacementCausesSolve_DataSource))]
        public void PlacingThirdWouldResultInRow(PlacementCausesSolve_DataSource_Object plan)
        {
            int lastIndex = plan.Placements.Count - 1;
            GameState testState = new GameState(plan.Width, plan.Height, 1);

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
                foreach (Coordinate position in plan.Placements)
                {
                    if (!testState.AcceptedSolutions.TryGetValue(position, out List<CellsSolution> positionSolutions))
                    {
                        Assert.Fail($"There should be an accepted solution for {position}.");
                    }
                }
            }
        }
    }

}
