using TicTacCOSTCO.Unity.UI;
using TMPro;
using UnityEngine;

public class CascadeBanner : MonoBehaviour
{
    public GameConductor GameConductor;
    public GameObject ToggleParent;
    public CascadeText CascadeAnimator;

    public void ResetGame()
    {
        this.GameConductor.CurrentGameState.OnPlayerMadeMove += OnMoveMade;
        this.ToggleParent.SetActive(false);
    }

    public void OnMoveMade(int player)
    {
        if (this.GameConductor.CurrentGameState.LastCascade == 0)
        {
            ToggleParent.SetActive(false);
            return;
        }

        ToggleParent.SetActive(true);
        if (this.GameConductor.CurrentGameState.LastCascade == 1)
        {
            this.CascadeAnimator.TextString = "Row x1";
        }
        else
        {
            this.CascadeAnimator.TextString = $"Cascade x{this.GameConductor.CurrentGameState.LastCascade}";
        }

        this.CascadeAnimator.EnableAnimation(1f / this.GameConductor.CurrentGameState.LastCascade);
    }
}
