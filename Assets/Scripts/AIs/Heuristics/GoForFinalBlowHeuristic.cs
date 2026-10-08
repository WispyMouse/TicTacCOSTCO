using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "Go For Final Blow Heuristic.asset", menuName = "COSTCO/AI Heuristic/Go For Final Blow Heuristic")]
    public class GoForFinalBlowHeuristic : AIHeuristic
    {
        private int _mostOpponentConnections { get; set; } = 0;

        public override void BakeInformation(int forSide, IReadOnlyBoardState currentGameState)
        {
            if (!currentGameState.StallTurnEmbargoLifted())
            {
                return;
            }

            // Determine the most number of possible connections any opponent has, anywhere
            _mostOpponentConnections = 0;

            foreach (int curSide in currentGameState.SideIndexesStillInGame)
            {
                if (curSide == forSide)
                {
                    continue;
                }

                foreach (Coordinate playableSpot in currentGameState.GetEmptySpots())
                {
                    _mostOpponentConnections = Mathf.Max(
                        _mostOpponentConnections,
                        currentGameState.GenerateCommandFromMove(curSide, playableSpot).ConnectionsMade.Count);
                }
            }
        }

        public override float ScorePosition(int forSide, IReadOnlyBoardState currentGameState, Coordinate position)
        {
            if (!currentGameState.StallTurnEmbargoLifted())
            {
                return 0;
            }

            // If this spot would create more connections than any opponent can currently make, consider this
            int creatableConnections = currentGameState.GenerateCommandFromMove(forSide, position).ConnectionsMade.Count;
            if (creatableConnections > _mostOpponentConnections)
            {
                UnityEngine.Debug.Log($"({this.name}) ({forSide}) Going for critical win against {_mostOpponentConnections} connections max");
                return this.Weight;
            }

            return 0;
        }
    }
}
