using System.Text;
using TMPro;
using UnityEngine;

namespace TicTacCOSTCO.Unity.UI
{
    public class GameDescriber : MonoBehaviour
    {
        public TMP_Text DescriptionText;

        public void OnEnable()
        {
            PersistentGameConfiguration.Singleton.OnGameplaySettingsUpdated += UpdateDescription;
            this.UpdateDescription();
        }

        public void OnDisable()
        {
            PersistentGameConfiguration.Singleton.OnGameplaySettingsUpdated -= UpdateDescription;
        }

        public void UpdateDescription()
        {
            StringBuilder description = new StringBuilder();

            description.AppendLine($"{PersistentGameConfiguration.Singleton.Players.Count} players");
            description.AppendLine($"{PersistentGameConfiguration.Singleton.Width}x{PersistentGameConfiguration.Singleton.Height}");

            if (PersistentGameConfiguration.Singleton.StallTurn > 0)
            {
                description.AppendLine($"No connections until turn {PersistentGameConfiguration.Singleton.StallTurn}");
            }

            bool appendVs = false;
            foreach (PlayerProfile curPlayer in PersistentGameConfiguration.Singleton.Players)
            {
                if (appendVs)
                {
                    description.Append($" vs ");
                }

                description.Append($"{curPlayer.PlayerName}");

                appendVs = true;
            }

            this.DescriptionText.text = description.ToString();
        }
    }
}
