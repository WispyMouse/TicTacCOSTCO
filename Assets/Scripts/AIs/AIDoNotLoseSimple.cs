using System.Collections.Generic;
using System.Linq;
using TicTacCOSTCO.DataStructures;
using UnityEngine;

/// <summary>
/// When considering what action to take, identify if there is a subset of actions that will not result in a lost game immediately.
/// This is not a very solid strategy, but will at least attempt to defend Cascades.
/// </summary>
[CreateAssetMenu(fileName = "AICore_DoNotLoseSimple.asset", menuName = "COSTCO/Do Not Lose Simple AI Core")]
public class AIDoNotLoseSimple : AICore
{
    public override Coordinate DetermineMove(int forSide, GameState currentGameState)
    {
        IReadOnlyList<Coordinate> possibleMoves = currentGameState.GetEmptySpots();

        if (currentGameState.CurrentGameState != GameState.GameStateEnum.Cascade)
        {
            // I can't possibly lose!
            return ChooseRandomly(possibleMoves);
        }

        // Oh, I might lose
        IReadOnlyList<Coordinate> notLosingMoves = this.GetMovesThatDoNotImmediatleyLose(possibleMoves, currentGameState, forSide);

        if (notLosingMoves.Any())
        {
            return ChooseRandomly(notLosingMoves);
        }

        // Doesn't matter, then!
        Debug.Log($"{nameof(AIDoNotLoseSimple)} could not figure out any non-losing moves!");
        return ChooseRandomly(possibleMoves);
    }
}
