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

            for (int xx = -Radius; xx < Radius; xx++)
            {
                for (int yy = -Radius; yy < Radius; yy++)
                {
                    Coordinate resultingCoordinate = new Coordinate(xx, yy) + position;

                    if (resultingCoordinate == position)
                    {
                        continue;
                    }

                    if (!currentGameState.SpotIsInBounds(resultingCoordinate))
                    {
                        continue;
                    }

                    coordinatesToCheck.Add(resultingCoordinate);
                }
            }

            foreach (Coordinate resultingPosition in coordinatesToCheck)
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
