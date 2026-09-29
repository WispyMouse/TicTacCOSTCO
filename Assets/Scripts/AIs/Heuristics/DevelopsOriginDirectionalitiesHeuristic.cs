using System.Collections.Generic;
using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "DevelopsOriginDirectionalitiesHeuristic.asset", menuName = "COSTCO/AI Heuristic/Develops Origin Directionalities")]
    public class DevelopsOriginDirectionalitiesHeuristic : AIHeuristic
    {
        /// <summary>
        /// In order to consider a direction "developable", there must be at least this many claimed tiles in the direction
        /// This helps with identifying easier to complete directions
        /// </summary>
        [Range(0, 3)]
        public int MinimumExistingMembers = 0;

        public override float ScorePosition(int forSide, BoardState currentGameState, Coordinate position)
        {
            // We're going to project a direction out in each of the directionalities, from this position
            // If the casted directionality is a possible development target, add to the weight
            // We're going to return the allowed weight divided by the shares
            int directionalitiesCounts = currentGameState.Directionalities.Count * 2;
            int directionalitiesThatCouldDevelop = 0;

            foreach (DirectionalityVector directionality in currentGameState.Directionalities)
            {
                bool IsDirectionValid(DirectionalityVector direction)
                {
                    bool valid = true;
                    int ownedInQuery = 0;

                    for (int ii = 1; ii < currentGameState.InARowToSolve; ii++)
                    {
                        Coordinate resultingPosition = position + direction * ii;

                        if (!currentGameState.SpotIsInBounds(resultingPosition))
                        {
                            valid = false;
                            break;
                        }

                        int? ownership = currentGameState.SpotToSideOwnership[resultingPosition];

                        // If it's not empty, or not owned by us, it doesn't work
                        if (!(ownership == null || ownership == forSide))
                        {
                            valid = false;
                            break;
                        }

                        if (ownership == forSide)
                        {
                            ownedInQuery++;
                        }

                        // Check to see if that tile has 
                        if (currentGameState.AcceptedSolutions.TryGetValue(resultingPosition, out List<CellsConnection> acceptedSolutions))
                        {
                            foreach (CellsConnection connection in acceptedSolutions)
                            {
                                if (connection.Directionality == direction)
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }
                    }

                    return valid && ownedInQuery >= this.MinimumExistingMembers;
                }

                if (IsDirectionValid(directionality))
                {
                    directionalitiesThatCouldDevelop++;
                }
                if (IsDirectionValid(-directionality))
                {
                    directionalitiesThatCouldDevelop++;
                }
            }

            return ((float)directionalitiesThatCouldDevelop / (float)directionalitiesCounts) * this.Weight;
        }
    }
}
