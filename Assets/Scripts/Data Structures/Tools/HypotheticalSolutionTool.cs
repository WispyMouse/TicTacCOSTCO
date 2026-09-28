namespace TicTacCOSTCO.DataStructures.Tools
{
    using System.Collections.Generic;
    using TicTacCOSTCO.DataStructures;

    public static class HypotheticalSolutionTool
    {
        /// <summary>
        /// Given a starting tile, this is all of the "directions" a scoring point can go in.
        /// This assumes that instead of scoring "up and left", you should instead start there and go "down and right".
        /// </summary>
        public static IReadOnlyList<DirectionalityVector> ScoreDirectionalities = new DirectionalityVector[]
        {
            new DirectionalityVector(1, 0),
            new DirectionalityVector(1, -1),
            new DirectionalityVector(0, -1),
            new DirectionalityVector(-1, -1),
        };

        public static List<Coordinate> OccupiedNeighborsCountInDirection(GameState currentGameState, Coordinate position, int ownership, DirectionalityVector directionality)
        {
            List<Coordinate> occupiedPositions = new List<Coordinate>(currentGameState.Width);

            // Theoretically infinite size query
            for (int ii = 1; ; ii++)
            {
                Coordinate nextPosition = position + directionality * ii;

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

}
