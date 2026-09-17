using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridPainter : MonoBehaviour
{
    [Range(3, 8)]
    [SerializeField]
    private int InitialWidth;
    [Range(3, 8)]
    [SerializeField]
    private int InitialHeight;
    public Cell CellPF;

    public Camera GridCamera;
    public float OrthographicSizeBase;
    public float OrthographicSizeScalar;

    public Slider WidthSlider;
    public TMP_Text WidthSliderValueLabel;
    public Slider HeightSlider;
    public TMP_Text HeightSliderValueLabel;

    public float WidthOrthographicViewMultiplier = 1.5f;

    private List<Cell> instantiatedCells { get; set; } = new List<Cell>();
    public GameConductor GameConductor;

    public void Awake()
    {
        this.WidthSlider.SetValueWithoutNotify(this.InitialWidth);
        this.HeightSlider.SetValueWithoutNotify(this.InitialHeight);
    }

    /// <summary>
    /// If this is called, we need to reconstruct the game.
    /// </summary>
    public void Paint(float _)
    {
        this.GameConductor.ResetGame();
    }

    public Dictionary<Vector2Int, Cell> Paint()
    {
        this.WidthSliderValueLabel.text = this.WidthSlider.value.ToString();
        this.HeightSliderValueLabel.text = this.HeightSlider.value.ToString();

        Dictionary<Vector2Int, Cell> cells = new Dictionary<Vector2Int, Cell>();

        for (int ii = this.instantiatedCells.Count - 1; ii >= 0; ii--)
        {
            Destroy(this.instantiatedCells[ii].gameObject);
        }
        this.instantiatedCells.Clear();

        float xOffset = this.GameConductor.Width / 2f;
        float yOffset = this.GameConductor.Height / 2f;

        for (int xx = 0; xx < this.GameConductor.Width; xx++)
        {
            for (int yy = 0; yy < this.GameConductor.Height; yy++)
            {
                Vector2Int position = new Vector2Int(xx, yy);
                Cell newCell = Instantiate(CellPF, this.transform);
                newCell.Position = position;
                newCell.transform.localPosition = new Vector2(xx - xOffset, yy - yOffset);
                instantiatedCells.Add(newCell);
                cells.Add(position, newCell);
            }
        }

        GridCamera.orthographicSize = OrthographicSizeBase 
            + Mathf.Max(this.GameConductor.Width * WidthOrthographicViewMultiplier, this.GameConductor.Height) 
            * OrthographicSizeScalar;

        return cells;
    }
}
