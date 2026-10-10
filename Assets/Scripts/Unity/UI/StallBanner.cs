using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacCOSTCO.Unity.UI
{
    public class StallBanner : MonoBehaviour
    {
        public GameObject StallBannerGO;
        public Image Colorable;
        public TMP_Text StallBannerText;

        public Color NeutralColor;
        public Color AttentionColor;

        public float TimeForAttentionState = 3f;
        private float curTimeForAttentionState { get; set; } = 0;
        public AnimationCurve AttentionStateLerpColorTimer;

        public GameConductor GameConductor;
        public TurnOrderHolder TurnOrderHolder;

        private void Awake()
        {
            this.TurnOrderHolder.OnTurnStarted += this.OnTurnUpdate;
            Colorable.color = NeutralColor;
            this.Clear(null);
        }

        public void OnTurnUpdate(int _)
        {
            Colorable.color = NeutralColor;

            if (this.GameConductor.CurrentBoardState.StallTurnEmbargoLifted())
            {
                this.StallBannerGO.gameObject.SetActive(false);
                return;
            }

            this.StallBannerGO.gameObject.SetActive(true);

            int remainingTurns = this.GameConductor.CurrentBoardState.StallTurn - this.GameConductor.CurrentBoardState.CurrentRound;

            if (remainingTurns == 1)
            {
                this.StallBannerText.text = $"No Connections For: 1 More Turn";
            }
            else
            {
                this.StallBannerText.text = $"No Connections For: {remainingTurns} More Turns";
            }
        }

        public void Clear(int? _)
        {
            this.StallBannerGO.gameObject.SetActive(false);
        }

        public void CallAttention()
        {
            this.curTimeForAttentionState = this.TimeForAttentionState;
        }

        private void Update()
        {
            if (this.curTimeForAttentionState <= 0)
            {
                Colorable.color = NeutralColor;
                return;
            }

            this.curTimeForAttentionState -= Time.deltaTime;
            Colorable.color = Color.Lerp(NeutralColor, AttentionColor, this.curTimeForAttentionState / this.TimeForAttentionState);
        }
    }
}
