namespace TicTacCOSTCO.DataStructures
{
    using System.Collections.Generic;
    using TicTacCOSTCO.DataStructures;

    public class CellsSolution
    {
        public readonly IReadOnlyList<Coordinate> Cells;
        public readonly Coordinate Root;
        public readonly Coordinate Tail;
        public readonly Coordinate Directionality;

        public CellsSolution(IReadOnlyList<Coordinate> cells, Coordinate directionality)
        {
            this.Cells = cells;
            this.Root = cells[0];
            this.Tail = cells[this.Cells.Count - 1];
            this.Directionality = directionality;
        }
    }
}