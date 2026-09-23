namespace TicTacCOSTCO.DataStructures
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Describes a "Connection", starting from the <see cref="Root"/> and ending at the <see cref="Tail"/>.
    /// </summary>
    public struct CellsConnection
    {
        const string REQUIRESONEORMORECELLS = "CellConnections must have one or more cells.";

        /// <summary>
        /// The <see cref="Coordinate"/> of cells making up this connection.
        /// Should be ordered such that <see cref="Root"/> is the 0th and <see cref="Tail"/> is the n-1th.
        /// </summary>
        public readonly IReadOnlyList<Coordinate> Cells;

        /// <summary>
        /// The starting point of this connection.
        /// </summary>
        public readonly Coordinate Root;

        /// <summary>
        /// The ending point of this connection.
        /// </summary>
        public readonly Coordinate Tail;

        /// <summary>
        /// What directional clear was used to determine this group?
        /// Can be used to join together same-Directionality Connections.
        /// </summary>
        public readonly DirectionalityVector Directionality;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cells">Cells contained in connection. Must contain at least one cell.</param>
        /// <param name="directionality">Directionality vector of the connection.</param>
        public CellsConnection(IReadOnlyList<Coordinate> cells, DirectionalityVector directionality)
        {
            int cellsCount = cells.Count;

            Assert.GreaterOrEqual(cellsCount, 1, REQUIRESONEORMORECELLS);

            this.Cells = cells;
            this.Root = cells[0];
            this.Tail = cells[this.Cells.Count - 1];

            this.Directionality = directionality;
        }
    }
}