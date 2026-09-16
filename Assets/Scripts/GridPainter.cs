using System.Collections.Generic;
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
    public Slider HeightSlider;

    private List<Cell> instantiatedCells { get; set; } = new List<Cell>();

    public void Start()
    {
        this.WidthSlider.SetValueWithoutNotify(this.InitialWidth);
        this.HeightSlider.SetValueWithoutNotify(this.InitialHeight);

        this.Paint();
    }

    public void Paint(float _)
    {
        this.Paint();
    }

    public void Paint()
    {
        for (int ii = this.instantiatedCells.Count - 1; ii >= 0; ii--)
        {
            Destroy(this.instantiatedCells[ii].gameObject);
        }
        this.instantiatedCells.Clear();

        int width = Mathf.RoundToInt(this.WidthSlider.value);
        int height = Mathf.RoundToInt(this.HeightSlider.value);

        float xOffset = width / 2f;
        float yOffset = height / 2f;

        for (int xx = 0; xx < width; xx++)
        {
            for (int yy = 0; yy < height; yy++)
            {
                Cell newCell = Instantiate(CellPF, this.transform);
                newCell.transform.position = new Vector2(xx - xOffset, yy - yOffset);
                instantiatedCells.Add(newCell);
            }
        }

        GridCamera.orthographicSize = OrthographicSizeBase + Mathf.Max(width, height) * OrthographicSizeScalar;
    }
}
