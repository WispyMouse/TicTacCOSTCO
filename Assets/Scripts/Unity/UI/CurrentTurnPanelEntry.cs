using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacCOSTCO.Unity.UI
{
    public class CurrentTurnPanelEntry : MonoBehaviour
    {
        public delegate void OnShowHintsDelegate(int side);
        public delegate void OnHideHintsDelegate();

        public OnShowHintsDelegate OnShowHints;
        public OnHideHintsDelegate OnHideHints;

        public PlayerProfile RepresentedProfile { get; private set; }

        public Image PlayerIcon;

        public GameObject IsCurrentTurnIndicator;
        public GameObject KnockoutPanel;
        public GameObject IsNotTurnIndicator;

        public GameObject CascadeLevelTextHolder;
        public TMP_Text CascadeLevelText;

        public TMP_Text VisibilityText;
        public AnimationCurve CascadeLevelVisibilityCurve;
        public float TimeForCascadeLevelAtLevelOne = 1f;

        public TMP_Text NameText;
        private float _curCascadeLevelVisibility { get; set; } = 0;
        private float _curCascadeTimeLevel { get; set; } = 0;

        /// <summary>
        /// When determining how fast to make the blinking, subtract out the multiple this by the number of connections above one.
        /// This will make it blink faster.
        /// </summary>
        public float CascadeLerpPerLevel = .2f;

        public void RepresentProfile(PlayerProfile toRepresent)
        {
            this.Clear();

            this.RepresentedProfile = toRepresent;
            this.PlayerIcon.sprite = toRepresent.SpriteRepresentation;

            if (toRepresent.PlayerName == PersistentGameConfiguration.HUMAN)
            {
                this.NameText.gameObject.SetActive(false);
            }
            else
            {
                this.NameText.text = toRepresent.PlayerName;
                this.NameText.gameObject.SetActive(true);
            }
        }

        public void Clear()
        {
            this.RepresentedProfile = null;
            this.CascadeLevelTextHolder.SetActive(false);
            this.IsCurrentTurnIndicator.SetActive(false);
            this.IsNotTurnIndicator.SetActive(false);
            this.KnockoutPanel.SetActive(false);
        }

        public void SetCascade(int level)
        {
            if (level == 0)
            {
                this.CascadeLevelTextHolder.SetActive(false);
                return;
            }

            this.CascadeLevelTextHolder.SetActive(true);
            this.CascadeLevelText.text = $"x{level}";

            // Blink faster for higher cascade levels
            this._curCascadeTimeLevel = this.TimeForCascadeLevelAtLevelOne 
                - Mathf.Lerp(0, this.TimeForCascadeLevelAtLevelOne, CascadeLerpPerLevel * (level - 1));
        }

        public void MarkKnockout()
        {
            this.KnockoutPanel.SetActive(true);
            this.IsNotTurnIndicator.SetActive(false);
            this.IsCurrentTurnIndicator.SetActive(false);
        }

        // Marks the player as back in the game, visually.
        // HACK: Assumes that it would be this player's turn.
        public void RestorePlayer()
        {
            this.KnockoutPanel.SetActive(false);
            this.IsNotTurnIndicator.SetActive(false);
            this.IsCurrentTurnIndicator.SetActive(true);
        }

        public void SetCurrentTurn(bool isTurn)
        {
            // If we're knocked out, don't update the plates
            if (this.KnockoutPanel.activeSelf)
            {
                return;
            }

            this.IsCurrentTurnIndicator.SetActive(isTurn);
            this.IsNotTurnIndicator.SetActive(!isTurn);
        }

        private void Update()
        {
            if (!this.CascadeLevelTextHolder.activeSelf)
            {
                return;
            }

            _curCascadeLevelVisibility = (_curCascadeLevelVisibility + Time.deltaTime) % this._curCascadeTimeLevel;

            this.VisibilityText.color = new Color(this.VisibilityText.color.r, this.VisibilityText.color.g, this.VisibilityText.color.b,
                this.CascadeLevelVisibilityCurve.Evaluate(_curCascadeLevelVisibility / this._curCascadeTimeLevel));
        }

        public void ShowHints()
        {
            OnShowHints?.Invoke(this.RepresentedProfile.Index);
        }

        public void HideHints()
        {
            OnHideHints?.Invoke();
        }
    }
}
