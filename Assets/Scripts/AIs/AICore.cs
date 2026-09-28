using System.Collections.Generic;
using TicTacCOSTCO.DataStructures;
using TicTacCOSTCO.DataStructures.Tools;
using UnityEngine;

[CreateAssetMenu(fileName = "AICore.asset", menuName = "COSTCO/AI Core")]
public class AICore : ScriptableObject
{
    public virtual Coordinate DetermineMove(int forSide, GameState currentGameState)
    {
        IReadOnlyList<Coordinate> possibleMoves = currentGameState.GetEmptySpots();
        return ChooseRandomly(possibleMoves);
    }

    protected virtual Coordinate ChooseRandomly(IReadOnlyList<Coordinate> options)
    {
        return options[Random.Range(0, options.Count)];
    }

    protected IReadOnlyList<Coordinate> GetMovesThatDoNotImmediatleyLose(IReadOnlyList<Coordinate> options, GameState currentGameState, int forSide)
    {
        List<Coordinate> notLosingMoves = new List<Coordinate>(options.Count);

        foreach (Coordinate move in options)
        {
            if (currentGameState.TryGetAllSolutionsFromCell(forSide, move, out List<CellsConnection> newConnections))
            {
                if (newConnections.Count >= currentGameState.CurrentCascadeLevel)
                {
                    notLosingMoves.Add(move);
                }
            }
        }

        return notLosingMoves;
    }
}
