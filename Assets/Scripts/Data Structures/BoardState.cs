using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TicTacCOSTCO.DataStructures
{
    public class BoardState : IReadOnlyBoardState
    {
        public static IReadOnlyList<DirectionalityVector> Directionalities = new DirectionalityVector[]
        {
            new DirectionalityVector(1, 0),
            new DirectionalityVector(1, -1),
            new DirectionalityVector(0, -1),
            new DirectionalityVector(-1, -1),
        };

        public static IReadOnlyList<DirectionalityVector> DirectionalitiesWithBackwards = new DirectionalityVector[]
        {
            new DirectionalityVector(1, 0),
            new DirectionalityVector(1, -1),
            new DirectionalityVector(0, -1),
            new DirectionalityVector(-1, -1),

            new DirectionalityVector(-1, 0),
            new DirectionalityVector(-1, 1),
            new DirectionalityVector(0, 1),
            new DirectionalityVector(1, 1),
        };

        public readonly int Height;
        public readonly int Width;
        public readonly int PlayerCount;
        public readonly int InARowToSolve;

        public IReadOnlyDictionary<Coordinate, int?> SpotToSideOwnership => this._SpotToSideOwnership;

        int IReadOnlyBoardState.Height => this.Height;
        int IReadOnlyBoardState.Width => this.Width;
        int IReadOnlyBoardState.PlayerCount => this.PlayerCount;
        int IReadOnlyBoardState.InARowToSolve => this.InARowToSolve;
        int IReadOnlyBoardState.CurrentCascadeLevel => this.CurrentCascadeLevel;
        int IReadOnlyBoardState.CurrentPlayerIndex => this.CurrentPlayerIndex;
        IReadOnlyCollection<int> IReadOnlyBoardState.SideIndexesStillInGame => this.SideIndexesStillInGame;
        GameStateEnum IReadOnlyBoardState.CurrentGameState => this.CurrentGameState;
        PossibilityContainer IReadOnlyBoardState.PossibilityContainer => this.PossibilityContainer;

        private readonly Dictionary<Coordinate, int?> _SpotToSideOwnership;

        public int CurrentCascadeLevel;
        public int CurrentPlayerIndex;
        public HashSet<int> SideIndexesStillInGame;

        private Dictionary<Coordinate, List<CellsConnection>> AcceptedSolutions;

        public readonly PossibilityContainer PossibilityContainer;

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

        public GameStateEnum CurrentGameState;
        public int? Winner;

        public BoardState(int width, int height, int playerCount)
        {
            this.Width = width;
            this.Height = height;
            this.PlayerCount = playerCount;

            // HACK: Starting at 3 for development
            this.InARowToSolve = 3;

            this.PossibilityContainer = new PossibilityContainer(this);

            this._SpotToSideOwnership = new Dictionary<Coordinate, int?>(width * height);
            for (int xx = 0; xx < width; xx++)
            {
                for (int yy = 0; yy < height; yy++)
                {
                    this._SpotToSideOwnership.Add(new Coordinate(xx, yy), null);
                }
            }

            this.SideIndexesStillInGame = new HashSet<int>(this.PlayerCount);
            for (int ii = 0; ii < this.PlayerCount; ii++)
            {
                this.SideIndexesStillInGame.Add(ii);
            }

            this.Winner = null;
            this.AcceptedSolutions = new Dictionary<Coordinate, List<CellsConnection>>(width * height);

            this.CurrentGameState = GameStateEnum.NotStarted;
            this.CurrentCascadeLevel = 0;
            this.CurrentGameState = 0;
            this.CurrentPlayerIndex = 0;
        }

        public BoardState(int width, int height, int playerCount,
            Dictionary<Coordinate, int?> ownershipToClone, int currentCascadeLevel, int currentPlayerIndex, Dictionary<Coordinate, List<CellsConnection>> connectionsToClone,
            GameStateEnum currentStatus, int? winner, HashSet<int> sideIndexesStillInGame, PossibilityContainer possibilityContainer)
        {
            this.Width = width;
            this.Height = height;
            this.PlayerCount = playerCount;

            // HACK: Starting at 3 for development
            this.InARowToSolve = 3;

            this.CurrentCascadeLevel = currentCascadeLevel;
            this.CurrentPlayerIndex = currentPlayerIndex;
            this.CurrentGameState = currentStatus;
            this.PossibilityContainer = possibilityContainer;
            this.Winner = winner;

            this._SpotToSideOwnership = new Dictionary<Coordinate, int?>(ownershipToClone);
            this.SideIndexesStillInGame = new HashSet<int>(sideIndexesStillInGame);

            this.AcceptedSolutions = new Dictionary<Coordinate, List<CellsConnection>>(connectionsToClone.Count);
            foreach (Coordinate coordinate in connectionsToClone.Keys)
            {
                this.AcceptedSolutions.Add(coordinate, new List<CellsConnection>(connectionsToClone[coordinate]));
            }
        }

        public BoardState DeepClone()
        {
            return new BoardState(this.Width, this.Height, this.PlayerCount,
                this._SpotToSideOwnership, this.CurrentCascadeLevel, this.CurrentPlayerIndex,
                this.AcceptedSolutions, this.CurrentGameState, this.Winner, this.SideIndexesStillInGame,
                this.PossibilityContainer);
        }

        public IReadOnlyList<Coordinate> GetEmptySpots()
        {
            List<Coordinate> emptySpots = new List<Coordinate>(this.SpotToSideOwnership.Count);

            foreach (KeyValuePair<Coordinate, int?> position in this.SpotToSideOwnership)
            {
                if (position.Value == null)
                {
                    emptySpots.Add(position.Key);
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
            IReadOnlyList<CellsConnection> connections = GetAllNewSolutions(sideIndex, position);
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

        public IReadOnlyList<CellsConnection> GetAllNewSolutions(int sideIndex, Coordinate hypotheticalPosition)
        {
            List<CellsConnection> newSolutions = new List<CellsConnection>(DirectionalitiesWithBackwards.Count);

            IReadOnlyCollection<CellsConnection> possibleConnections = this.PossibilityContainer.PossibleConnections[hypotheticalPosition];

            foreach (CellsConnection possibleConnection in possibleConnections)
            {
                if (OverlayConnectionOwnedAfterPlacement(hypotheticalPosition, possibleConnection, sideIndex))
                {
                    newSolutions.Add(possibleConnection);
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

        public void ApplyMoveCommand(MoveCommand toApply)
        {
            if (this.CurrentGameState == BoardState.GameStateEnum.NotStarted)
            {
                this.CurrentGameState = BoardState.GameStateEnum.Playing;
            }

            this._SpotToSideOwnership[toApply.Position] = toApply.SideIndex;

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

        private List<CellsConnection> PruneSolutionsForNotAlreadySolved(List<CellsConnection> solutions)
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

                    if (anyDiscarded)
                    {
                        break;
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

        public static List<Coordinate> SortByDirectionality(IEnumerable<Coordinate> coordinates, DirectionalityVector directionality)
        {
            // Place randomly in to list
            List<Coordinate> ordered = new List<Coordinate>(coordinates.Distinct());

            // Then sort by likeness to directionality
            ordered.Sort((Coordinate x, Coordinate y) => { return (x.X * directionality.X + x.Y * directionality.Y).CompareTo(y.X * directionality.X + y.Y * directionality.Y); });

            return ordered;
        }

        public bool TryGetConnectionsForCoordinate(Coordinate toGet, out IReadOnlyCollection<CellsConnection> connections)
        {
            if (this.AcceptedSolutions.TryGetValue(toGet, out List<CellsConnection> writeableConnections))
            {
                connections = writeableConnections;
                return connections.Any();
            }

            connections = Array.Empty<CellsConnection>();
            return false;
        }

        public void ForceMarkOwnership(Coordinate position, int? ownership)
        {
            this._SpotToSideOwnership[position] = ownership;
        }

        public void RemoveConnection(Coordinate position, CellsConnection connection)
        {
            this.AcceptedSolutions[position].Remove(connection);
        }
    
        public bool OverlayConnectionOwned(CellsConnection connection, int side)
        {
            bool allMatch = true;

            foreach (Coordinate curCell in connection.Cells)
            {
                if (this.SpotToSideOwnership[curCell] != side)
                {
                    return false;
                }
            }

            return allMatch;
        }

        public bool OverlayConnectionOwnedAfterPlacement(Coordinate overrideCoordinate, CellsConnection connection, int side)
        {
            bool allMatch = true;

            foreach (Coordinate curCell in connection.Cells)
            {
                if (curCell == overrideCoordinate)
                {
                    continue;
                }

                if (this.SpotToSideOwnership[curCell] != side)
                {
                    return false;
                }
            }

            return allMatch;
        }
    }
}
