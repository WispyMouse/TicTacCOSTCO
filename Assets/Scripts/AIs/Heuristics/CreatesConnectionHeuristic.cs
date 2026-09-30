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
            int newlyCreatedConnectionSpots = 0;

            BoardState clonedState = currentGameState.DeepClone();
            clonedState.ForceMarkOwnership(position, forSide);

            foreach (Coordinate checkedPosition in currentGameState.GetEmptySpots())
            {
                int currentlyConnectionsThere = currentGameState.GetAllNewSolutions(forSide, checkedPosition).Count;
                int newConnectionsThere = clonedState.GetAllNewSolutions(forSide, checkedPosition).Count;

                newlyCreatedConnectionSpots = newConnectionsThere - currentlyConnectionsThere;
            }

            // This move created enough things
            if (newlyCreatedConnectionSpots > ConnectionsRequired)
            {
                return this.Weight;
            }

            return 0;
        }
    }
}
