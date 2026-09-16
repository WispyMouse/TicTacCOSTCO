using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderHolder : MonoBehaviour
{
    public List<Sprite> SpritesForTurns = new List<Sprite>();

    [Range(2, 5)]
    public int PlayerCount = 2;
    public int PlayerCountIndex { get; set; } = 0;

    public Image CurrentTurnIconHolder;

    public void SetTurnIndex(int index)
    {
        this.PlayerCountIndex = index;
        this.CurrentTurnIconHolder.sprite = this.SpritesForTurns[this.PlayerCountIndex];
    }

    public void NextPlayerIcon()
    {
        this.PlayerCountIndex = (this.PlayerCountIndex + 1) % this.PlayerCount;
        this.CurrentTurnIconHolder.sprite = this.SpritesForTurns[this.PlayerCountIndex];
    }
}
