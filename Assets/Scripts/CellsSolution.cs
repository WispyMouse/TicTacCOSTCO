using System.Collections.Generic;
using UnityEngine;

public class CellsSolution
{
    public readonly IReadOnlyList<Vector2Int> Cells;
    public readonly Vector2Int Root;
    public readonly Vector2Int Tail;
    public readonly Vector2Int Directionality;

    public CellsSolution(IReadOnlyList<Vector2Int> cells, Vector2Int directionality)
    {
        this.Cells = cells;
        this.Root = cells[0];
        this.Tail = cells[this.Cells.Count - 1];
        this.Directionality = directionality;
    }
}
