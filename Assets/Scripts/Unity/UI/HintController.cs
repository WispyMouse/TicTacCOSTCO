namespace TicTacCOSTCO.Unity.UI
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using TicTacCOSTCO.DataStructures;
    using TicTacCOSTCO.DataStructures.Tools;
    using UnityEngine;

    public class HintController : MonoBehaviour
    {
        public GameConductor GameConductor;
        public TurnOrderHolder TurnOrderHolder;
        public GridPainter GridPainter;

        bool isOn { get; set; } = false;


        public void Start()
        {
            this.TurnOrderHolder.OnTurnStarted += UpdateHints;
        }

        void UpdateHints(int _)
        {
            if (!isOn)
            {
                return;
            }

            ShowHints();
        }


        public void ShowHints()
        {
            foreach (Cell cell in GameConductor.PositionsToCells.Values)
            {
                cell.SetHint(0);

                if (cell.AlreadyPlaced)
                {
                    continue;
                }

                this.GameConductor.CurrentGameState.TryGetAllSolutionsFromCell(this.GameConductor.CurrentGameState.CurrentPlayerIndex, cell.Position, out List<CellsConnection> newConnections);
                cell.SetHint(newConnections.Count);
            }
            isOn = true;
        }

        public void HideHints()
        {
            foreach (Cell cell in GameConductor.PositionsToCells.Values)
            {
                cell.SetHint(0);
            }
            isOn = false;
        }
    }
}