using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderHolder : MonoBehaviour
{
    public delegate void TurnStarted(int sideIndex);
    public TurnStarted OnTurnStarted;

    public List<Sprite> SpritesForTurns = new List<Sprite>();
    public List<Color> KnockoutColorsForTurns = new List<Color>();

    [Range(2, 5)]
    public int PlayerCount = 2;
    public int CurrentPlayerIndex { get; set; } = 0;

    public Image CurrentTurnIconHolder;

    public HashSet<int> SideIndexesStillInGame = new HashSet<int>();
    public HashSet<int> SidesThatAreAI = new HashSet<int>();

    public Slider PlayerCountSlider;
    public TMP_Text PlayerCountSliderValueLabel;
    public ConfigurationPlayersPanel CurrentConfigurationPanel;

    public void ResetGame()
    {
        this.PlayerCount = (int)this.PlayerCountSlider.value;
        this.PlayerCountSliderValueLabel.text = this.PlayerCount.ToString();

        this.SideIndexesStillInGame.Clear();

        for (int ii = 0; ii < this.PlayerCount; ii++)
        {
            this.SideIndexesStillInGame.Add(ii);
        }

        this.SetTurnIndex(0);
        this.CurrentConfigurationPanel.PlayerCountUpdated();
    }

    public void SetTurnIndex(int index)
    {
        this.CurrentPlayerIndex = index;
        this.CurrentTurnIconHolder.sprite = this.SpritesForTurns[this.CurrentPlayerIndex];
        OnTurnStarted?.Invoke(index);
    }

    public void NextPlayerIcon()
    {
        for (int ii = 1; ii < this.PlayerCount; ii ++)
        {
            int nextProspectivePlayer = (this.CurrentPlayerIndex + ii) % this.PlayerCount;
            if (!this.SideIndexesStillInGame.Contains(nextProspectivePlayer))
            {
                continue;
            }

            this.SetTurnIndex(nextProspectivePlayer);
            break;
        }

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

    public void KnockOutPlayer(int index)
    {
        this.SideIndexesStillInGame.Remove(index);
    }
}
