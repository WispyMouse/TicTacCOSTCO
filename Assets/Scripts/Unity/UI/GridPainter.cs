namespace TicTacCOSTCO.Unity.UI
{
    using System.Collections.Generic;
    using TicTacCOSTCO.DataStructures;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class GridPainter : MonoBehaviour
    {
        public Cell CellPF;

        public Camera GridCamera;
        public float OrthographicSizeBase;
        public float OrthographicSizeScalar;

        public float WidthOrthographicViewMultiplier = 1.5f;

        private List<Cell> instantiatedCells { get; set; } = new List<Cell>();
        public GameConductor GameConductor;

        /// <summary>
        /// If this is called, we need to reconstruct the game.
        /// </summary>
        public void Paint(float _)
        {
            this.GameConductor.ResetGame();
        }

        public Dictionary<Coordinate, Cell> Paint()
        {
            Dictionary<Coordinate, Cell> cells = new Dictionary<Coordinate, Cell>();

            for (int ii = this.instantiatedCells.Count - 1; ii >= 0; ii--)
            {
                Destroy(this.instantiatedCells[ii].gameObject);
            }
            this.instantiatedCells.Clear();

            float xOffset = this.GameConductor.CurrentBoardState.Width / 2f;
            float yOffset = this.GameConductor.CurrentBoardState.Height / 2f;

            for (int xx = 0; xx < this.GameConductor.CurrentBoardState.Width; xx++)
            {
                for (int yy = 0; yy < this.GameConductor.CurrentBoardState.Height; yy++)
                {
                    Coordinate position = new Coordinate(xx, yy);
                    Cell newCell = Instantiate(CellPF, this.transform);
                    newCell.Position = position;
                    newCell.transform.localPosition = new Vector2(xx - xOffset, yy - yOffset);
                    instantiatedCells.Add(newCell);
                    cells.Add(position, newCell);
                }
            }

            GridCamera.orthographicSize = OrthographicSizeBase
                + Mathf.Max(this.GameConductor.CurrentBoardState.Width * WidthOrthographicViewMultiplier, this.GameConductor.CurrentBoardState.Height)
                * OrthographicSizeScalar;

            return cells;
        }
    }
}