using System.Collections.Generic;
using TicTacCOSTCO.AIs;
using TicTacCOSTCO.DataStructures;
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

    public virtual Coordinate DetermineMove(int forSide, BoardState currentGameState)
    {
        IReadOnlyList<Coordinate> possibleMoves = currentGameState.GetEmptySpots();

        // Determine the heuristic value for every possible move
        Dictionary<Coordinate, float> moveToHeuristicTotal = new Dictionary<Coordinate, float>(possibleMoves.Count);
        float highestHeuristic = 0;

        foreach (Coordinate move in possibleMoves)
        {
            moveToHeuristicTotal.Add(move, 0);
        }

#if UNITY_EDITOR
        string logText = $"(Side: {forSide}) (Core: {this.name})";
#endif

        foreach (AIHeuristic heuristic in AIHeuristics)
        {

#if UNITY_EDITOR
            string withHeuristic = logText + $" (Heuristic: {heuristic.name})";
#endif

            heuristic.BakeInformation(forSide, currentGameState);

            foreach (Coordinate coordinate in possibleMoves)
            {
#if UNITY_EDITOR
                string withCoordinate = withHeuristic + $" (Coordinate: {coordinate})";
#endif

                float heuristicValue = heuristic.ScorePosition(forSide, currentGameState, coordinate);

#if UNITY_EDITOR
                UnityEngine.Debug.Log(withCoordinate + $" (Value: {heuristicValue})");
#endif

                float newTotal = moveToHeuristicTotal[coordinate] + heuristicValue;
                highestHeuristic = Mathf.Max(newTotal, highestHeuristic);
                moveToHeuristicTotal[coordinate] = newTotal;
            }
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
        Coordinate choice = ChooseRandomly(coordinatesToConsider);

#if UNITY_EDITOR
        UnityEngine.Debug.Log(logText + $" Chose {choice} which was {(moveToHeuristicTotal[choice] / highestHeuristic).ToString("P2")} of highest at {moveToHeuristicTotal[choice]}");
#endif

        return choice;
    }

    protected virtual Coordinate ChooseRandomly(IReadOnlyList<Coordinate> options)
    {
        return options[Random.Range(0, options.Count)];
    }
}
