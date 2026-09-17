using UnityEngine;

public class Cell : MonoBehaviour
{
    public SpriteRenderer CellSpriteRenderer;
    public SpriteRenderer IconSpriteRenderer;

    public Sprite VictoryBorderSprite;

    public bool AlreadyPlaced { get; set; } = false;

    public int SideIndex { get; private set; }
    public Vector2Int Position { get; set; }

    public void SetSide(Sprite sprite, int sideIndex)
    {
        this.SideIndex = sideIndex;
        this.IconSpriteRenderer.sprite = sprite;
        this.AlreadyPlaced = true;
    }

    public void HighlightForVictory()
    {
        this.CellSpriteRenderer.sprite = this.VictoryBorderSprite;
    }
}
