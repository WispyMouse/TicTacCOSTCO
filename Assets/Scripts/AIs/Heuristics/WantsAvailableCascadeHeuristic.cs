using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "Wants At Cascade Level Heuristic.asset", menuName = "COSTCO/AI Heuristic/Wants At Least Cacade Level")]
    public class WantsAvailableCascadeHeuristic : AIHeuristic
    {
        /// <summary>
        /// Add this to the cascade level for the target connection level
        /// </summary>
        public int MinimumRaisedLevel = 0;

        private int currentConnectionsCount { get; set; } = 0;

        public override void BakeInformation(int forSide, IReadOnlyBoardState currentGameState)
        {
            this.currentConnectionsCount = 0;

            // Determine all possible connection granting positions
            foreach (Coordinate coordinate in currentGameState.GetEmptySpots())
            {
                int connectionsMade = currentGameState.GetAllNewSolutions(forSide, coordinate).Count;
                if (connectionsMade >= currentGameState.CurrentCascadeLevel + MinimumRaisedLevel)
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
            if (this.currentConnectionsCount >= currentGameState.CurrentCascadeLevel)
            {
                // Yup! Nothing should get points for this
                return 0;
            }

            // Consider a version of the map where we've picked this position
            // If we do, how many connections open up?
            int connectionsPossibleFromThisPosition = 0;

            foreach (CellsConnection possibleConnection in currentGameState.PossibilityContainer.PossibleConnections[position])
            {
                bool valid = true;

                foreach (Coordinate connectionComponent in possibleConnection.Cells)
                {
                    if (connectionComponent ==  position)
                    {
                        continue;
                    }

                    if (currentGameState.SpotToSideOwnership[connectionComponent] != forSide)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    connectionsPossibleFromThisPosition++;
                }
            }

            // If there are enough connections, apply weight
            return (connectionsPossibleFromThisPosition >= currentGameState.CurrentCascadeLevel) ? this.Weight : 0;
        }
    }
}
