namespace TicTacCOSTCO.Unity.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Profiling;

    public class CurrentTurnPanel : MonoBehaviour
    {
        public CurrentTurnPanelEntry CurrentTurnPanelEntryPF;
        public Transform CurrentTurnPanelEntryParent;
        private List<CurrentTurnPanelEntry> entries { get; set; } = new List<CurrentTurnPanelEntry>();

        public TurnOrderHolder TurnOrderHolder;
        public HintController HintController;

        public void Awake()
        {
            this.TurnOrderHolder.OnTurnStarted += this.UpdateToNewTurn;
        }

        public void ResetGame()
        {
            for (int ii = this.CurrentTurnPanelEntryParent.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.CurrentTurnPanelEntryParent.GetChild(ii).gameObject);
            }
            this.entries.Clear();

            for (int playerIndex = 0; playerIndex < this.TurnOrderHolder.PlayerCount; playerIndex++)
            {
                CurrentTurnPanelEntry newTurn = Instantiate(this.CurrentTurnPanelEntryPF, this.CurrentTurnPanelEntryParent);
                this.entries.Add(newTurn);
                newTurn.RepresentProfile(PersistentGameConfiguration.Singleton.Players[playerIndex]);

                newTurn.OnShowHints += SetHints;
                newTurn.OnHideHints += HideHints;
            }

            this.UpdateToNewTurn(this.TurnOrderHolder.GameConductor.CurrentBoardState.CurrentPlayerIndex);
        }

        public void UpdateToNewTurn(int newTurn)
        {
            for (int ii = 0; ii < this.TurnOrderHolder.GameConductor.CurrentBoardState.PlayerCount; ii++)
            {
                this.entries[ii].SetCurrentTurn(ii == newTurn);
            }

            this.HideHints();
        }

        public void KnockOutPlayer(int index)
        {
            this.entries[index].MarkKnockout();
        }

        public void RestorePlayer(int index)
        {
            this.entries[index].RestorePlayer();
        }

        public void SetCascade(int index, int cascadeLevel)
        {
            this.entries[index].SetCascade(cascadeLevel);
        }

        public void SetHints(int visibleHint)
        {
            for (int ii = 0; ii < this.TurnOrderHolder.GameConductor.CurrentBoardState.PlayerCount; ii++)
            {
                SetHint(ii, ii == visibleHint);
            }
        }

        public void SetHint(int index, bool shown)
        {
            if (!shown)
            {
                return;
            }

            this.HintController.ShowHints(index);
        }

        public void HideHints()
        {
            this.HintController.HideHints();
        }
    }
}