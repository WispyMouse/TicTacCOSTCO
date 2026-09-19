using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AICore.asset", menuName = "COSTCO/AI Core")]
public class AICore : ScriptableObject
{
    public virtual Vector2Int DetermineMove(int forSide, GameState currentGameState)
    {
        IReadOnlyList<Vector2Int> possibleMoves = currentGameState.GetEmptySpots();
        return ChooseRandomly(possibleMoves);
    }

    protected virtual Vector2Int ChooseRandomly(IReadOnlyList<Vector2Int> options)
    {
        return options[Random.Range(0, options.Count)];
    }

    protected IReadOnlyList<Vector2Int> GetMovesThatDoNotImmediatleyLose(IReadOnlyList<Vector2Int> options, GameState currentGameState, int forSide)
    {
        List<Vector2Int> notLosingMoves = new List<Vector2Int>(options.Count);

        foreach (Vector2Int move in options)
        {
            int solutionCountFromMove = HypotheticalSolutionTool.GetSolutionsFromClaimingTile(currentGameState, move, forSide).Count;
            if (solutionCountFromMove >= currentGameState.LastCascade)
            {
                notLosingMoves.Add(move);
            }
        }

        return notLosingMoves;
    }
}
