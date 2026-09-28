namespace TicTacCOSTCO.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using TicTacCOSTCO.DataStructures;
    using TicTacCOSTCO.DataStructures.Tools;
    using Unity.VisualScripting;

    public class GameState
    {
        public const int MINIMUMPLAYERS = 2;

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
            /// Once in Cascade, players need to make at least <see cref="LastCascade"/>
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

        public GameStateEnum CurrentGameState { get; private set; } = GameStateEnum.NotStarted;
        public int? Winner { get; private set; } = null;

        public IReadOnlyList<DirectionalityVector> Directionalities = new DirectionalityVector[]
        {
            new DirectionalityVector(1, 0),
            new DirectionalityVector(1, -1),
            new DirectionalityVector(0, -1),
            new DirectionalityVector(-1, -1),
        };

        public readonly Dictionary<Coordinate, int?> SpotToSideOwnership = new Dictionary<Coordinate, int?>();

        public int LastCascade { get; set; } = 0;


        public readonly int Height;
        public readonly int Width;
        public readonly int PlayerCount;

        public int InARowToSolve = 3;

        public int CurrentPlayerIndex { get; set; } = 0;
        public HashSet<int> SideIndexesStillInGame = new HashSet<int>();

        public delegate void OnPlayerTurnDelegate(int turn);
        public OnPlayerTurnDelegate OnPlayerStartTurn;
        public OnPlayerTurnDelegate OnPlayerMadeMove;

        public IReadOnlyList<MoveCommand> MoveCommandsApplied => this._MoveCommandsApplied;
        private List<MoveCommand> _MoveCommandsApplied { get; set; } = new List<MoveCommand>();

        public Dictionary<Coordinate, List<CellsConnection>> AcceptedSolutions { get; set; } = new Dictionary<Coordinate, List<CellsConnection>>();

        public GameState(int width, int height, int playerCount, bool forceZeroIndexStart = false)
        {
            this.PlayerCount = playerCount;
            this.Width = width;
            this.Height = height;

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

            if (forceZeroIndexStart)
            {
                // Force the first player to be zero, perhaps because we're in test mode
                this.CurrentPlayerIndex = 0;
            }
            else
            {
                // Choose a random player to go first
                this.CurrentPlayerIndex = new Random().Next(this.PlayerCount);
            }
        }

        public IReadOnlyList<Coordinate> GetEmptySpots()
        {
            List<Coordinate> emptySpots = new List<Coordinate>();

            foreach (Coordinate position in SpotToSideOwnership.Keys)
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
            foreach (Coordinate position in SpotToSideOwnership.Keys)
            {
                int? ownership = SpotToSideOwnership[position];

                if (ownership == null)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetAllSolutionsFromCell(int sideIndex, Coordinate cell, out List<CellsConnection> solutionsInvolvingCell)
        {
            solutionsInvolvingCell = new List<CellsConnection>();

            foreach (CellsConnection solution in GetAllNewSolutions(sideIndex, cell))
            {
                if (solution.Cells.Contains(cell))
                {
                    solutionsInvolvingCell.Add(solution);
                }
            }

            return solutionsInvolvingCell.Any();
        }

        public bool TryGetAllSolutionsFromCellAlongDirection(int sideIndex, Coordinate cell, DirectionalityVector offset, out CellsConnection solution, Coordinate selectedCoordinate)
        {
            solution = default;

            // If we're too close to the end direction this offset is going in, don't consider this at all
            if (!SpotIsInBounds(cell + (offset * (InARowToSolve - 1))))
            {
                return false;
            }

            List<CellsConnection> solutions = new List<CellsConnection>();

            bool valid = true;

            for (int ii = 0; ii < InARowToSolve; ii++)
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
            for (int ii = 0; ii < InARowToSolve; ii++)
            {
                Coordinate position = cell + offset * ii;
                solutionCells.Add(position);
            }
            solution = new CellsConnection(solutionCells, offset);
            return true;
        }

        public List<CellsConnection> GetAllNewSolutions(int sideIndex, Coordinate hypotheticalPosition)
        {
            List<CellsConnection> newSolutions = new List<CellsConnection>();

            for (int xx = 0; xx < this.Width; xx++)
            {
                for (int yy = 0; yy < this.Height; yy++)
                {
                    Coordinate position = new Coordinate(xx, yy);

                    if (hypotheticalPosition != position && !SpotToSideOwnership[position].HasValue && SpotToSideOwnership[position] != sideIndex)
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

            return PruneSolutionsForNotAlreadySolved(newSolutions, out _);
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

        public List<CellsConnection> PruneSolutionsForNotAlreadySolved(List<CellsConnection> solutions, out List<CellsConnection> removedConnections)
        {
            List<CellsConnection> remainingSolutions = new List<CellsConnection>(solutions);
            removedConnections = new List<CellsConnection>();

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
                            removedConnections.Add(rightCellsSolution);
                            removedConnections.Add(leftCellsSolution);

                            // Add a new composite solution to the end of the list, which won't be evaluated again
                            CellsConnection compositeSolution = new CellsConnection(leftCellsSolution.Cells.Union(rightCellsSolution.Cells).ToList(), leftCellsSolution.Directionality);
                            remainingSolutions.Add(compositeSolution);
                            anyDiscarded = true;


                            break;
                        }
                    }
                }
            } while (anyDiscarded);

            // Next check if any of our new solutions contains pieces that already have that directionality solved
            for (int ii = remainingSolutions.Count - 1; ii >= 0; ii--)
            {
                bool discard = false;
                CellsConnection thisSolution = remainingSolutions[ii];

                foreach (Coordinate curCoordinate in remainingSolutions[ii].Cells)
                {
                    if (this.AcceptedSolutions.TryGetValue(curCoordinate, out List<CellsConnection> existingConnections))
                    {
                        foreach (CellsConnection existingConnection in existingConnections)
                        {
                            if (existingConnection.Directionality == thisSolution.Directionality)
                            {
                                discard = true;
                                break;
                            }
                        }
                    }

                    if (discard)
                    {
                        remainingSolutions.RemoveAt(ii);
                        break;
                    }
                }
            }

            return remainingSolutions;
        }

        public void ApplyMoveCommand(MoveCommand toApply, bool advancePlayer = true)
        {
            if (this.CurrentGameState == GameStateEnum.NotStarted)
            {
                this.CurrentGameState = GameStateEnum.Playing;
            }

            this._MoveCommandsApplied.Add(toApply);

            this.SpotToSideOwnership[toApply.Position] = toApply.SideIndex;

            int solutionsCount = toApply.NewConnectionsMade;

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

            // HACK SHOULD GENERALIZE: Would this move result in a loss
            if (this.CurrentGameState == GameState.GameStateEnum.Cascade && this.LastCascade > solutionsCount)
            {
                // If there were no new solutions, or not enough solutions for previous cascade, and we're in cascade state,
                // the current player should be knocked out
                this.SideIndexesStillInGame.Remove(this.CurrentPlayerIndex);

                if (this.SideIndexesStillInGame.Count == 1)
                {
                    this.CurrentPlayerIndex = this.SideIndexesStillInGame.First();
                    this.Winner = this.CurrentPlayerIndex;
                    this.CurrentGameState = GameStateEnum.End;
                    return;
                }
            }
            else if (solutionsCount > 0)
            {
                this.CurrentGameState = GameState.GameStateEnum.Cascade;
                this.LastCascade = solutionsCount;
            }

            if (!this.AnyEmptySpots())
            {
                this.CurrentGameState = GameStateEnum.End;
                this.Winner = null;
                return;
            }

            if (advancePlayer)
            {
                AdvancePlayer();
            }
        }

        public MoveCommand ReversePreviousMoveCommand()
        {
            int moveCommandsApplied = this._MoveCommandsApplied.Count;
            if (moveCommandsApplied == 0)
            {
                return null;
            }

            MoveCommand toRemove = this.MoveCommandsApplied[moveCommandsApplied - 1];
            this._MoveCommandsApplied.RemoveAt(moveCommandsApplied - 1);

            // HACK: At this point in the game's development, the only thing that *could* have been here is null
            this.SpotToSideOwnership[toRemove.Position] = null;
            this.LastCascade = toRemove.PreviousCascade;

            foreach (int playerRemoved in toRemove.PlayersRemoved)
            {
                this.SideIndexesStillInGame.Add(playerRemoved);
            }

            foreach (CellsConnection connectionsAdded in toRemove.ConnectionsMade)
            {
                foreach (Coordinate coordinate in connectionsAdded.Cells)
                {
                    this.AcceptedSolutions[coordinate].Remove(connectionsAdded);
                }
            }

            foreach (CellsConnection connectionRemoved in toRemove.ConnectionsRemoved)
            {
                foreach (Coordinate coordinate in connectionRemoved.Cells)
                {
                    this.AcceptedSolutions[coordinate].Add(connectionRemoved);
                }
            }

            this.AdvancePlayer(reversePlayer: true);
            return toRemove;
        }

        public MoveCommand GenerateCommandFromMove(int sideIndex, Coordinate position)
        {
            List<CellsConnection> connections = GetAllNewSolutions(sideIndex, position);
            List<int> playersRemoved = new List<int>();

            connections = PruneSolutionsForNotAlreadySolved(connections, out List<CellsConnection> removedConnections);

            // HACK SHOULD GENERALIZE: Would this move result in a loss
            if (this.CurrentGameState == GameState.GameStateEnum.Cascade && this.LastCascade > connections.Count)
            {
                playersRemoved.Add(sideIndex);
            }

            return new MoveCommand(sideIndex, position, connections, this.LastCascade, playersRemoved, removedConnections);
        }

        public void AdvancePlayer(bool reversePlayer = false)
        {
            this.OnPlayerMadeMove?.Invoke(this.CurrentPlayerIndex);

            // If we're advancing, add one. Reversing, minus one
            // This will modulo around the player count and let us find the next valid player
            int advancer = reversePlayer ? -1 : 1;

            for (int ii = 1; ii < this.PlayerCount; ii++)
            {
                int nextProspectivePlayer = (this.CurrentPlayerIndex + advancer + this.PlayerCount) % this.PlayerCount;
                if (!this.SideIndexesStillInGame.Contains(nextProspectivePlayer))
                {
                    continue;
                }

                this.CurrentPlayerIndex = nextProspectivePlayer;
                this.OnPlayerStartTurn?.Invoke(this.CurrentPlayerIndex);
                break;
            }
        }
    }
}
