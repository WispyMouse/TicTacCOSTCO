using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "AIHeuristic.asset", menuName = "COSTCO/AI Heuristic/Does Not Lose")]
    public class DoesNotLoseHeuristic : AIHeuristic
    {
        public override float ScorePosition(int forSide, GameState currentGameState, Coordinate position)
        {
            // If the game isn't in the cascade state, then it doesn't matter 
            if (currentGameState.CurrentGameState != GameState.GameStateEnum.Cascade)
            {
                return 0;
            }

            int connectionsMadeByPlayingHere = currentGameState.GenerateCommandFromMove(forSide, position).NewConnectionsMade;

            // If this wouldn't make enough of a cascade, then we would lose if we pick it
            if (connectionsMadeByPlayingHere < currentGameState.CurrentCascadeLevel)
            {
                return 0;
            }

            // This move keeps us alive, so report positive for it
            return Weight;
        }
    }
}
