namespace TicTacCOSTCO.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using TicTacCOSTCO.DataStructures;

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

        // Quadruple Cascade
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { 
                // Diagonal up/right
                Coordinate.zero, Coordinate.right + Coordinate.up, (Coordinate.right + Coordinate.up) * 3, (Coordinate.right + Coordinate.up) * 4,
                // Diagonal up/left
                Coordinate.right * 4, Coordinate.right * 3 + Coordinate.up, Coordinate.right + Coordinate.up * 3, Coordinate.up * 4,
                // Horizontal
                Coordinate.up * 2, Coordinate.up * 2 + Coordinate.right, Coordinate.up * 2 + Coordinate.right * 3, Coordinate.up * 2 + Coordinate.right * 4,
                // Vertical
                Coordinate.right * 2, Coordinate.right * 2 + Coordinate.up, Coordinate.right * 2 + Coordinate.up * 3,  Coordinate.right * 2 + Coordinate.up * 4,
                // Pop it!
                Coordinate.up * 2 + Coordinate.right * 2
            },
            5, 5, 4),

        // 4-in-a-row should count as one solution
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { Coordinate.zero, Coordinate.right * 2, Coordinate.right * 3, Coordinate.right },
            5, 5, 1),

        // 5-in-a-row should count as one solution
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { Coordinate.zero, Coordinate.right, Coordinate.right * 3, Coordinate.right * 4, Coordinate.right * 2 },
            5, 5, 1),

        // Shouldn't count as a new solution if you continue an old 3-of
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { Coordinate.right, Coordinate.right * 2, Coordinate.right * 3, Coordinate.right * 4 },
            5, 5, 0),

        // Shouldn't care that there was a previously made connection
        new PlacementCausesSolve_DataSource_Object(
            new List<Coordinate>() { Coordinate.right, Coordinate.right * 2, Coordinate.right * 3, Coordinate.up + Coordinate.right, Coordinate.up + Coordinate.right * 2 },
            5, 5, 0),
    };

        [Test]
        [TestCaseSource(nameof(PlacementCausesSolve_DataSource))]
        public void PlacingThirdWouldResultInRow(PlacementCausesSolve_DataSource_Object plan)
        {
            int lastIndex = plan.Placements.Count - 1;
            BoardStateHolder testState = new BoardStateHolder(plan.Width, plan.Height, 1, 0);

            for (int ii = 0; ii < lastIndex; ii++)
            {
                Assert.IsTrue(testState.TryApplyMoveCommand(testState.CurrentBoardState.GenerateCommandFromMove(0, plan.Placements[ii]), false));
            }

            MoveCommand lastCommand = testState.CurrentBoardState.GenerateCommandFromMove(0, plan.Placements[lastIndex]);
            Assert.AreEqual(plan.ExpectedSolutions, lastCommand.ConnectionsMade.Count, $"Especting a specific amount of solutions total");

            Assert.IsTrue(testState.TryApplyMoveCommand(lastCommand, false));

            if (plan.ExpectedSolutions > 0)
            {
                foreach (Coordinate position in plan.Placements)
                {
                    if (!testState.CurrentBoardState.TryGetConnectionsForCoordinate(position, out IReadOnlyCollection<CellsConnection> positionSolutions))
                    {
                        Assert.Fail($"There should be an accepted solution for {position}.");
                    }
                }
            }
        }
    }

}
