using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AICore.asset", menuName = "COSTCO/AI Core")]
public class AICore : ScriptableObject
{
    public Vector2Int DetermineMove(GameState currentGameState)
    {
        IReadOnlyList<Vector2Int> possibleMoves = currentGameState.GetEmptySpots();
        return possibleMoves[Random.Range(0, possibleMoves.Count)];
    }
}
