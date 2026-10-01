using System.Collections.Generic;
using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "Near Other Players Heuristic.asset", menuName = "COSTCO/AI Heuristic/Near Other Players")]
    public class NearOtherPlayersHeuristic : AIHeuristic
    {
        /// <summary>
        /// If there are more than this number of occupied neighbors, rate at 0.
        /// If there are this amount or fewer, rate at weight.
        /// </summary>
        public int MinimumNeighbors = 1;

        /// <summary>
        /// How distant to check
        /// </summary>
        public int Radius = 1;

        public override float ScorePosition(int forSide, IReadOnlyBoardState currentGameState, Coordinate position)
        {
            int occupiedNeighbors = 0;

            List<Coordinate> coordinatesToCheck = new List<Coordinate>(Radius * Radius);

            foreach (Coordinate resultingPosition in currentGameState.PossibilityContainer.GetCoordinatesAround(currentGameState, position, this.Radius))
            {
                if (!currentGameState.SpotIsInBounds(resultingPosition))
                {
                    continue;
                }

                int? ownership = currentGameState.SpotToSideOwnership[resultingPosition];
                if (ownership.HasValue && ownership != forSide)
                {
                    occupiedNeighbors++;
                }
            }

            // Congested enough, ship it
            if (occupiedNeighbors >= MinimumNeighbors)
            {
                return Weight;
            }

            return 0;
        }
    }
}
