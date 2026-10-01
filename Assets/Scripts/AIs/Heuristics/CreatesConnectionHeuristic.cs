using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "Creates Connection Heuristic.asset", menuName = "COSTCO/AI Heuristic/Creates Connection")]
    public class CreatesConnectionHeuristic : AIHeuristic
    {
        public int ConnectionsRequired = 1;

        public override float ScorePosition(int forSide, IReadOnlyBoardState currentGameState, Coordinate position)
        {
            int connectionsMade = 0;

            foreach (CellsConnection possibleConnection in currentGameState.PossibilityContainer.PossibleConnections[position])
            {
                foreach (Coordinate connectionComponent in possibleConnection.Cells)
                {
                    // We only want tiles owned by this team
                    int? ownership = currentGameState.SpotToSideOwnership[connectionComponent];
                    if (ownership != forSide)
                    {
                        break;
                    }

                    connectionsMade++;

                    if (connectionsMade >= this.ConnectionsRequired)
                    {
                        return this.Weight;
                    }
                }
            }

            return 0;
        }
    }
}
