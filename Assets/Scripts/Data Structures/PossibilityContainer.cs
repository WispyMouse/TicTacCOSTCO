using System;
using System.Collections;
using System.Collections.Generic;

namespace TicTacCOSTCO.DataStructures
{
    public class PossibilityContainer
    {
        struct GetCoordinatesAroundKey
        {
            public Coordinate position;
            public int radius;

            public GetCoordinatesAroundKey(Coordinate position, int radius)
            {
                this.position = position;
                this.radius = radius;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.position, this.radius);
            }
        }

        private Dictionary<GetCoordinatesAroundKey, List<Coordinate>> coordinateToRadiusNearby { get; set; } = new Dictionary<GetCoordinatesAroundKey, List<Coordinate>>();

        public readonly IDictionary<Coordinate, IReadOnlyCollection<CellsConnection>> PossibleConnections;

        public PossibilityContainer(BoardState boardState)
        {
            Dictionary<Coordinate, List<CellsConnection>> possibleConnections = new Dictionary<Coordinate, List<CellsConnection>>(boardState.Width * boardState.Height);

            for (int xx = 0; xx < boardState.Width; xx++)
            {
                for (int yy = 0; yy < boardState.Height; yy++)
                {
                    possibleConnections.Add(new Coordinate(xx, yy), new List<CellsConnection>());
                }
            }

            // For every cell, check what directionality-shapes can possibly lead to a connection
            // If any part of this is out of bounds, that direction couldn't start from this shape
            // We want to put every direction every shape is involved in within lists, including when it doesn't start from the shape
            for (int xx = 0; xx < boardState.Width; xx++)
            {
                for (int yy = 0; yy < boardState.Height; yy++)
                {
                    Coordinate rootedCoordinate = new Coordinate(xx, yy);

                    foreach (DirectionalityVector directionality in BoardState.DirectionalitiesWithBackwards)
                    {
                        if (!DirectionalityIsInBounds(boardState, rootedCoordinate, directionality))
                        {
                            continue;
                        }

                        List<Coordinate> coordinatesInConnection = new List<Coordinate>(boardState.InARowToSolve);
                        List<CellsConnection> newConnections = new List<CellsConnection>();

                        coordinatesInConnection.Add(rootedCoordinate);

                        for (int ii = 1; ii < boardState.InARowToSolve; ii++)
                        {
                            Coordinate resultingCoordinate = rootedCoordinate + directionality * ii;

                            coordinatesInConnection.Add(resultingCoordinate);

                            // If we are at least long enough, mark the connection
                            if (ii < boardState.InARowToSolve - 1)
                            {
                                continue;
                            }

                            CellsConnection thisConnection = new CellsConnection(new List<Coordinate>(coordinatesInConnection), directionality);
                            newConnections.Add(thisConnection);
                        }

                        foreach (CellsConnection connection in newConnections)
                        {
                            foreach (Coordinate curCoordinate in connection.Cells)
                            {
                                possibleConnections[curCoordinate].Add(connection);
                            }
                        }
                    }
                }
            }

            Dictionary<Coordinate, IReadOnlyCollection<CellsConnection>> castConnections = new Dictionary<Coordinate, IReadOnlyCollection<CellsConnection>>(possibleConnections.Count);

            foreach (Coordinate coordinate in possibleConnections.Keys)
            {
                castConnections.Add(coordinate, possibleConnections[coordinate]);
            }

            this.PossibleConnections = castConnections;
        }

        bool DirectionalityIsInBounds(BoardState boardState, Coordinate fromCell, DirectionalityVector fromDirection)
        {
            for (int ii = 1; ii < boardState.InARowToSolve; ii++)
            {
                Coordinate resultingCoordinate = fromCell + fromDirection * ii;
                if (!boardState.SpotIsInBounds(resultingCoordinate))
                {
                    return false;
                }
            }

            return true;
        }

        public IReadOnlyList<Coordinate> GetCoordinatesAround(IReadOnlyBoardState currentGameState, Coordinate position, int radius)
        {
            GetCoordinatesAroundKey key = new GetCoordinatesAroundKey(position, radius);

            if (this.coordinateToRadiusNearby.TryGetValue(key, out List<Coordinate> coordinates))
            {
                return coordinates;
            }

            int span = radius * 2 - 1;
            List<Coordinate> coordinatesToCheck = new List<Coordinate>(span);

            for (int xx = -radius; xx <= radius; xx++)
            {
                for (int yy = -radius; yy <= radius; yy++)
                {
                    Coordinate resultingCoordinate = new Coordinate(xx, yy) + position;

                    if (resultingCoordinate == position)
                    {
                        continue;
                    }

                    if (!currentGameState.SpotIsInBounds(resultingCoordinate))
                    {
                        continue;
                    }

                    coordinatesToCheck.Add(resultingCoordinate);
                }
            }

            this.coordinateToRadiusNearby.Add(key,  coordinatesToCheck);

            return coordinatesToCheck;
        }
    }
}
