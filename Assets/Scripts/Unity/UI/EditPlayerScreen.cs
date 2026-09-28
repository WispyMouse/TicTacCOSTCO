namespace TicTacCOSTCO.Unity.UI
{
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class EditPlayerScreen : MonoBehaviour
    {
        public PlayerProfile ShowingProfile { get; private set; }

        public Image PlayerSpriteDisplay;

        public GameObject HumanPanel;
        public TMP_InputField NameInputField;

        public GameObject AIPanel;

        public GameObject AlreadyTakenPanel;

        public TMP_Text AICoreLabel;

        public void OpenFromPlayer(int player)
        {
            this.AlreadyTakenPanel.SetActive(false);
            this.gameObject.SetActive(true);

            this.ShowingProfile = PersistentGameConfiguration.Singleton.Players[player];

            if (this.ShowingProfile.PlayerName == PersistentGameConfiguration.HUMAN)
            {
                // Show the placeholder text
                this.NameInputField.SetTextWithoutNotify(string.Empty);
            }
            else
            {
                this.NameInputField.SetTextWithoutNotify(this.ShowingProfile.PlayerName);
            }

            this.PlayerSpriteDisplay.sprite = this.ShowingProfile.SpriteRepresentation;

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

            this.UpdateIcon();
            this.UpdateAICore();
        }

        public void ToggleHumanAI()
        {
            this.ShowingProfile.IsAI = !this.ShowingProfile.IsAI;

            if (this.ShowingProfile.IsAI)
            {
                this.ShowingProfile.AICoreIndex = 0;
                this.ShowingProfile.PlayerName = this.ShowingProfile.AICore.name;
            }
            else
            {
                this.ShowingProfile.PlayerName = PersistentGameConfiguration.HUMAN;
            }

            this.OpenFromPlayer(this.ShowingProfile.Index);
        }

        public void NameChanged(string newName)
        {
            this.ShowingProfile.PlayerName = newName;
        }

        public void NextIcon()
        {
            int nextIndex = (this.ShowingProfile.RepresenterSpriteIndex + 1) % PersistentGameConfiguration.Singleton.PlayerIconOptions.Count;
            this.ShowingProfile.RepresenterSpriteIndex = nextIndex;
            this.UpdateIcon();
        }

        public void PreviousIcon()
        {
            int previousIndex = (this.ShowingProfile.RepresenterSpriteIndex + PersistentGameConfiguration.Singleton.PlayerIconOptions.Count - 1) % PersistentGameConfiguration.Singleton.PlayerIconOptions.Count;
            this.ShowingProfile.RepresenterSpriteIndex = previousIndex;
            this.UpdateIcon();
        }

        private void UpdateIcon()
        {
            this.PlayerSpriteDisplay.sprite = this.ShowingProfile.SpriteRepresentation;

            int countWithIndex = 0;

            for (int ii = 0, count = PersistentGameConfiguration.Singleton.Players.Count; ii < count; ii ++)
            {
                if (PersistentGameConfiguration.Singleton.Players[ii].RepresenterSpriteIndex == this.ShowingProfile.RepresenterSpriteIndex)
                {
                    countWithIndex++;
                }
            }

            // If there was more than one with this icon, we can't allow you to select it
            this.AlreadyTakenPanel.SetActive(countWithIndex > 1);
        }

        public void NextAICore()
        {
            this.ShowingProfile.AICoreIndex = (this.ShowingProfile.AICoreIndex + 1) % PersistentGameConfiguration.Singleton.AICores.Count;
            this.UpdateAICore();
        }

        public void PreviousAICore()
        {
            this.ShowingProfile.AICoreIndex = (this.ShowingProfile.AICoreIndex + PersistentGameConfiguration.Singleton.AICores.Count - 1) % PersistentGameConfiguration.Singleton.AICores.Count;
            this.UpdateAICore();
        }

        void UpdateAICore()
        {
            if (this.ShowingProfile.IsAI)
            {
                this.AICoreLabel.gameObject.SetActive(true);
                this.ShowingProfile.PlayerName = this.ShowingProfile.AICore.name;
                this.AICoreLabel.text = this.ShowingProfile.AICore.name;
            }
            else
            {
                this.AICoreLabel.gameObject.SetActive(false);
            }
        }
    }
}
