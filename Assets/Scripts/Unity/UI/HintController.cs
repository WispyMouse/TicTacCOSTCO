namespace TicTacCOSTCO.Unity.UI
{
    using UnityEngine;

    public class HintController : MonoBehaviour
    {
        public GameConductor GameConductor;
        public TurnOrderHolder TurnOrderHolder;
        public GridPainter GridPainter;

        bool isOn { get; set; } = false;


        public void Start()
        {
            // As it becomes a new player's turn, if hints are on, update them
            // It doesn't become the new player's hints, but we may need to update the hints for the displaying player
            this.TurnOrderHolder.OnTurnStarted += RefreshHints;
        }


        public void ShowHints(int side)
        {
            foreach (Cell cell in GameConductor.PositionsToCells.Values)
            {
                cell.SetHint(0);

                if (cell.AlreadyPlaced)
                {
                    continue;
                }

                cell.SetHint(this.GameConductor.CurrentGameState.CurrentBoardState.GenerateCommandFromMove(side, cell.Position).ConnectionsMade.Count);
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

        public void RefreshHints(int side)
        {
            if (!isOn)
            {
                return;
            }

            ShowHints(side);
        }
    }
}