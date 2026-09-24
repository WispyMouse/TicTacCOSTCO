namespace TicTacCOSTCO.Unity.UI
{
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class TurnOrderHolder : MonoBehaviour
    {
        public delegate void UITurnStarted(int sideIndex);
        public UITurnStarted OnTurnStarted;

        public List<Sprite> SpritesForTurns = new List<Sprite>();
        public List<Color> KnockoutColorsForTurns = new List<Color>();
        public List<string> PlayerNames { get; set; } = new List<string>();

        public Image CurrentTurnIconHolder;

        public HashSet<int> SidesThatAreAI = new HashSet<int>();

        public GameConductor GameConductor;
        public int PlayerCount => this.GameConductor.CurrentGameState == null ? 0 : this.GameConductor.CurrentGameState.PlayerCount;

        public void ResetGame()
        {
            int playerCount = this.GameConductor.CurrentGameState.SideIndexesStillInGame.Count;

            this.PlayerNames.Clear();

            for (int ii = 0; ii < playerCount; ii++)
            {
                if (!this.SidesThatAreAI.Contains(ii))
                {
                    // TODO: Re-enable naming yourself!
                    this.PlayerNames.Add("Human");
                }
                else
                {
                    this.PlayerNames.Add($"Computer");
                }
            }

            this.GameConductor.CurrentGameState.OnPlayerTurn += UpdateTurn;
            this.UpdateTurn(this.GameConductor.CurrentGameState.CurrentPlayerIndex);
        }

        public void UpdateTurn(int index)
        {
            this.CurrentTurnIconHolder.sprite = this.SpritesForTurns[index];
            OnTurnStarted?.Invoke(index);
        }

        public void ToggleHumanity(int index)
        {
            if (this.SidesThatAreAI.Contains(index))
            {
                this.SidesThatAreAI.Remove(index);
            }
            else
            {
                this.SidesThatAreAI.Add(index);
            }
            this.ResetGame();
        }

        public bool PlayerIsHuman(int index)
        {
            return !this.SidesThatAreAI.Contains(index);
        }
    }
}