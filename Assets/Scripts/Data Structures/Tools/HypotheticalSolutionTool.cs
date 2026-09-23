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
        public static IReadOnlyList<Coordinate> ScoreDirectionalities = new Coordinate[]
        {
            Coordinate.right,
            Coordinate.right + Coordinate.down,
            Coordinate.down,
            Coordinate.left + Coordinate.down,
        };

        /// <summary>
        /// Gets all solutions that involve a given tile, if that tile were placed.
        /// </summary>
        public static List<CellsSolution> GetSolutionsFromClaimingTile(GameState currentGameState, Coordinate position, int ownership)
        {
            List<CellsSolution> newSolutions = new List<CellsSolution>();

            foreach (Coordinate scoreDirectionality in ScoreDirectionalities)
            {
                // Let's say this is a "right" directionality
                // This is a point if it has three in a row with that directionality,
                // whether it's "negative" away or "positive" away
                List<Coordinate> pairingsForward = OccupiedNeighborsCountInDirection(currentGameState, position, ownership, scoreDirectionality);
                List<Coordinate> pairingsBackward = OccupiedNeighborsCountInDirection(currentGameState, position, ownership, -scoreDirectionality);

                if (pairingsBackward.Count + pairingsForward.Count < currentGameState.InARowToSolve - 1)
                {
                    continue;
                }

                // Flip the "reverse" pairing so that the farther away spot is earlier in the list
                pairingsBackward.Reverse();
                List<Coordinate> pairings = new List<Coordinate>(currentGameState.InARowToSolve * 2 - 1);

                pairings.AddRange(pairingsBackward);
                pairings.Add(position);
                pairings.AddRange(pairingsForward);

                newSolutions.Add(new CellsSolution(pairings, scoreDirectionality));
            }

            return currentGameState.PruneSolutionsForNotAlreadySolved(newSolutions);
        }

        public static List<Coordinate> OccupiedNeighborsCountInDirection(GameState currentGameState, Coordinate position, int ownership, Coordinate directionality)
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
