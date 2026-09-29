namespace TicTacCOSTCO.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using TicTacCOSTCO.DataStructures;

    public class KnockoutTests
    {
        public class Knockout_ExpectedPlayer_DataSource_Object
        {
            public List<Coordinate> Placements;
            public int Width;
            public int Height;
            public int Players;

            public int ExpectedPlayerIndex;
            public BoardState.GameStateEnum ExpectedGameStateEnum;
            public int ExpectedCascade;
            public int ExpectedRemainingPlayers;

            public Knockout_ExpectedPlayer_DataSource_Object(List<Coordinate> placements, int width, int height, int players, int expectedPlayerIndex, 
                BoardState.GameStateEnum expectedGameStateEnum, int expectedCascade, int expectedRemainingPlayers)
            {
                this.Placements = placements;
                this.Width = width;
                this.Height = height;
                this.Players = players;
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

        public static Knockout_ExpectedPlayer_DataSource_Object[] Knockout_ExpectedPlayer_DataSource =
        {
            // Early in the game, it's still in playing state
            new Knockout_ExpectedPlayer_DataSource_Object(new List<Coordinate>()
            {
                Coordinate.zero, Coordinate.up, Coordinate.right,
            }, 5, 5, 2, 1, BoardState.GameStateEnum.Playing, 0, 2),

            // Player 1 places three in a row while Player 2 doesn't
            // On Player 2's response turn, the cascade should be set, but the game isn't over
            new Knockout_ExpectedPlayer_DataSource_Object(new List<Coordinate>()
            {
                Coordinate.zero, Coordinate.up, Coordinate.right, Coordinate.up + Coordinate.right, Coordinate.right * 2
            }, 5, 5, 2, 1, BoardState.GameStateEnum.Cascade, 1, 2),
            
            // Player 2 makes a move that ends up losing them the game
            new Knockout_ExpectedPlayer_DataSource_Object(new List<Coordinate>()
            {
                Coordinate.zero, Coordinate.up, Coordinate.right, Coordinate.up + Coordinate.right, Coordinate.right * 2, Coordinate.up * 2
            }, 5, 5, 2, 0, BoardState.GameStateEnum.End, 1, 1),

            // Player 2 makes a move that keeps them in the game by placing three in a row
            new Knockout_ExpectedPlayer_DataSource_Object(new List<Coordinate>()
            {
                Coordinate.zero, Coordinate.up, Coordinate.right, Coordinate.up + Coordinate.right, Coordinate.right * 2, Coordinate.up + Coordinate.right * 2
            }, 5, 5, 2, 0, BoardState.GameStateEnum.Cascade, 1, 2),

            // Player 0 responds with a move that should lose them the game
            new Knockout_ExpectedPlayer_DataSource_Object(new List<Coordinate>()
            {
                Coordinate.zero, Coordinate.up, Coordinate.right, Coordinate.up + Coordinate.right, Coordinate.right * 2, Coordinate.up + Coordinate.right * 2,

                // Should not make a new cascade, thus should make the first player lose
                Coordinate.right * 4
            }, 5, 5, 2, 1, BoardState.GameStateEnum.End, 1, 1)
        };

        [Test]
        [TestCaseSource(nameof(Knockout_ExpectedPlayer_DataSource))]
        public void KnockoutStatusAsExpected(Knockout_ExpectedPlayer_DataSource_Object plan)
        {
            BoardStateHolder testState = new BoardStateHolder(plan.Width, plan.Height, plan.Players, forceZeroIndexStart: true);

            for (int ii = 0, count = plan.Placements.Count; ii < count; ii++)
            {
                testState.ApplyMoveCommand(testState.CurrentBoardState.GenerateCommandFromMove(testState.CurrentBoardState.CurrentPlayerIndex, plan.Placements[ii]), true);
            }

            Assert.AreEqual(plan.ExpectedGameStateEnum, testState.CurrentBoardState.CurrentGameState, $"Expecting game state to be in specific status");
            Assert.AreEqual(plan.ExpectedPlayerIndex, testState.CurrentBoardState.CurrentPlayerIndex, $"Expecting current player index to be specific");
            Assert.AreEqual(plan.ExpectedCascade, testState.CurrentBoardState.CurrentCascadeLevel, $"Expecting cascade to be specific");
            Assert.AreEqual(plan.ExpectedRemainingPlayers, testState.CurrentBoardState.SideIndexesStillInGame.Count, $"Expecting remaining player count to be specific");
        }
    }

}
