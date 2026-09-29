namespace TicTacCOSTCO.Unity.UI
{
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class TurnOrderHolder : MonoBehaviour
    {
        public delegate void UITurnStarted(int playerProfileIndex);
        public UITurnStarted OnTurnStarted;

        public Image CurrentTurnIconHolder;

        public HashSet<int> SidesThatAreAI = new HashSet<int>();

        public GameConductor GameConductor;
        public int PlayerCount => this.GameConductor.CurrentGameState == null ? 0 : this.GameConductor.CurrentGameState.CurrentBoardState.PlayerCount;

        public void ResetGame()
        {
            int playerCount = this.GameConductor.CurrentGameState.CurrentBoardState.SideIndexesStillInGame.Count;

            this.GameConductor.CurrentGameState.OnPlayerStartTurn += UpdateTurn;
            this.UpdateTurn(this.GameConductor.CurrentGameState.CurrentBoardState.CurrentPlayerIndex);
        }

        public void UpdateTurn(int toProfileIndex)
        {
            PlayerProfile profile = PersistentGameConfiguration.Singleton.Players[toProfileIndex];
            this.CurrentTurnIconHolder.sprite = profile.SpriteRepresentation;
            OnTurnStarted?.Invoke(toProfileIndex);
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
            return !PersistentGameConfiguration.Singleton.Players[index].IsAI;
        }
    }
}