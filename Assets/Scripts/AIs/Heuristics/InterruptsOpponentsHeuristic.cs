using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "Interupts Opponent Heuristic.asset", menuName = "COSTCO/AI Heuristic/Interrupts Opponent")]
    public class InterruptsOpponentHeuristic : AIHeuristic
    {
        /// <summary>
        /// How many connections this needs to interrupt to apply
        /// </summary>
        public int MinimumConnections = 1;

        public override float ScorePosition(int forSide, IReadOnlyBoardState currentGameState, Coordinate position)
        {
            int connectionsOpponentsCanMake = 0;

            foreach (int playerIndex in currentGameState.SideIndexesStillInGame)
            {
                if (playerIndex == forSide)
                {
                    continue;
                }

                connectionsOpponentsCanMake += currentGameState.GetAllNewSolutions(playerIndex, position).Count;
            }

            if (connectionsOpponentsCanMake >= this.MinimumConnections)
            {
                return this.Weight;
            }
            else
            {
                return 0;
            }
        }
    }
}
