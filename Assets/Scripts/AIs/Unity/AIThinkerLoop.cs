namespace TicTacCOSTCO.Unity.UI
{
    using System.Collections;
    using TicTacCOSTCO.DataStructures;
    using UnityEngine;

    public class AIThinkerLoop : MonoBehaviour
    {
        public AICore BasicAICore;

        public GameConductor GameConductor;
        public TurnOrderHolder TurnOrderHolder;

        private Coroutine ThinkingCoroutine { get; set; }

        public float TimeForAIToThinkBase = .1f;
        public AnimationCurve TimeForAIToThinkAdditionalSeconds;
        public AnimationCurve AdditionalTimeDuringCascade;

        private void Awake()
        {
            this.TurnOrderHolder.OnTurnStarted += OnTurnStarted;
        }

        private void OnTurnStarted(int turn)
        {
            if (ThinkingCoroutine != null)
            {
                StopCoroutine(ThinkingCoroutine);
                ThinkingCoroutine = null;
            }

            if (!this.GameConductor.CurrentGameState.CurrentBoardState.AnyEmptySpots())
            {
                return;
            }

            if (this.GameConductor.CurrentGameState == null || this.GameConductor.CurrentGameState.CurrentBoardState.CurrentGameState == BoardState.GameStateEnum.End)
            {
                return;
            }

            // If the current player is human, do nothing
            if (this.TurnOrderHolder.PlayerIsHuman(this.GameConductor.CurrentGameState.CurrentBoardState.CurrentPlayerIndex))
            {
                return;
            }

            // If this side has been eliminated, skip them
            if (!this.GameConductor.CurrentGameState.CurrentBoardState.SideIndexesStillInGame.Contains(this.GameConductor.CurrentGameState.CurrentBoardState.CurrentPlayerIndex))
            {
                return;
            }

            // If there are no possible moves, do nothing
            if (!this.GameConductor.CurrentGameState.CurrentBoardState.AnyEmptySpots())
            {
                return;
            }

            this.ThinkingCoroutine = StartCoroutine(AIThinksAndTakesTurn());
        }

        IEnumerator AIThinksAndTakesTurn()
        {
            float randomWait = TimeForAIToThinkAdditionalSeconds.Evaluate(Random.Range(0, 1f));

            if (this.GameConductor.CurrentGameState.CurrentBoardState.CurrentGameState == BoardState.GameStateEnum.Cascade)
            {
                randomWait += AdditionalTimeDuringCascade.Evaluate(Random.Range(0, 1f));
            }

            yield return new WaitForSeconds(this.TimeForAIToThinkBase + randomWait);

            if (this.GameConductor.CurrentGameState.CurrentBoardState.CurrentGameState == BoardState.GameStateEnum.End)
            {
                yield break;
            }

            this.ThinkingCoroutine = null;
            Coordinate move =
                PersistentGameConfiguration.Singleton.Players[this.GameConductor.CurrentGameState.CurrentBoardState.CurrentPlayerIndex].AICore
                .DetermineMove(this.GameConductor.CurrentGameState.CurrentBoardState.CurrentPlayerIndex, this.GameConductor.CurrentGameState.CurrentBoardState);
            this.GameConductor.ChooseCell(move);
        }
    }
}