using System.Collections.Generic;
using UnityEngine;

public class ConfigurationPlayersPanel : MonoBehaviour
{
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
        }
    }
}
