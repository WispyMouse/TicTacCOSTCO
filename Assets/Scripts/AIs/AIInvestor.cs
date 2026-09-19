using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// When considering what action to take, identify if there is a subset of actions that will not result in a lost game immediately.
/// This is not a very solid strategy, but will at least attempt to defend Cascades.
/// </summary>
[CreateAssetMenu(fileName = "AICore_Investor.asset", menuName = "COSTCO/Investor AI Core")]
public class AIInvestor : AICore
{
    public override Vector2Int DetermineMove(int forSide, GameState currentGameState)
    {
        IReadOnlyList<Vector2Int> possibleMoves = currentGameState.GetEmptySpots();

        if (currentGameState.CurrentGameState != GameState.GameStateEnum.Cascade)
        {
            // Try to invest
            List<Vector2Int> investingMoves = GetPositionsThatInvestWithoutSolving(forSide, currentGameState);
            if (investingMoves.Count > 0)
            {
                return ChooseRandomly(investingMoves);
            }

            Debug.Log($"{nameof(AIInvestor)} I couldn't find anywhere to invest in");

            // I can't possibly lose!
            return ChooseRandomly(possibleMoves);
        }

        // Oh, I might lose
        IReadOnlyList<Vector2Int> notLosingMoves = this.GetMovesThatDoNotImmediatleyLose(possibleMoves, currentGameState, forSide);

        if (notLosingMoves.Any())
        {
            return ChooseRandomly(notLosingMoves);
        }

        // Doesn't matter, then!
        Debug.Log($"{nameof(AIDoNotLoseSimple)} could not figure out any non-losing moves!");
        return ChooseRandomly(possibleMoves);
    }

    private List<Vector2Int> GetPositionsThatInvestWithoutSolving(int forSide, GameState currentGameState)
    {
        IReadOnlyList<Vector2Int> possibleMoves = currentGameState.GetEmptySpots();
        List<Vector2Int> results = new List<Vector2Int>();

        List<Vector2Int> relativeDirections = new List<Vector2Int>();
        relativeDirections.AddRange(currentGameState.directionalities);
        relativeDirections.AddRange(currentGameState.directionalities.Select(x => -x));

        foreach (Vector2Int move in possibleMoves)
        {
            // If this solves anything, we shouldn't use it
            if (currentGameState.TryGetAllSolutionsFromCell(forSide, move, out List<CellsSolution> solutions))
            {
                continue;
            }

            foreach (Vector2Int direction in relativeDirections)
            {
                // If there is a claimed tile from this side in an adjacent direction,
                // and there isn't ~the void~ or an opposing claimed tile in the opposite direction,
                // then this would be an investing move
                Vector2Int position = move + direction;
                if (position.x < 0 || position.y < 0 || position.x >= currentGameState.Width || position.y >= currentGameState.Height)
                {
                    continue;
                }
                Vector2Int back = move - direction;
                if (back.x < 0 || back.y < 0 || back.x >= currentGameState.Width || back.y >= currentGameState.Height)
                {
                    continue;
                }

                if (currentGameState.SpotToSideOwnership.TryGetValue(position, out int? claim) && claim == forSide
                    && currentGameState.SpotToSideOwnership.TryGetValue(back, out claim) && claim == null)
                {
                    Debug.Log($"{nameof(AIInvestor)} Investing in position {move} because it is adjacent to a claim with room to grow");
                    results.Add(move);
                }
            }
        }

        return results;
    }
}
