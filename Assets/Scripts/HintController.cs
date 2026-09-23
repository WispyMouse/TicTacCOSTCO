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
            if (cell.AlreadyPlaced)
            {
                continue;
            }

            cell.SetHint(HypotheticalSolutionTool.GetSolutionsFromClaimingTile(this.GameConductor.CurrentGameState, cell.Position, this.GameConductor.CurrentGameState.CurrentPlayerIndex).Count);
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
