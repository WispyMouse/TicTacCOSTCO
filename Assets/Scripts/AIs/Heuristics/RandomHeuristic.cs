using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "Random Heuristic.asset", menuName = "COSTCO/AI Heuristic/Random")]
    public class RandomHeuristic : AIHeuristic
    {
        public override float ScorePosition(int forSide, IReadOnlyBoardState currentGameState, Coordinate position)
        {
            return Random.Range(0, this.Weight);
        }
    }
}
