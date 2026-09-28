namespace TicTacCOSTCO.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using TicTacCOSTCO.DataStructures;
    using UnityEngine;
    using TicTacCOSTCO.DataStructures.Tools;

    public class UndoTests
    {
        public class UndoTests_UndosAtEnd_DataSource_Object
        {
            public List<Coordinate> Placements;
            public int Width;
            public int Height;
            public int Players;

            public int UndosAtEnd;

            public int ExpectedPlayerIndex;
            public GameState.GameStateEnum ExpectedGameStateEnum;
            public int ExpectedCascade;
            public int ExpectedRemainingPlayers;

            public UndoTests_UndosAtEnd_DataSource_Object(List<Coordinate> placements, int width, int height, int players, int undosAtEnd,
                int expectedPlayerIndex, GameState.GameStateEnum expectedGameStateEnum, int expectedCascade, int expectedRemainingPlayers)
            {
                this.Placements = placements;
                this.Width = width;
                this.Height = height;
                this.Players = players;
                this.UndosAtEnd = undosAtEnd;
                this.ExpectedPlayerIndex = expectedPlayerIndex;
                this.ExpectedGameStateEnum = expectedGameStateEnum;
                this.ExpectedCascade = expectedCascade;
                this.ExpectedRemainingPlayers = expectedRemainingPlayers;
            }

            public override string ToString()
            {
                return $"({string.Join(", ", Placements.Select(x => x.ToString()))}) ({this.Width}x{this.Height} {this.Players} players) ({this.ExpectedPlayerIndex} player expected in {this.ExpectedGameStateEnum} with cascade {this.ExpectedCascade})";
            }
        }

        public static UndoTests_UndosAtEnd_DataSource_Object[] UndoTests_UndosAtEnd_DataSource =
        {
            // One action taken, undone
            new UndoTests_UndosAtEnd_DataSource_Object(new List<Coordinate>() { Coordinate.zero },
                5, 5, 2, 1, 0, GameState.GameStateEnum.NotStarted, 0, 2),

            // Two actions taken, both undone
            new UndoTests_UndosAtEnd_DataSource_Object(new List<Coordinate>() { Coordinate.zero, Coordinate.right },
                5, 5, 2, 2, 0, GameState.GameStateEnum.NotStarted, 0, 2),

            // Two actions taken, one undone
            new UndoTests_UndosAtEnd_DataSource_Object(new List<Coordinate>() { Coordinate.zero, Coordinate.right },
                5, 5, 2, 1, 1, GameState.GameStateEnum.Playing, 0, 2),

            // Three-in-a-row for one player, undone
            new UndoTests_UndosAtEnd_DataSource_Object(new List<Coordinate>() { Coordinate.zero, Coordinate.right, Coordinate.right * 2 },
                5, 5, 1, 1, 0, GameState.GameStateEnum.Playing, 0, 1),

            // Three-in-a-row for one player in two player game, undone
            new UndoTests_UndosAtEnd_DataSource_Object(new List<Coordinate>() { Coordinate.zero, Coordinate.up, Coordinate.right, Coordinate.up * 2, Coordinate.right * 2 },
                5, 5, 2, 1, 0, GameState.GameStateEnum.Playing, 0, 2),

            // Three-in-a-row for one player, responded to with losing move, undone
            new UndoTests_UndosAtEnd_DataSource_Object(new List<Coordinate>() { Coordinate.zero, Coordinate.up, Coordinate.right, Coordinate.up * 2, Coordinate.right * 2, Coordinate.up + Coordinate.right },
                5, 5, 2, 1, 1, GameState.GameStateEnum.Cascade, 1, 2),

            // In three-player-game, undo targets expected player
            new UndoTests_UndosAtEnd_DataSource_Object(new List<Coordinate>() { Coordinate.zero, Coordinate.right, Coordinate.right * 2 },
                5, 5, 3, 1, 2, GameState.GameStateEnum.Playing, 0, 3),
            new UndoTests_UndosAtEnd_DataSource_Object(new List<Coordinate>() { Coordinate.zero, Coordinate.right, Coordinate.right * 2 },
                5, 5, 3, 2, 1, GameState.GameStateEnum.Playing, 0, 3),
            new UndoTests_UndosAtEnd_DataSource_Object(new List<Coordinate>() { Coordinate.zero, Coordinate.right, Coordinate.right * 2, Coordinate.up, Coordinate.up + Coordinate.right, Coordinate.up + Coordinate.right * 2 },
                5, 5, 3, 6, 0, GameState.GameStateEnum.NotStarted, 0, 3),
        };

        [Test]
        [TestCaseSource(nameof(UndoTests_UndosAtEnd_DataSource))]
        public void KnockoutStatusAsExpected(UndoTests_UndosAtEnd_DataSource_Object plan)
        {
            GameState testState = new GameState(plan.Width, plan.Height, plan.Players, forceZeroIndexStart: true);
            List<int> expectedOwnership = new List<int>();

            for (int ii = 0, count = plan.Placements.Count; ii < count; ii++)
            {
                testState.ApplyMoveCommand(testState.GenerateCommandFromMove(testState.CurrentPlayerIndex, plan.Placements[ii]), true);
                expectedOwnership.Add(testState.SpotToSideOwnership[plan.Placements[ii]].Value);
            }

            for (int ii = 0; ii < plan.UndosAtEnd; ii++)
            {
                testState.ReversePreviousMoveCommand();
            }

            Assert.AreEqual(plan.ExpectedGameStateEnum, testState.CurrentGameState, $"Expecting game state to be in specific status");
            Assert.AreEqual(plan.ExpectedPlayerIndex, testState.CurrentPlayerIndex, $"Expecting current player index to be specific");
            Assert.AreEqual(plan.ExpectedCascade, testState.CurrentCascadeLevel, $"Expecting cascade to be specific");
            Assert.AreEqual(plan.ExpectedRemainingPlayers, testState.SideIndexesStillInGame.Count, $"Expecting remaining player count to be specific");

            for (int ii = 0, count = plan.Placements.Count - plan.UndosAtEnd; ii < count; ii++)
            {
                Assert.IsTrue(testState.SpotToSideOwnership[plan.Placements[ii]] == expectedOwnership[ii], "Expecting ownership to still be claimed");
            }

            for (int ii = plan.Placements.Count - plan.UndosAtEnd + 1, count = plan.Placements.Count; ii < count; ii++)
            {
                Assert.IsTrue(testState.SpotToSideOwnership[plan.Placements[ii]] == null, "Expecting ownership to now be empty");
            }
        }
    }

}
