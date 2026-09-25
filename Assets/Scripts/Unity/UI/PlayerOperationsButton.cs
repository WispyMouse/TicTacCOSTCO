using TicTacCOSTCO.DataStructures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacCOSTCO.Unity.UI
{
    public class PlayerOperationsButton : MonoBehaviour
    {
        public GameObject IconRepresenterBase;
        public Image IconOfPlayer;
        public TMP_Text TextOfPlayer;

        public GameObject ConfigureButtonBase;
        public GameObject PlusButtonBase;
        public GameObject MinusButtonBase;

        public PlayerProfile PlayerProfile { get; private set; }
        [SerializeReference]
        private PlayerConfiguratorSelector PlayerConfiguratorSelector;

        private void Awake()
        {
            this.ConfigureButtonBase.SetActive(false);
            this.PlusButtonBase.SetActive(true);
            this.MinusButtonBase.SetActive(false);
            this.IconRepresenterBase.SetActive(false);
            this.TextOfPlayer.gameObject.SetActive(false);
        }

        public void SetFromProfile(PlayerProfile profile)
        {
            this.PlayerProfile = profile;
            this.ConfigureButtonBase.SetActive(true);
            this.PlusButtonBase.SetActive(false);

            // Don't show minus for minimum players
            this.MinusButtonBase.SetActive(PersistentGameConfiguration.Singleton.Players.Count > GameState.MINIMUMPLAYERS);

            this.IconOfPlayer.sprite = profile.RepresenterSprite;
            this.IconRepresenterBase.SetActive(true);
            this.TextOfPlayer.gameObject.SetActive(true);
        }

        public void OnMinus()
        {
            PersistentGameConfiguration.Singleton.RemovePlayer(this.PlayerProfile);
            this.PlayerProfile = null;
            this.ConfigureButtonBase.SetActive(false);
            this.PlusButtonBase.SetActive(true);
            this.MinusButtonBase.SetActive(false);
            this.IconRepresenterBase.SetActive(false);
            this.TextOfPlayer.gameObject.SetActive(false);

            this.PlayerConfiguratorSelector.UpdateFromMinusOrPlus();
        }

        public void OnPlus()
        {
            SetFromProfile(PersistentGameConfiguration.Singleton.AddPlayer());
            this.PlayerConfiguratorSelector.UpdateFromMinusOrPlus();
        }

        public void Deactivate(bool showPlus)
        {
            this.PlayerProfile = null;
            this.ConfigureButtonBase.SetActive(false);
            this.PlusButtonBase.SetActive(showPlus);
            this.MinusButtonBase.SetActive(false);
            this.IconRepresenterBase.SetActive(false);
            this.TextOfPlayer.gameObject.SetActive(false);
        }
    }
}
