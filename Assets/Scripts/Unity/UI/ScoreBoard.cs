namespace TicTacCOSTCO.Unity.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ScoreBoard : MonoBehaviour
    {
        public Image WinnerIconImage;

        public TurnOrderHolder TurnOrderHolder;

        public Transform WinnerIconHolder;

        private int MaxDisplayedWins { get; set; } = 9;

        private void Awake()
        {
            for (int ii = this.WinnerIconHolder.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.WinnerIconHolder.GetChild(ii).gameObject);
            }
        }

        public void AddToScoreboard(PlayerProfile winner)
        {
            if (this.WinnerIconHolder.childCount >= this.MaxDisplayedWins)
            {
                Destroy(this.WinnerIconHolder.GetChild(0).gameObject);
            }

            Sprite winnerSprite = winner.RepresenterSprite;
            Image newImage = Instantiate(WinnerIconImage, WinnerIconHolder);
            newImage.sprite = winnerSprite;
        }
    }
}