using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfigurationPlayerPanelRepresentation : MonoBehaviour
{
    public Image PlayerImage;
    public int Index { get; set; }

    private TurnOrderHolder TurnOrderHolder;

    public TMP_Text HumanityLabel;

    public void Set(int index, TurnOrderHolder turnHolder)
    {
        this.TurnOrderHolder = turnHolder;
        this.Index = index;
        this.PlayerImage.sprite = turnHolder.SpritesForTurns[index];

        if (turnHolder.PlayerIsHuman(index))
        {
            this.HumanityLabel.text = "Human Player";
        }
        else
        {
            this.HumanityLabel.text = "Computer Player";
        }
    }

    public void ToggleHumanity()
    {
        this.TurnOrderHolder.ToggleHumanity(this.Index);
    }
}
