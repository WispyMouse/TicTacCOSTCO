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
                        unplayableSpaces++;
                        continue;
                    }

                    coordinatesToCheck.Add(resultingCoordinate);
                }
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
