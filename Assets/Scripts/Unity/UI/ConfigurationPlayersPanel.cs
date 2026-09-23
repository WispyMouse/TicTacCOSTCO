namespace TicTacCOSTCO.Unity.UI
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public class ConfigurationPlayersPanel : MonoBehaviour
    {
        public List<string> PlayerNamesRecorded { get; set; } = new List<string>();

        public ConfigurationPlayerPanelRepresentation PlayerPanelRepresentationPF;
        public TurnOrderHolder TurnOrderHolder;

        private List<ConfigurationPlayerPanelRepresentation> Reps { get; set; } = new List<ConfigurationPlayerPanelRepresentation>();

        private void OnEnable()
        {
            this.PlayerCountUpdated();
        }

        public void PlayerCountUpdated()
        {
            for (int ii = this.transform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(transform.GetChild(ii).gameObject);
            }
            this.Reps.Clear();

            for (int ii = 0; ii < this.TurnOrderHolder.PlayerCount; ii++)
            {
                ConfigurationPlayerPanelRepresentation rep = Instantiate(this.PlayerPanelRepresentationPF, this.transform);
                rep.Set(ii, this.TurnOrderHolder);

                if (this.PlayerNamesRecorded.Count > ii)
                {
                    rep.NameInput.SetTextWithoutNotify(this.PlayerNamesRecorded[ii]);
                }
                rep.NameInput.onEndEdit.AddListener(PlayerNameUpdated);
                this.Reps.Add(rep);
            }
        }

        public string GetPlayerName(int index)
        {
            if (this.Reps.Count <= index)
            {
                return string.Empty;
            }
            return this.Reps[index].NameInput.text;
        }

        public void PlayerNameUpdated(string _)
        {
            this.PlayerNamesRecorded.Clear();
            for (int ii = 0; ii < this.Reps.Count; ii++)
            {
                this.PlayerNamesRecorded.Add(this.Reps[ii].NameInput.text);
            }
            this.TurnOrderHolder.ResetGame();
        }

        public void OpenSymbolPanel(int forPlayer)
        {

        }

        public void OpenColorPanel(int forPlayer)
        {

        }
    }
}