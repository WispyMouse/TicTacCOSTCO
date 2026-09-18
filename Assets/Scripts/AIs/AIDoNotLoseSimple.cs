using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When considering what action to take, identify if there is a subset of actions that will not result in a lost game immediately.
/// This is not a very solid strategy, but will at least attempt to defend Cascades.
/// </summary>
[CreateAssetMenu(fileName = "AICore_DoNotLoseSimple.asset", menuName = "COSTCO/Do Not Lose Simple AI Core")]
public class AIDoNotLoseSimple : ScriptableObject
{
    public Vector2Int DetermineMove(GameState currentGameState)
    {
        IReadOnlyList<Vector2Int> possibleMoves = currentGameState.GetEmptySpots();
        return possibleMoves[Random.Range(0, possibleMoves.Count)];
    }
}
