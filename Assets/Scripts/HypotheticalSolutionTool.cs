using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public static class HypotheticalSolutionTool
{
    /// <summary>
    /// Given a starting tile, this is all of the "directions" a scoring point can go in.
    /// This assumes that instead of scoring "up and left", you should instead start there and go "down and right".
    /// </summary>
    public static IReadOnlyList<Vector2Int> ScoreDirectionalities = new Vector2Int[]
    {
        Vector2Int.right,
        Vector2Int.right + Vector2Int.down,
        Vector2Int.down,
        Vector2Int.left + Vector2Int.down,
    };

    /// <summary>
    /// Gets all solutions that involve a given tile, if that tile were placed.
    /// </summary>
    public static List<CellsSolution> GetSolutionsFromClaimingTile(GameState currentGameState, Vector2Int position, int ownership)
    {
        List<CellsSolution> newSolutions = new List<CellsSolution>();

        foreach (Vector2Int scoreDirectionality in ScoreDirectionalities)
        {
            // Let's say this is a "right" directionality
            // This is a point if it has three in a row with that directionality,
            // whether it's "negative" away or "positive" away
            List<Vector2Int> pairingsForward = OccupiedNeighborsCountInDirection(currentGameState, position, ownership, scoreDirectionality);
            List<Vector2Int> pairingsBackward = OccupiedNeighborsCountInDirection(currentGameState, position, ownership, -scoreDirectionality);

            if (pairingsBackward.Count + pairingsForward.Count < currentGameState.InARowToSolve - 1)
            {
                continue;
            }

            // Flip the "reverse" pairing so that the farther away spot is earlier in the list
            pairingsBackward.Reverse();
            List<Vector2Int> pairings = new List<Vector2Int>(currentGameState.InARowToSolve * 2 - 1);

            pairings.AddRange(pairingsBackward);
            pairings.Add(position);
            pairings.AddRange(pairingsForward);

            newSolutions.Add(new CellsSolution(pairings, scoreDirectionality));
        }

        return currentGameState.PruneSolutionsForNotAlreadySolved(newSolutions);
    }

    public static List<Vector2Int> OccupiedNeighborsCountInDirection(GameState currentGameState, Vector2Int position, int ownership, Vector2Int directionality)
    {
        List<Vector2Int> occupiedPositions = new List<Vector2Int>(currentGameState.Width);

        // Theoretically infinite size query
        for (int ii = 1; ; ii++)
        {
            Vector2Int nextPosition = position + directionality * ii;

            if (!currentGameState.SpotIsInBounds(nextPosition))
            {
                break;
            }

            if (currentGameState.SpotToSideOwnership[nextPosition] != ownership)
            {
                break;
            }

            occupiedPositions.Add(nextPosition);
        }

        return occupiedPositions;
    }
}
