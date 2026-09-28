using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "DevelopsOriginDirectionalitiesHeuristic.asset", menuName = "COSTCO/AI Heuristic/Develops Origin Directionalities")]
    public class DevelopsOriginDirectionalitiesHeuristic : AIHeuristic
    {
        public override float ScorePosition(int forSide, GameState currentGameState, Coordinate position)
        {
            // We're going to project a direction out in each of the directionalities, from this position
            // If the casted directionality is a possible development target, add to the weight
            // We're going to return the allowed weight divided by the shares
            int directionalitiesCounts = currentGameState.Directionalities.Count * 2;
            int directionalitiesThatCouldDevelop = 0;

            foreach (DirectionalityVector directionality in currentGameState.Directionalities)
            {
                bool forwardValid = true;

                for (int ii = 1; ii < currentGameState.InARowToSolve; ii++)
                {
                    Coordinate resultingPosition = position + directionality * ii;

                    if (!currentGameState.SpotIsInBounds(resultingPosition))
                    {
                        forwardValid = false;
                        break;
                    }

                    int? ownership = currentGameState.SpotToSideOwnership[resultingPosition];

                    // If it's not empty, or not owned by us, it doesn't work
                    if (!(ownership == null || ownership == forSide))
                    {
                        forwardValid = false;
                        break;
                    }
                }

                if (forwardValid)
                {
                    directionalitiesThatCouldDevelop++;
                }

                bool backwardValid = true;
                Coordinate opposite = directionality * -1;

                for (int ii = 1; ii < currentGameState.InARowToSolve; ii++)
                {
                    Coordinate resultingPosition = position + opposite * ii;

                    if (!currentGameState.SpotIsInBounds(resultingPosition))
                    {
                        backwardValid = false;
                        break;
                    }

                    int? ownership = currentGameState.SpotToSideOwnership[resultingPosition];

                    // If it's not empty, or not owned by us, it doesn't work
                    if (!(ownership == null || ownership == forSide))
                    {
                        backwardValid = false;
                        break;
                    }
                }

                if (backwardValid)
                {
                    directionalitiesThatCouldDevelop++;
                }
            }

            return (directionalitiesThatCouldDevelop / directionalitiesCounts) * this.Weight;
        }
    }
}
