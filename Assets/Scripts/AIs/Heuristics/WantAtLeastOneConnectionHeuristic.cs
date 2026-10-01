using System.Collections.Generic;
using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "Wants At Least One Connection Heuristic.asset", menuName = "COSTCO/AI Heuristic/Wants At Least One Connection")]
    public class WantsAtLeastConnectionsHeuristic : AIHeuristic
    {
        /// <summary>
        /// Looking for connections with at least this many connections.
        /// </summary>
        [Range(0, 4)]
        public int MinimumCascadeLevel = 1;

        /// <summary>
        /// Looking for at least this many connections to apply <see cref="AIHeuristic.Weight"/>.
        /// </summary>
        [Range(0, 6)]
        public int MinimumConnections = 1;
        
        private int currentConnectionsCount { get; set; } = 0;

        public override void BakeInformation(int forSide, IReadOnlyBoardState currentGameState)
        {
            this.currentConnectionsCount = 0;

            // Determine all possible connection granting positions
            foreach (Coordinate coordinate in currentGameState.GetEmptySpots())
            {
                int createdConnections = currentGameState.GetAllNewSolutions(forSide, coordinate).Count;
                if (createdConnections >= this.MinimumCascadeLevel)
                {
                    this.currentConnectionsCount += 1;
                }
            }

#if UNITY_EDITOR
            Debug.Log($"(Side {forSide}) ({this.name}) Baked {this.currentConnectionsCount} connections");
#endif
        }

        public override float ScorePosition(int forSide, IReadOnlyBoardState currentGameState, Coordinate position)
        {
            // We've baked the information and now know if we have any existing connections
            // First question; are we already satisfied?
            if (this.currentConnectionsCount >= this.MinimumConnections)
            {
                // Yup! Nothing should get points for this
                return 0;
            }

            // Consider a version of the map where we've picked this position
            // If we do, how many connections open up?
            int connectionsAllowed = 0;

            // Check each potential connection for the position we're placing
            // then check which of these are imminently possible
            foreach (CellsConnection possibleConnection in currentGameState.PossibilityContainer.PossibleConnections[position])
            {
                bool isValid = true;
                int hits = 0;

                foreach (Coordinate connectionComponent in possibleConnection.Cells)
                {
                    // If this spot is taken by another team, we can't possibly score here
                    int? ownership = currentGameState.SpotToSideOwnership[connectionComponent];
                    if (ownership.HasValue && ownership != forSide)
                    {
                        isValid = false;
                        break;
                    }

                    // If this is the tile we're checking against, it's always at least one hit
                    if (connectionComponent == position)
                    {
                        hits++;
                    }
                    else if (ownership == forSide)
                    {
                        hits++;
                    }
                }

                // If this connection is valid, and only one away from full, then it counts
                if (isValid && hits == currentGameState.InARowToSolve - 1)
                {
                    connectionsAllowed++;
                }
            }

            // If there are enough connections, apply weight
            return (connectionsAllowed >= this.MinimumConnections) ? this.Weight : 0;
        }
    }
}
