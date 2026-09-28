using System.Collections.Generic;
using TicTacCOSTCO.AIs;
using TicTacCOSTCO.DataStructures;
using TicTacCOSTCO.DataStructures.Tools;
using UnityEngine;

[CreateAssetMenu(fileName = "AICore.asset", menuName = "COSTCO/AI Core")]
public class AICore : ScriptableObject
{
    /// <summary>
    /// After the heuristic for each move is considered, calculate their relative value to the highest value recorded.
    /// Then, choose a move randomly from the top percentage cut.
    /// This helps choose only good moves, but also leaves open the ability to choose randomly.
    /// </summary>
    [Range(0, 1f)]
    public float TopCut = .8f;

    public List<AIHeuristic> AIHeuristics = new List<AIHeuristic>();

    public virtual Coordinate DetermineMove(int forSide, GameState currentGameState)
    {
        IReadOnlyList<Coordinate> possibleMoves = currentGameState.GetEmptySpots();

        // Determine the heuristic value for every possible move
        Dictionary<Coordinate, float> moveToHeuristicTotal = new Dictionary<Coordinate, float>(possibleMoves.Count);
        float highestHeuristic = 0;

#if UNITY_EDITOR
        string logText = $"(Side: {forSide}) (Core: {this.name})";
#endif

        foreach (Coordinate coordinate in possibleMoves)
        {
            float heuristicTotal = 0;

#if UNITY_EDITOR
            string withCoordinate = logText + $" (Coordinate: {coordinate})";
#endif

            foreach (AIHeuristic heuristic in AIHeuristics)
            {
                float heuristicValue = heuristic.ScorePosition(forSide, currentGameState, coordinate);

#if UNITY_EDITOR
                UnityEngine.Debug.Log(withCoordinate + $" (Heuristic: {heuristic.name}) (Value: {heuristicValue})");
#endif

                heuristicTotal += heuristicValue;
            }

            moveToHeuristicTotal.Add(coordinate, heuristicTotal);

            highestHeuristic = Mathf.Max(highestHeuristic, heuristicTotal);
        }

        // Determine which coordinates scored *at least* the TopCut percentage of the highest heuristic
        float heuristicCutoff = highestHeuristic * TopCut;
        List<Coordinate> coordinatesToConsider = new List<Coordinate>();

        foreach (Coordinate coordinate in possibleMoves)
        {
            if (moveToHeuristicTotal[coordinate] >= heuristicCutoff)
            {
                coordinatesToConsider.Add(coordinate);
            }
        }

        // Unweightedly pick one at random
        return ChooseRandomly(coordinatesToConsider);
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
