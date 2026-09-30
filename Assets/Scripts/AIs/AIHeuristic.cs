using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    using TicTacCOSTCO.DataStructures;
    using UnityEngine;

    // [CreateAssetMenu(fileName = "AIHeuristic.asset", menuName = "COSTCO/AI Heuristic")]
    public abstract class AIHeuristic : ScriptableObject
    {
        public float Weight = 1f;

        public abstract float ScorePosition(int forSide, IReadOnlyBoardState currentGameState, Coordinate position);
        public virtual void BakeInformation(int forSide, IReadOnlyBoardState currentGameState)
        {

        }
    }
}
