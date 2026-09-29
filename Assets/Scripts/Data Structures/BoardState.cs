using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TicTacCOSTCO.DataStructures
{
    public class BoardState
    {
        public static IReadOnlyList<DirectionalityVector> Directionalities = new DirectionalityVector[]
        {
            new DirectionalityVector(1, 0),
            new DirectionalityVector(1, -1),
            new DirectionalityVector(0, -1),
            new DirectionalityVector(-1, -1),
        };

        public readonly int Height;
        public readonly int Width;
        public readonly int PlayerCount;
        public readonly int InARowToSolve;

        public readonly Dictionary<Coordinate, int?> SpotToSideOwnership = new Dictionary<Coordinate, int?>();
        public int CurrentCascadeLevel { get; set; } = 0;
        public int CurrentPlayerIndex { get; set; } = 0;
        public HashSet<int> SideIndexesStillInGame = new HashSet<int>();

        public Dictionary<Coordinate, List<CellsConnection>> AcceptedSolutions { get; set; } = new Dictionary<Coordinate, List<CellsConnection>>();

        public enum GameStateEnum
        {
            /// <summary>
            /// This game has not started yet.
            /// </summary>
            NotStarted = 0,
            /// <summary>
            /// The game is progress.
            /// </summary>
            Playing = 1,
            /// <summary>
            /// Indicates the "cascade" state.
            /// Once in Cascade, players need to make at least <see cref="CurrentCascadeLevel"/>
            /// connections on their turn, or they are eliminated.
            /// </summary>
            Cascade = 2,
            /// <summary>
            /// Indicates that the game has ended.
            /// If <see cref="Winner"/> has no value, the game is a draw.
            /// Otherwise, that player index is declared the winner.
            /// </summary>
            End = 3
        }

        public GameStateEnum CurrentGameState { get; set; } = GameStateEnum.NotStarted;
        public int? Winner { get; set; } = null;

        public BoardState(int width, int height, int playerCount)
        {
            this.Width = width;
            this.Height = height;
            this.PlayerCount = playerCount;

            // HACK: Starting at 3 for development
            this.InARowToSolve = 3;

            this.SpotToSideOwnership.EnsureCapacity(width * height);
            for (int xx = 0; xx < width; xx++)
            {
                for (int yy = 0; yy < height; yy++)
                {
                    this.SpotToSideOwnership.Add(new Coordinate(xx, yy), null);
                }
            }

            this.SideIndexesStillInGame.EnsureCapacity(this.PlayerCount);
            for (int ii = 0; ii < this.PlayerCount; ii++)
            {
                this.SideIndexesStillInGame.Add(ii);
            }
        }

        public BoardState(int width, int height, int playerCount,
            Dictionary<Coordinate, int?> ownershipToClone, int currentCascadeLevel, int currentPlayerIndex, Dictionary<Coordinate, List<CellsConnection>> connectionsToClone,
            GameStateEnum currentStatus, int? winner, HashSet<int> sideIndexesStillInGame) : this(width, height, playerCount)
        {
            this.SpotToSideOwnership = new Dictionary<Coordinate, int?>(ownershipToClone);
            this.CurrentCascadeLevel = currentCascadeLevel;
            this.CurrentPlayerIndex = currentPlayerIndex;
            this.AcceptedSolutions = new Dictionary<Coordinate, List<CellsConnection>>(connectionsToClone);
            this.CurrentGameState = currentStatus;
            this.Winner = winner;
            this.SideIndexesStillInGame = new HashSet<int>(sideIndexesStillInGame);
        }

        public BoardState DeepClone()
        {
            return new BoardState(this.Width, this.Height, this.PlayerCount,
                this.SpotToSideOwnership, this.CurrentCascadeLevel, this.CurrentPlayerIndex,
                this.AcceptedSolutions, this.CurrentGameState, this.Winner, this.SideIndexesStillInGame);
        }

        public IReadOnlyList<Coordinate> GetEmptySpots()
        {
            List<Coordinate> emptySpots = new List<Coordinate>();

            foreach (Coordinate position in this.SpotToSideOwnership.Keys)
            {
                int? ownership = this.SpotToSideOwnership[position];

                if (ownership == null)
                {
                    emptySpots.Add(position);
                }
            }

            return emptySpots;
        }

        public bool AnyEmptySpots()
        {
            foreach (Coordinate position in this.SpotToSideOwnership.Keys)
            {
                int? ownership = this.SpotToSideOwnership[position];

                if (ownership == null)
                {
                    return true;
                }
            }

            return false;
        }

        public MoveCommand GenerateCommandFromMove(int sideIndex, Coordinate position)
        {
            List<CellsConnection> connections = GetAllNewSolutions(sideIndex, position);
            List<int> playersRemoved = new List<int>();

            // HACK SHOULD GENERALIZE: Would this move result in a loss
            // "newConnections" are determined by the number of actual new connections
            // we want to only check new scoring connections in this process
            if (this.CurrentGameState == BoardState.GameStateEnum.Cascade && this.CurrentCascadeLevel > connections.Count)
            {
                playersRemoved.Add(sideIndex);
            }

            return new MoveCommand(sideIndex, position, connections, this.CurrentCascadeLevel, playersRemoved);
        }


        public List<CellsConnection> GetAllNewSolutions(int sideIndex, Coordinate hypotheticalPosition)
        {
            List<CellsConnection> newSolutions = new List<CellsConnection>();

            for (int xx = 0; xx < this.Width; xx++)
            {
                for (int yy = 0; yy < this.Height; yy++)
                {
                    Coordinate position = new Coordinate(xx, yy);

                    if (hypotheticalPosition != position && !this.SpotToSideOwnership[position].HasValue && this.SpotToSideOwnership[position] != sideIndex)
                    {
                        // This cell isn't claimed
                        continue;
                    }

                    List<CellsConnection> allSolutions = new List<CellsConnection>();

                    foreach (DirectionalityVector direction in Directionalities)
                    {
                        if (TryGetAllSolutionsFromCellAlongDirection(sideIndex, position, direction, out CellsConnection cellSolutions, hypotheticalPosition))
                        {
                            allSolutions.Add(cellSolutions);
                        }

                        if (TryGetAllSolutionsFromCellAlongDirection(sideIndex, position, -direction, out cellSolutions, hypotheticalPosition))
                        {
                            allSolutions.Add(cellSolutions);
                        }
                    }

                    // Check that these solutions contain the new piece
                    for (int ii = 0, count = allSolutions.Count; ii < count; ii++)
                    {
                        if (allSolutions[ii].Cells.Contains(hypotheticalPosition))
                        {
                            newSolutions.Add(allSolutions[ii]);
                        }
                    }
                }
            }

            return PruneSolutionsForNotAlreadySolved(newSolutions);
        }

        public bool SpotIsInBounds(Coordinate position)
        {
            if (position.X >= this.Width)
            {
                return false;
            }

            if (position.X < 0)
            {
                return false;
            }

            if (position.Y >= this.Height)
            {
                return false;
            }

            if (position.Y < 0)
            {
                return false;
            }

            return true;
        }

        public List<CellsConnection> PruneSolutionsForNotAlreadySolved(List<CellsConnection> solutions)
        {
            List<CellsConnection> remainingSolutions = new List<CellsConnection>(solutions);

            // Check if any new solutions should be banded together; 4-in-a-row is the same value as a 3-in-a-row
            // Any solutions that have the same directionality *must* be bandable
            bool anyDiscarded = false;

            do
            {
                anyDiscarded = false;
                for (int leftSolutionIndex = remainingSolutions.Count - 2; leftSolutionIndex >= 0; leftSolutionIndex--)
                {
                    CellsConnection leftCellsSolution = remainingSolutions[leftSolutionIndex];
                    for (int rightSolutionIndex = remainingSolutions.Count - 1; rightSolutionIndex > leftSolutionIndex; rightSolutionIndex--)
                    {
                        CellsConnection rightCellsSolution = remainingSolutions[rightSolutionIndex];

                        // Forward or backward are the same
                        if (leftCellsSolution.Directionality == rightCellsSolution.Directionality || leftCellsSolution.Directionality == -rightCellsSolution.Directionality)
                        {
                            // Remove these two composite solutions, and note that they were removed
                            remainingSolutions.RemoveAt(rightSolutionIndex);
                            remainingSolutions.RemoveAt(leftSolutionIndex);

                            // Add a new composite solution to the end of the list, which won't be evaluated again
                            List<Coordinate> Sorted = SortByDirectionality(leftCellsSolution.Cells.Union(rightCellsSolution.Cells), leftCellsSolution.Directionality);
                            CellsConnection compositeSolution = new CellsConnection(Sorted, leftCellsSolution.Directionality);
                            remainingSolutions.Add(compositeSolution);
                            anyDiscarded = true;


                            break;
                        }
                    }
                }
            } while (anyDiscarded);

            anyDiscarded = false;
            do
            {
                anyDiscarded = false;

                // Next check if any of our new solutions contains pieces that already have that directionality solved
                for (int ii = remainingSolutions.Count - 1; ii >= 0; ii--)
                {
                    CellsConnection thisSolution = remainingSolutions[ii];

                    foreach (Coordinate curCoordinate in remainingSolutions[ii].Cells)
                    {
                        if (this.AcceptedSolutions.TryGetValue(curCoordinate, out List<CellsConnection> existingConnections))
                        {
                            foreach (CellsConnection existingConnection in existingConnections)
                            {
                                if (existingConnection.Directionality == thisSolution.Directionality || existingConnection.Directionality == -thisSolution.Directionality)
                                {
                                    anyDiscarded = true;
                                    break;
                                }
                            }
                        }

                        if (anyDiscarded)
                        {
                            remainingSolutions.RemoveAt(ii);
                            break;
                        }
                    }
                }
            } while (anyDiscarded);

            return remainingSolutions;
        }

        public bool TryGetAllSolutionsFromCellAlongDirection(int sideIndex, Coordinate cell, DirectionalityVector offset, out CellsConnection solution, Coordinate selectedCoordinate)
        {
            solution = default;

            // If we're too close to the end direction this offset is going in, don't consider this at all
            if (!SpotIsInBounds(cell + (offset * (this.InARowToSolve - 1))))
            {
                return false;
            }

            List<CellsConnection> solutions = new List<CellsConnection>();

            bool valid = true;

            for (int ii = 0; ii < this.InARowToSolve; ii++)
            {
                Coordinate position = cell + offset * ii;

                if (!SpotIsInBounds(position))
                {
                    return false;
                }

                int? ownership = this.SpotToSideOwnership[position];

                // If this isn't the cell we're hypothetically selecting,
                // and it isn't ours already, this shape must not be valid
                if (position != selectedCoordinate && ownership != sideIndex)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
            {
                return false;
            }

            // If still valid, this must have been a solve
            List<Coordinate> solutionCells = new List<Coordinate>();
            for (int ii = 0; ii < this.InARowToSolve; ii++)
            {
                Coordinate position = cell + offset * ii;
                solutionCells.Add(position);
            }
            solution = new CellsConnection(solutionCells, offset);
            return true;
        }

        public void ApplyMoveCommand(MoveCommand toApply)
        {
            if (this.CurrentGameState == BoardState.GameStateEnum.NotStarted)
            {
                this.CurrentGameState = BoardState.GameStateEnum.Playing;
            }

            this.SpotToSideOwnership[toApply.Position] = toApply.SideIndex;

            int solutionsCount = toApply.ConnectionsMade.Count;
            this.CurrentCascadeLevel = Math.Max(this.CurrentCascadeLevel, solutionsCount);

            // Notate all accepted connections
            foreach (CellsConnection connection in toApply.ConnectionsMade)
            {
                foreach (Coordinate coordinate in connection.Cells)
                {
                    if (!this.AcceptedSolutions.TryGetValue(coordinate, out List<CellsConnection> existingConnections))
                    {
                        existingConnections = new List<CellsConnection>();
                        this.AcceptedSolutions.Add(coordinate, existingConnections);
                    }
                    existingConnections.Add(connection);
                }
            }

            // Removed knocked out players, and consider end game state
            if (toApply.PlayersRemoved.Any())
            {
                foreach (int playerRemoved in toApply.PlayersRemoved)
                {
                    // If there were no new solutions, or not enough solutions for previous cascade, and we're in cascade state,
                    // the current player should be knocked out
                    this.SideIndexesStillInGame.Remove(this.CurrentPlayerIndex);
                }

                if (this.SideIndexesStillInGame.Count == 1)
                {
                    this.CurrentPlayerIndex = this.SideIndexesStillInGame.First();
                    this.Winner = this.CurrentPlayerIndex;
                    this.CurrentGameState = BoardState.GameStateEnum.End;
                    return;
                }
            }
            else if (solutionsCount > 0)
            {
                this.CurrentGameState = BoardState.GameStateEnum.Cascade;
            }

            if (!this.AnyEmptySpots())
            {
                this.CurrentGameState = BoardState.GameStateEnum.End;
                this.Winner = null;
                return;
            }
        }

        public List<Coordinate> SortByDirectionality(IEnumerable<Coordinate> coordinates, DirectionalityVector directionality)
        {
            List<Coordinate> ordered = new List<Coordinate>(coordinates);

            ordered.Sort((Coordinate x, Coordinate y) => { return (x.X * directionality.X + x.Y * directionality.Y).CompareTo(y.X * directionality.X + y.Y * directionality.Y); });

            return ordered;
        }
    }
}
