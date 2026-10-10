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

        public HashSet<int> SidesThatAreAI = new HashSet<int>();

        public GameConductor GameConductor;
        public int PlayerCount => this.GameConductor.CurrentBoardState == null ? 0 : this.GameConductor.CurrentBoardState.PlayerCount;

        public void ResetGame()
        {
            int playerCount = this.GameConductor.CurrentBoardState.SideIndexesStillInGame.Count;

            this.GameConductor.CurrentBoardState.OnPlayerStartTurn += UpdateTurn;
        }

        public void UpdateTurn(int toProfileIndex)
        {
            PlayerProfile profile = PersistentGameConfiguration.Singleton.Players[toProfileIndex];
            OnTurnStarted?.Invoke(toProfileIndex);
        }

        public bool PlayerIsHuman(int index)
        {
            return !PersistentGameConfiguration.Singleton.Players[index].IsAI;
        }
    }
}