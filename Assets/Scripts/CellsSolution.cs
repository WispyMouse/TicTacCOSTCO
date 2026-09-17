using System.Collections.Generic;
using UnityEngine;

public class CellsSolution
{
    public readonly IReadOnlyList<Cell> Cells;
    public readonly Cell Root;
    public readonly Cell Tail;
    public readonly Vector2Int Directionality;

    public CellsSolution(IReadOnlyList<Cell> cells, Vector2Int directionality)
    {
        this.Cells = cells;
        this.Root = cells[0];
        this.Tail = cells[this.Cells.Count - 1];
        this.Directionality = directionality;
    }
}
