using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;

/// <summary>
/// When it is possible to make a connection, consider a % chance of taking it.
/// </summary>
[CreateAssetMenu(fileName = "AICore_MayAttack.asset", menuName = "COSTCO/May Attack AI Core")]
public class AIMayAttack : AICore
{
    [Range(0, 1f)]
    public float OneConnectionChance = .2f;

    [Range(0, 1f)]
    public float TwoConnectionChance = .9f;

    [Range(0, 1f)]
    public float ThreeOrMoreConnectionChance = 1f;

    public override Vector2Int DetermineMove(int forSide, GameState currentGameState)
    {
        IReadOnlyList<Vector2Int> possibleMoves = currentGameState.GetEmptySpots();

        float connectionChanceAttackRoll = Random.Range(0, 1f);
        List<Vector2Int> oneConnectionAttacks = new List<Vector2Int>();
        List<Vector2Int> twoConnectionAttacks = new List<Vector2Int>();
        List<Vector2Int> moreConnectionAttacks = new List<Vector2Int>();
        foreach (Vector2Int possibleMove in possibleMoves)
        {
            int solutionCounts = HypotheticalSolutionTool.GetSolutionsFromClaimingTile(currentGameState, possibleMove, forSide).Count;

            // Only count solutions that won't lose to the current cascade
            if (currentGameState.LastCascade > solutionCounts)
            {
                continue;
            }

            // ladder of attacks!
            // Identify how many solutions stem from one piece
            if (solutionCounts >= 1)
            {
                oneConnectionAttacks.Add(possibleMove);

                if (solutionCounts >= 2)
                {
                    twoConnectionAttacks.Add(possibleMove);

                    if (solutionCounts >= 3)
                    {
                        moreConnectionAttacks.Add(possibleMove);
                    }
                }
            }
        }

        if (moreConnectionAttacks.Any() && connectionChanceAttackRoll <= ThreeOrMoreConnectionChance)
        {
            return ChooseRandomly(moreConnectionAttacks);
        }
        else if (twoConnectionAttacks.Any() && connectionChanceAttackRoll <= TwoConnectionChance)
        {
            return ChooseRandomly(twoConnectionAttacks);
        }
        else if(oneConnectionAttacks.Any() && connectionChanceAttackRoll <= OneConnectionChance)
        {
            return ChooseRandomly(oneConnectionAttacks);
        }


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
            if (HypotheticalSolutionTool.GetSolutionsFromClaimingTile(currentGameState, move, forSide).Any())
            {
                continue;
            }

            // Are there immediate neighbors we can develop next to?
            foreach (Vector2Int direction in relativeDirections)
            {
                // If there is a claimed tile from this side in an adjacent direction,
                // and there isn't ~the void~ or an opposing claimed tile in the opposite direction,
                // then this would be an investing move
                Vector2Int position = move + direction;
                Vector2Int back = move - direction;
                if (!currentGameState.SpotIsInBounds(position))
                {
                    continue;
                }

                if (!currentGameState.SpotIsInBounds(back))
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

            // Are there distal neighbors we can develop next to?
            foreach (Vector2Int direction in relativeDirections)
            {
                Vector2Int adjacentPositionThatWeWantToBeEmpty = move + direction;

                if (!currentGameState.SpotIsInBounds(adjacentPositionThatWeWantToBeEmpty))
                {
                    continue;
                }

                Vector2Int distalPositionThatWeWantToBeOurs = move + direction * 2;

                if (!currentGameState.SpotIsInBounds(distalPositionThatWeWantToBeOurs))
                {
                    continue;
                }

                if (currentGameState.SpotToSideOwnership.TryGetValue(adjacentPositionThatWeWantToBeEmpty, out int? claim) && claim == null
                    && currentGameState.SpotToSideOwnership.TryGetValue(distalPositionThatWeWantToBeOurs, out claim) && claim == forSide)
                {
                    Debug.Log($"{nameof(AIInvestor)} Investing in position {move} because it is distal to a claim with room to grow");
                    results.Add(move);
                }
            }
        }

        return results;
    }
}
