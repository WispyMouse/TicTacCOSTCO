using NUnit.Framework;
using System.Collections.Generic;
using TicTacCOSTCO.DataStructures;
using UnityEngine;
using static TicTacCOSTCO.Tests.CoordinateTests;

namespace TicTacCOSTCO.Tests
{
    public class GameResultTests
    {
        public class CoordinatesToPlay_DataSource_Object
        {
            public int XSize = 5;
            public int YSize = 5;
            public int PlayerCount = 2;
            public List<Coordinate> Coordinates;
            public int? Winner;

            public CoordinatesToPlay_DataSource_Object(int xSize, int ySize, int playerCount, List<Coordinate> coordinates, int? winner)
            {
                this.XSize = xSize;
                this.YSize = ySize;
                this.PlayerCount = playerCount;
                this.Coordinates = coordinates;
                this.Winner = winner;
            }

            public override string ToString()
            {
                return $"{this.PlayerCount} players over {this.Coordinates.Count} moves: winner: {this.Winner.ToString()}";
            }
        }

        public static CoordinatesToPlay_DataSource_Object[] CoordinatesToPlay_DataSource =
        {
            // O moves three in a row and thus should win
            new CoordinatesToPlay_DataSource_Object(3, 3, 2, 
                new List<Coordinate>()
            {
                    new Coordinate(0, 0), new Coordinate(0, 1),
                    new Coordinate(1, 0), new Coordinate(0, 2),
                    new Coordinate(2, 0), new Coordinate(1, 1),
            }, 0),

            // O moves three in a row, X responds, X should win because there are no more moves
            new CoordinatesToPlay_DataSource_Object(3, 2, 2,
                new List<Coordinate>()
            {
                    new Coordinate(0, 0), new Coordinate(0, 1),
                    new Coordinate(1, 0), new Coordinate(1, 1),
                    new Coordinate(2, 0), new Coordinate(2, 1),
            }, 1),

            // No one can make a connection on this tiny board, so no one wins
            new CoordinatesToPlay_DataSource_Object(2, 2, 2,
                new List<Coordinate>()
            {
                    new Coordinate(0, 0), new Coordinate(0, 1),
                    new Coordinate(1, 0), new Coordinate(1, 1),
            }, null),
        };

        [Test]
        [TestCaseSource(nameof(CoordinatesToPlay_DataSource))]
        public void ResultChecks(CoordinatesToPlay_DataSource_Object plan)
        {
            BoardState newBoard = new BoardState(plan.XSize, plan.YSize, plan.PlayerCount, 0, 0);

            for (int ii = 0; ii < plan.Coordinates.Count; ii++)
            {
                newBoard.TryApplyMoveCommand(newBoard.GenerateCommandFromMove(ii % plan.PlayerCount, plan.Coordinates[ii]));
            }

            Assert.AreEqual(BoardState.GameStateEnum.End, newBoard.CurrentGameState, $"Expecting board state to result in the end of a game.");
            Assert.AreEqual(newBoard.Winner, plan.Winner, $"Expecting a predicted winner / null lack of winner.");
        }
    }
}
