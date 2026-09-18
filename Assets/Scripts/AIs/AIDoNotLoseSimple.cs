using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When considering what action to take, identify if there is a subset of actions that will not result in a lost game immediately.
/// This is not a very solid strategy, but will at least attempt to defend Cascades.
/// </summary>
[CreateAssetMenu(fileName = "AICore_DoNotLoseSimple.asset", menuName = "COSTCO/Do Not Lose Simple AI Core")]
public class AIDoNotLoseSimple : AICore
{
    public override Vector2Int DetermineMove(int forSide, GameState currentGameState)
    {
        IReadOnlyList<Vector2Int> possibleMoves = currentGameState.GetEmptySpots();

        if (currentGameState.CurrentGameState != GameState.GameStateEnum.Cascade)
        {
            // I can't possibly lose!
            return possibleMoves[Random.Range(0, possibleMoves.Count)];
        }

        // Oh, I might lose
        List<Vector2Int> notLosingMoves = new List<Vector2Int>(possibleMoves.Count);

        foreach (Vector2Int move in possibleMoves)
        {
            if (currentGameState.TryGetAllSolutionsFromCell(forSide, move, out List<CellsSolution> solutions))
            {
                if (solutions.Count >= currentGameState.LastCascade)
                {
                    notLosingMoves.Add(move);
                }
            }
        }

        if (notLosingMoves.Count == 0)
        {
            // Doesn't matter, then!
            Debug.Log($"{nameof(AIDoNotLoseSimple)} could not figure out any non-losing moves!");
            return possibleMoves[Random.Range(0, possibleMoves.Count)];
        }

        return notLosingMoves[Random.Range(0, notLosingMoves.Count)];
    }
}
