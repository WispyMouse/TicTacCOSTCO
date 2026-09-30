using System.Collections.Generic;
using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "Develops Origin Directionalities Heuristic.asset", menuName = "COSTCO/AI Heuristic/Develops Origin Directionalities")]
    public class DevelopsOriginDirectionalitiesHeuristic : AIHeuristic
    {
        /// <summary>
        /// In order to consider a direction "developable", there must be at least this many claimed tiles in the direction
        /// This helps with identifying easier to complete directions
        /// </summary>
        [Range(0, 3)]
        public int MinimumExistingMembers = 0;

        /// <summary>
        /// When calculating how many members are connected, only care about values up to this amount.
        /// </summary>
        [Range(0, 8)]
        public int CutoffConnections = 3;

        public override float ScorePosition(int forSide, BoardState currentGameState, Coordinate position)
        {
            // We're going to project a direction out in each of the directionalities, from this position
            // If the casted directionality is a possible development target, add to the weight
            // We're going to return the allowed weight divided by the shares
            int directionalitiesCounts = BoardState.DirectionalitiesWithBackwards.Count;
            int directionalitiesThatCouldDevelop = 0;

            foreach (DirectionalityVector directionality in BoardState.DirectionalitiesWithBackwards)
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
                        if (currentGameState.TryGetConnectionsForCoordinate(resultingPosition, out IReadOnlyCollection<CellsConnection> acceptedSolutions))
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
            }

            return Mathf.InverseLerp(0, this.CutoffConnections, directionalitiesThatCouldDevelop) * this.Weight;
        }
    }
}
