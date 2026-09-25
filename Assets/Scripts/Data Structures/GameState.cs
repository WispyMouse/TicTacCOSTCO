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
        public OnPlayerTurnDelegate OnPlayerTurn;

        public Dictionary<Coordinate, List<CellsConnection>> AcceptedSolutions { get; set; } = new Dictionary<Coordinate, List<CellsConnection>>();

        public GameState(int width, int height, int playerCount)
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

            // Choose a random player to go first
            this.CurrentPlayerIndex = new Random().Next(this.PlayerCount);
        }

        /// <summary>
        /// Sets the ownership of a coordinate.
        /// </summary>
        /// <param name="position">Coordinate to mark.</param>
        /// <param name="side">Side to set. Null is "empty and unclaimed", otherwise it is the side's <see cref="CurrentPlayerIndex"/>.</param>
        /// <param name="newSolutions">New solutions stemming from this move.</param>
        /// <param name="advancePlayer">If true, make it the next player's turn. Otherwise, don't touch the current player index.</param>
        /// 
        public void SetSideOwnership(Coordinate position, int? side, out List<CellsConnection> newSolutions, bool advancePlayer = true)
        {
            if (side.HasValue)
            {
                newSolutions = HypotheticalSolutionTool.GetSolutionsFromClaimingTile(this, position, side.Value);

                foreach (CellsConnection newSolution in newSolutions)
                {
                    foreach (Coordinate cellPosition in newSolution.Cells)
                    {
                        if (!this.AcceptedSolutions.TryGetValue(cellPosition, out List<CellsConnection> existingSolutionsForCell))
                        {
                            existingSolutionsForCell = new List<CellsConnection>();
                            this.AcceptedSolutions.Add(cellPosition, existingSolutionsForCell);
                        }

                        existingSolutionsForCell.Add(newSolution);
                    }
                }
            }
            else
            {
                newSolutions = new List<CellsConnection>();
            }

            this.SpotToSideOwnership[position] = side;

            int solutionsCount = newSolutions.Count;

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
                for (int ii = 1; ii < this.PlayerCount; ii++)
                {
                    int nextProspectivePlayer = (this.CurrentPlayerIndex + ii) % this.PlayerCount;
                    if (!this.SideIndexesStillInGame.Contains(nextProspectivePlayer))
                    {
                        continue;
                    }

                    this.CurrentPlayerIndex = nextProspectivePlayer;
                    this.OnPlayerTurn?.Invoke(this.CurrentPlayerIndex);
                    break;
                }
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

            foreach (CellsConnection solution in GetAllSolutions(sideIndex, cell))
            {
                if (solution.Cells.Contains(cell))
                {
                    solutionsInvolvingCell.Add(solution);
                }
            }

            return solutionsInvolvingCell.Any();
        }

        public bool TryGetAllSolutionsFromCellAlongDirection(int sideIndex, Coordinate cell, DirectionalityVector offset, out CellsConnection solution)
        {
            solution = default;

            // If we're too close to the end direction this offset is going in, don't consider this at all
            if (SpotIsInBounds(cell + (offset * (InARowToSolve - 1))))
            {
                return false;
            }

            if (SpotIsInBounds(cell - (offset * (InARowToSolve - 1))))
            {
                return false;
            }

            List<CellsConnection> solutions = new List<CellsConnection>();

            bool valid = true;

            for (int ii = 1; ii < InARowToSolve; ii++)
            {
                Coordinate position = cell + offset * ii;

                int? ownership = this.SpotToSideOwnership[position];

                // This cell isn't ours
                if (ownership != sideIndex)
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


        public List<CellsConnection> GetAllSolutions()
        {
            List<CellsConnection> solutions = new List<CellsConnection>();

            for (int xx = 0; xx < this.Width; xx++)
            {
                for (int yy = 0; yy < this.Height; yy++)
                {
                    Coordinate position = new Coordinate(xx, yy);

                    if (!SpotToSideOwnership[position].HasValue)
                    {
                        // This cell isn't claimed
                        continue;
                    }

                    foreach (DirectionalityVector direction in Directionalities)
                    {
                        if (TryGetAllSolutionsFromCellAlongDirection(SpotToSideOwnership[position].Value, position, direction, out CellsConnection cellSolutions))
                        {
                            solutions.Add(cellSolutions);
                        }
                    }

                }
            }

            // Check if any new solutions should be banded together; 4-in-a-row is the same value as a 3-in-a-row
            // Any solutions that have the same directionality *must* be bandable
            bool anyDiscarded = false;

            do
            {
                anyDiscarded = false;
                for (int leftSolutionIndex = solutions.Count - 2; leftSolutionIndex >= 0; leftSolutionIndex--)
                {
                    bool discardLeftSolution = false;
                    CellsConnection leftCellsSolution = solutions[leftSolutionIndex];
                    for (int rightSolutionIndex = solutions.Count - 1; rightSolutionIndex > leftSolutionIndex; rightSolutionIndex--)
                    {
                        CellsConnection rightCellsSolution = solutions[rightSolutionIndex];

                        if (leftCellsSolution.Directionality == rightCellsSolution.Directionality)
                        {
                            // Add a new composite solution to the end of the list, which won't be evaluated again
                            CellsConnection compositeSolution = new CellsConnection(leftCellsSolution.Cells.Union(rightCellsSolution.Cells).ToList(), leftCellsSolution.Directionality);
                            solutions.Add(compositeSolution);
                            discardLeftSolution = true;
                            anyDiscarded = true;

                            // We can immediately discard this rightSolution
                            solutions.RemoveAt(rightSolutionIndex);
                        }
                    }

                    if (discardLeftSolution)
                    {
                        solutions.RemoveAt(leftSolutionIndex);
                    }
                }
            } while (anyDiscarded);

            return solutions;
        }


        public List<CellsConnection> GetAllSolutions(int sideIndex, Coordinate hypotheticalPosition)
        {
            List<CellsConnection> solutions = new List<CellsConnection>();

            for (int xx = 0; xx < this.Width; xx++)
            {
                for (int yy = 0; yy < this.Height; yy++)
                {
                    Coordinate position = new Coordinate(xx, yy);

                    if (hypotheticalPosition != position && !SpotToSideOwnership[position].HasValue)
                    {
                        // This cell isn't claimed
                        continue;
                    }

                    foreach (DirectionalityVector direction in Directionalities)
                    {
                        if (TryGetAllSolutionsFromCellAlongDirection(sideIndex, position, direction, out CellsConnection cellSolutions))
                        {
                            solutions.Add(cellSolutions);
                        }
                    }

                }
            }

            // Check if any new solutions should be banded together; 4-in-a-row is the same value as a 3-in-a-row
            // Any solutions that have the same directionality *must* be bandable
            bool anyDiscarded = false;

            do
            {
                anyDiscarded = false;
                for (int leftSolutionIndex = solutions.Count - 2; leftSolutionIndex >= 0; leftSolutionIndex--)
                {
                    bool discardLeftSolution = false;
                    CellsConnection leftCellsSolution = solutions[leftSolutionIndex];
                    for (int rightSolutionIndex = solutions.Count - 1; rightSolutionIndex > leftSolutionIndex; rightSolutionIndex--)
                    {
                        CellsConnection rightCellsSolution = solutions[rightSolutionIndex];

                        if (leftCellsSolution.Directionality == rightCellsSolution.Directionality)
                        {
                            // Add a new composite solution to the end of the list, which won't be evaluated again
                            CellsConnection compositeSolution = new CellsConnection(leftCellsSolution.Cells.Union(rightCellsSolution.Cells).ToList(), leftCellsSolution.Directionality);
                            solutions.Add(compositeSolution);
                            discardLeftSolution = true;
                            anyDiscarded = true;

                            // We can immediately discard this rightSolution
                            solutions.RemoveAt(rightSolutionIndex);
                        }
                    }

                    if (discardLeftSolution)
                    {
                        solutions.RemoveAt(leftSolutionIndex);
                    }
                }
            } while (anyDiscarded);

            return solutions;
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

            // Remove the parts that already have this same directionality solved, and only if there are enough pieces should we keep this
            // Otherwise "4-in-a-rows" count as two
            for (int solutionIndex = solutions.Count - 1; solutionIndex >= 0; solutionIndex--)
            {
                for (int cellIndex = 0; cellIndex < solutions[solutionIndex].Cells.Count; cellIndex++)
                {
                    bool removeSolution = false;
                    if (this.AcceptedSolutions.TryGetValue(solutions[solutionIndex].Cells[cellIndex], out List<CellsConnection> existingSolutions))
                    {
                        foreach (CellsConnection solution in existingSolutions)
                        {
                            if (solution.Directionality == solutions[solutionIndex].Directionality)
                            {
                                removeSolution = true;
                                break;
                            }
                        }
                    }
                    if (removeSolution)
                    {
                        remainingSolutions.RemoveAt(solutionIndex);
                        break;
                    }
                }
            }

            return remainingSolutions;
        }
    }

}
