using System.Collections.Generic;
using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "Less Nearby Space Heuristic.asset", menuName = "COSTCO/AI Heuristic/Less Nearby Space")]
    public class LessEmptySpaceNearbyHeuristic : AIHeuristic
    {
        public int Radius = 1;

        public override float ScorePosition(int forSide, IReadOnlyBoardState currentGameState, Coordinate position)
        {
            int unplayableSpaces = 0;

            int expectedCoordinates = Radius * 2 - 2;
            List<Coordinate> coordinatesToCheck = new List<Coordinate>(Radius * 2 - 2);
            IReadOnlyList<Coordinate> coordinatesAround = currentGameState.PossibilityContainer.GetCoordinatesAround(currentGameState, position, this.Radius);

            // If there are areas that aren't on the grid, those are by definition unplayable
            // Imagine a query for the radius around a corner piece; it should add the missing space to the total
            unplayableSpaces += expectedCoordinates - coordinatesAround.Count;

            foreach (Coordinate resultingPosition in currentGameState.PossibilityContainer.GetCoordinatesAround(currentGameState, position, this.Radius))
            {
                if (resultingPosition == position)
                {
                    continue;
                }

                if (!currentGameState.SpotIsInBounds(resultingPosition))
                {
                    unplayableSpaces++;
                    continue;
                }

                coordinatesToCheck.Add(resultingPosition);
            }

            foreach (Coordinate curCoordinate in coordinatesToCheck)
            {
                int? ownership = currentGameState.SpotToSideOwnership[curCoordinate];
                if (ownership.HasValue)
                {
                    unplayableSpaces++;
                }
            }

            return Mathf.Lerp(0, this.Weight, (float)unplayableSpaces / BoardState.DirectionalitiesWithBackwards.Count);
        }
    }
}
