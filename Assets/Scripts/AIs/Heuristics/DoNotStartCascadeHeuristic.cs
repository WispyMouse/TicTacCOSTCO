using System.Linq;
using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "Do Not Start Cascade Heuristic.asset", menuName = "COSTCO/AI Heuristic/Do Not Start Cascade")]
    public class DoNotStartCascadeHeuristic : AIHeuristic
    {
        [Range(0, 4)]
        public int MaximimumCascadeLevel = 1;

        public override float ScorePosition(int forSide, IReadOnlyBoardState currentGameState, Coordinate position)
        {
            // We're already in Cascade, so this rule doesn't apply anymore
            if (currentGameState.CurrentGameState == BoardState.GameStateEnum.Cascade)
            {
                return 0;
            }

            // If we aren't in cascade mode, don't start cascade with this set value of cascade level or lower
            int connectionsMadeByPlayingHere = currentGameState.GenerateCommandFromMove(forSide, position).ConnectionsMade.Count;

            // If there aren't any cascades here, then apply weight
            if (connectionsMadeByPlayingHere == 0)
            {
                return this.Weight;
            }

            // If there is *enough* cascade level, then this seems fin etoo
            if (connectionsMadeByPlayingHere > this.MaximimumCascadeLevel)
            {
                return this.Weight;
            }

            // Seems like this has connections and isn't large enough, so don't apply weight
            return 0;
        }
    }
}
