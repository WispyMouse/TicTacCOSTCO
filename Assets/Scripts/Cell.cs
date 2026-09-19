using TMPro;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public SpriteRenderer CellSpriteRenderer;
    public SpriteRenderer IconSpriteRenderer;

    public Sprite DefaultBorder;
    public Sprite RecentBorder;

    public GameObject HintHolder;
    public TMP_Text HintNumber;

    public bool AlreadyPlaced { get; set; } = false;

    public int SideIndex { get; private set; }
    public Vector2Int Position { get; set; }

    private void Awake()
    {
        this.SetHighlightStatus(false);
        this.HintHolder.gameObject.SetActive(false);
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

    public void SetHint(int amount)
    {
        if (amount == 0)
        {
            this.HintHolder.SetActive(false);
        }
        else
        {
            this.HintHolder.SetActive(true);
            this.HintNumber.text = amount.ToString();
        }
    }
}
