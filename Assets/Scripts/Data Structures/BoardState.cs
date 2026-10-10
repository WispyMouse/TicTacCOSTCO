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

        public readonly GameConfiguration GameConfiguration;

        public int Height => this.GameConfiguration.Height;
        public int Width => this.GameConfiguration.Width;
        public int PlayerCount => this.GameConfiguration.PlayerCount;
        public int InARowToSolve => this.GameConfiguration.InARowToSolve;
        public int StallTurn => this.GameConfiguration.StallTurn;

        public readonly int FirstPlayerToMove;

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

        public int CurrentCascadeLevel { get; set; }
        public int CurrentPlayerIndex { get; set; }
        public HashSet<int> SideIndexesStillInGame { get; set; }
        public int CurrentRound { get; set; } = 1;

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

        public BoardState(GameConfiguration gameConfiguration, int firstPlayerToMove)
        {
            this.GameConfiguration = gameConfiguration;

            this.PossibilityContainer = new PossibilityContainer(this);

            this._SpotToSideOwnership = new Dictionary<Coordinate, int?>(Width * Height);
            for (int xx = 0; xx < Width; xx++)
            {
                for (int yy = 0; yy < Height; yy++)
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
            this.AcceptedSolutions = new Dictionary<Coordinate, List<CellsConnection>>(Width * Height);

            this.CurrentGameState = GameStateEnum.NotStarted;
            this.CurrentCascadeLevel = 0;
            this.CurrentGameState = 0;
            this.FirstPlayerToMove = firstPlayerToMove;
            this.CurrentPlayerIndex = firstPlayerToMove;
        }

        public BoardState(BoardState toDeepClone)
        {
            this.GameConfiguration = toDeepClone.GameConfiguration;

            this.CurrentCascadeLevel = toDeepClone.CurrentCascadeLevel;
            this.CurrentPlayerIndex = toDeepClone.CurrentPlayerIndex;
            this.FirstPlayerToMove = toDeepClone.FirstPlayerToMove;
            this.CurrentGameState = toDeepClone.CurrentGameState;
            this.PossibilityContainer = toDeepClone.PossibilityContainer;
            this.Winner = toDeepClone.Winner;

            this._SpotToSideOwnership = new Dictionary<Coordinate, int?>(toDeepClone.SpotToSideOwnership);
            this.SideIndexesStillInGame = new HashSet<int>(toDeepClone.SideIndexesStillInGame);

            this.AcceptedSolutions = new Dictionary<Coordinate, List<CellsConnection>>(this.AcceptedSolutions.Count);
            foreach (Coordinate coordinate in toDeepClone.AcceptedSolutions.Keys)
            {
                this.AcceptedSolutions.Add(coordinate, new List<CellsConnection>(toDeepClone.AcceptedSolutions[coordinate]));
            }
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

        /// <summary>
        /// Attempts to apply a move command.
        /// Checks the legality of the move and forbids invalid moves.
        /// TODO: Describe failures
        /// </summary>
        /// <returns>True if the command successfully applied. False otherwise.</returns>
        public bool TryApplyMoveCommand(MoveCommand toApply)
        {
            if (!this.MoveCommandLegalToPlay(toApply))
            {
                return false;
            }

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
                    this.SideIndexesStillInGame.Remove(playerRemoved);
                }

                if (this.SideIndexesStillInGame.Count == 1)
                {
                    this.CurrentPlayerIndex = this.SideIndexesStillInGame.First();
                    this.Winner = this.CurrentPlayerIndex;
                    this.CurrentGameState = BoardState.GameStateEnum.End;
                    return true;
                }
            }
            else if (solutionsCount > 0)
            {
                this.CurrentGameState = BoardState.GameStateEnum.Cascade;
            }

            if (!this.AnyLegalMovesRemainForPlayer(toApply.SideIndex, out _))
            {
                this.CurrentGameState = BoardState.GameStateEnum.End;
                this.Winner = null;

                // If there are no more empty spots, the winner is the player who most recently made a valid connection
                // We know they made a valid connection if the cascade level is above zero and they're still in the game
                // after their most recent move
                // If there haven't been any, the game is truly a draw
                if (this.CurrentCascadeLevel > 0)
                {
                    this.Winner = null;

                    for (int ii = 1; ii <= this.PlayerCount; ii++)
                    {
                        // Count backwards through players to get most recent plays
                        int playerIndex = (this.CurrentPlayerIndex - ii + this.PlayerCount) % this.PlayerCount;

                        if (!this.SideIndexesStillInGame.Contains(playerIndex))
                        {
                            continue;
                        }

                        this.Winner = playerIndex;
                    }
                }
            }

            return true;
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
                            break;
                        }
                    }

                    if (anyDiscarded)
                    {
                        if (anyDiscarded)
                        {
                            remainingSolutions.RemoveAt(ii);
                            break;
                        }
                        break;
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

        public bool StallTurnEmbargoLifted()
        {
            if (this.StallTurn <= 0)
            {
                return true;
            }

            return this.CurrentRound >= this.StallTurn;
        }

        public bool MoveCommandLegalToPlay(MoveCommand toApply)
        {
            if (!SpotIsInBounds(toApply.Position))
            {
                return false;
            }

            if (this.SpotToSideOwnership[toApply.Position].HasValue)
            {
                return false;
            }

            if (!this.StallTurnEmbargoLifted() && toApply.ConnectionsMade.Count > 0)
            {
                return false;
            }

            return true;
        }

        public bool AnyLegalMovesRemainForPlayer(int player, out IReadOnlyList<Coordinate> possiblePlays)
        {
            // No where left to play?
            if (!this.AnyEmptySpots())
            {
                possiblePlays = null;
                return false;
            }

            IReadOnlyList<Coordinate> emptyPlaces = this.GetEmptySpots();

            // Is there a "stall turn" setting? If not, then we're definitely okay to play
            if (this.StallTurnEmbargoLifted())
            {
                possiblePlays = emptyPlaces;
                return true;
            }

            // Check possible moves
            List<Coordinate> validPlaces = new List<Coordinate>(emptyPlaces.Count);

            foreach (Coordinate place in emptyPlaces)
            {
                MoveCommand command = this.GenerateCommandFromMove(player, place);

                // If it makes any connections, it can't be played there
                if (command.ConnectionsMade.Count > 0)
                {
                    continue;
                }

                validPlaces.Add(place);
            }

            // Nowhere to play
            if (validPlaces.Count == 0)
            {
                possiblePlays = null;
                return false;
            }

            possiblePlays = validPlaces;
            return true;
        }

        public IReadOnlyBoardState DeepClone()
        {
            return new BoardState(this);
        }
    }
}
