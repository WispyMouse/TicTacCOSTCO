using System.Collections.Generic;
using UnityEngine;

public class AICore
{
    public Vector2Int DetermineMove(GameState currentGameState)
    {
        IReadOnlyList<Vector2Int> possibleMoves = currentGameState.GetEmptySpots();
        return possibleMoves[Random.Range(0, possibleMoves.Count)];
    }
}
