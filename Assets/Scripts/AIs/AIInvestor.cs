using System.Collections.Generic;
using System.Linq;
using TicTacCOSTCO.DataStructures;
using TicTacCOSTCO.DataStructures.Tools;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// When considering what action to take, identify if there is a subset of actions that will not result in a lost game immediately.
/// This is not a very solid strategy, but will at least attempt to defend Cascades.
/// </summary>
[CreateAssetMenu(fileName = "AICore_Investor.asset", menuName = "COSTCO/Investor AI Core")]
public class AIInvestor : AICore
{
    public override Coordinate DetermineMove(int forSide, GameState currentGameState)
    {
        IReadOnlyList<Coordinate> possibleMoves = currentGameState.GetEmptySpots();

        if (currentGameState.CurrentGameState != GameState.GameStateEnum.Cascade)
        {
            // Try to invest
            List<Coordinate> investingMoves = GetPositionsThatInvestWithoutSolving(forSide, currentGameState);
            if (investingMoves.Count > 0)
            {
                return ChooseRandomly(investingMoves);
            }

            Debug.Log($"{nameof(AIInvestor)} I couldn't find anywhere to invest in");

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

    private List<Coordinate> GetPositionsThatInvestWithoutSolving(int forSide, GameState currentGameState)
    {
        IReadOnlyList<Coordinate> possibleMoves = currentGameState.GetEmptySpots();
        List<Coordinate> results = new List<Coordinate>();

        List<DirectionalityVector> relativeDirections = new List<DirectionalityVector>();
        relativeDirections.AddRange(currentGameState.Directionalities);
        relativeDirections.AddRange(currentGameState.Directionalities.Select(x => -x));

        foreach (Coordinate move in possibleMoves)
        {
            // If this solves anything, we shouldn't use it
            if (HypotheticalSolutionTool.GetSolutionsFromClaimingTile(currentGameState, move, forSide).Any())
            {
                continue;
            }

            // Are there immediate neighbors we can develop next to?
            foreach (DirectionalityVector direction in relativeDirections)
            {
                // If there is a claimed tile from this side in an adjacent direction,
                // and there isn't ~the void~ or an opposing claimed tile in the opposite direction,
                // then this would be an investing move
                Coordinate position = move + direction * 1;
                Coordinate back = move - direction * 1;
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
            foreach (DirectionalityVector direction in relativeDirections)
            {
                Coordinate adjacentPositionThatWeWantToBeEmpty = move + direction * 1;

                if (!currentGameState.SpotIsInBounds(adjacentPositionThatWeWantToBeEmpty))
                {
                    continue;
                }

                Coordinate distalPositionThatWeWantToBeOurs = move + direction * 2;

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
