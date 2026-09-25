using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacCOSTCO.Unity.UI
{
    public class EditPlayerScreen : MonoBehaviour
    {
        public PlayerProfile ShowingProfile { get; private set; }

        public Image PlayerSpriteDisplay;

        public GameObject HumanPanel;
        public TMP_InputField NameInputField;

        public GameObject AIPanel;

        public void OpenFromPlayer(int player)
        {
            this.gameObject.SetActive(true);

            this.ShowingProfile = PersistentGameConfiguration.Singleton.Players[player];

            this.PlayerSpriteDisplay.sprite = this.ShowingProfile.RepresenterSprite;

            if (this.ShowingProfile.IsAI)
            {
                this.AIPanel.SetActive(true);
                this.HumanPanel.SetActive(false);
            }
            else
            {
                this.AIPanel.SetActive(false);
                this.HumanPanel.SetActive(true);
            }
        }

        public void ToggleHumanAI()
        {
            this.ShowingProfile.IsAI = !this.ShowingProfile.IsAI;

            if (this.ShowingProfile.IsAI)
            {
                this.ShowingProfile.PlayerName = "AI";
            }
            else
            {
                this.ShowingProfile.PlayerName = "Human";
            }

                this.OpenFromPlayer(this.ShowingProfile.Index);
        }
    }
}
