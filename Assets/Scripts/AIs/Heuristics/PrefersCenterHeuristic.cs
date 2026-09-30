using TicTacCOSTCO.DataStructures;
using UnityEngine;

namespace TicTacCOSTCO.AIs
{
    [CreateAssetMenu(fileName = "Prefer Center Heuristic.asset", menuName = "COSTCO/AI Heuristic/Prefer Center")]
    public class PrefersCenterHeuristic : AIHeuristic
    {
        int maxX;
        int maxY;
        Coordinate centerish;

        public override void BakeInformation(int forSide, IReadOnlyBoardState currentGameState)
        {
            // Integer division rounds, there might not be an exact center
            maxX = currentGameState.Width / 2;
            maxY = currentGameState.Height / 2;

            centerish = new Coordinate(maxX, maxY);
        }

        public override float ScorePosition(int forSide, IReadOnlyBoardState currentGameState, Coordinate position)
        {
            return Weight * (2 - ((Mathf.Abs(centerish.Y - position.Y) / (float)(maxX)) + (Mathf.Abs(centerish.X - position.X) / (float)(maxY))));
        }
    }
}
