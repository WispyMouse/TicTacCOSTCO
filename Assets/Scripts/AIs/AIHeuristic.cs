using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    using System.Collections.Generic;
    using TicTacCOSTCO.AIs;
    using TicTacCOSTCO.DataStructures;
    using TicTacCOSTCO.DataStructures.Tools;
    using UnityEngine;

    // [CreateAssetMenu(fileName = "AIHeuristic.asset", menuName = "COSTCO/AI Heuristic")]
    public abstract class AIHeuristic : ScriptableObject
    {
        public float Weight = 1f;

        public abstract float ScorePosition(int forSide, GameState currentGameState, Coordinate position);
    }
}
