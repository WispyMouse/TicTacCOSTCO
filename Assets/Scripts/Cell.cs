using UnityEngine;

public class Cell : MonoBehaviour
{
    public SpriteRenderer CellSpriteRenderer;
    public SpriteRenderer IconSpriteRenderer;

    public Sprite DefaultBorder;
    public Sprite RecentBorder;

    public bool AlreadyPlaced { get; set; } = false;

    public int SideIndex { get; private set; }
    public Vector2Int Position { get; set; }

    private void Awake()
    {
        this.SetHighlightStatus(false);
    }

    public void SetSide(Sprite sprite, int sideIndex)
    {
        this.SideIndex = sideIndex;
        this.IconSpriteRenderer.sprite = sprite;
        this.AlreadyPlaced = true;
    }

    public void SetHighlightStatus(bool toHighlight)
    {
        this.CellSpriteRenderer.sprite = toHighlight ? this.RecentBorder : this.DefaultBorder;
    }
}
