using TicTacCOSTCO.DataStructures;
using TicTacCOSTCO.Unity;
using TicTacCOSTCO.Unity.UI;
using TMPro;
using UnityEngine;

public class CascadeBanner : MonoBehaviour
{
    public GameConductor GameConductor;
    public GameObject ToggleParent;
    public CascadeText CascadeAnimator;

    public float NoWinnerSpeed = .7f;
    public float WinnerSpeed = 3f;

    public void ResetGame()
    {
        this.GameConductor.CurrentGameState.OnPlayerMadeMove += OnMoveMade;
        this.GameConductor.CurrentGameState.OnMoveUndone += OnMoveUndone;
        this.GameConductor.CurrentGameState.OnGameConclusion += OnGameConclusion;
        this.ToggleParent.SetActive(false);
    }

    public void OnMoveUndone(MoveCommand undone)
    {
        this.OnMoveMade(this.GameConductor.CurrentGameState.CurrentBoardState.CurrentPlayerIndex);
    }

    public void OnMoveMade(int player)
    {
        if (this.GameConductor.CurrentGameState.CurrentBoardState.CurrentCascadeLevel == 0)
        {
            ToggleParent.SetActive(false);
            return;
        }

        ToggleParent.SetActive(true);
        if (this.GameConductor.CurrentGameState.CurrentBoardState.CurrentCascadeLevel == 1)
        {
            this.CascadeAnimator.TextString = "Row x1";
        }
        else
        {
            this.CascadeAnimator.TextString = $"Cascade x{this.GameConductor.CurrentGameState.CurrentBoardState.CurrentCascadeLevel}";
        }

        this.CascadeAnimator.EnableAnimation(1f / this.GameConductor.CurrentGameState.CurrentBoardState.CurrentCascadeLevel);
    }

    public void OnGameConclusion(int? winner)
    {
        if (winner.HasValue)
        {
            this.CascadeAnimator.TextString = $"{PersistentGameConfiguration.Singleton.Players[winner.Value].PlayerName} WINS!";
            this.CascadeAnimator.EnableAnimation(this.WinnerSpeed);
        }
        else
        {
            this.CascadeAnimator.TextString = "~NO WINNER~";
            this.CascadeAnimator.EnableAnimation(this.NoWinnerSpeed);
        }
    }
}
