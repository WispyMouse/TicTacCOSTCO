using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public readonly Dictionary<Vector2Int, int?> SpotToSideOwnership = new Dictionary<Vector2Int, int?>();

    public GameState(int width, int height)
    {
        for (int xx = 0; xx < width; xx++)
        {
            for (int yy = 0; yy < height; yy++)
            {
                this.SpotToSideOwnership.Add(new Vector2Int(xx, yy), null);
            }
        }
    }

    public void SetSideOwnership(Vector2Int position, int side)
    {
        this.SpotToSideOwnership[position] = side;
    }

    public IReadOnlyList<Vector2Int> GetEmptySpots()
    {
        List<Vector2Int> emptySpots = new List<Vector2Int>();

        foreach (Vector2Int position in SpotToSideOwnership.Keys)
        {
            int? ownership = SpotToSideOwnership[position];

            if (ownership == null)
            {
                emptySpots.Add(position);
            }
        }

        return emptySpots;
    }

    public bool AnyEmptySpots()
    {
        foreach (Vector2Int position in SpotToSideOwnership.Keys)
        {
            int? ownership = SpotToSideOwnership[position];

            if (ownership == null)
            {
                return true;
            }
        }

        return false;
    }
}
