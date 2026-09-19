using NUnit.Framework;
using System.Collections;
using Unity.VisualScripting;
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
        if (!this.GameConductor.CurrentGameState.AnyEmptySpots())
        {
            return;
        }

        if (this.GameConductor.CurrentGameState == null || this.GameConductor.CurrentGameState.CurrentGameState == GameState.GameStateEnum.End)
        {
            return;
        }

        // If the current player is human, do nothing
        if (this.TurnOrderHolder.PlayerIsHuman(this.TurnOrderHolder.CurrentPlayerIndex))
        {
            return;
        }

        // If there are no possible moves, do nothing
        if (!this.GameConductor.CurrentGameState.AnyEmptySpots())
        {
            return;
        }

        this.ThinkingCoroutine = StartCoroutine(AIThinksAndTakesTurn());
    }

    IEnumerator AIThinksAndTakesTurn()
    {
        float randomWait = TimeForAIToThinkAdditionalSeconds.Evaluate(Random.Range(0, 1f));

        if (this.GameConductor.CurrentGameState.CurrentGameState == GameState.GameStateEnum.Cascade)
        {
            randomWait += AdditionalTimeDuringCascade.Evaluate(Random.Range(0, 1f));
        }

        yield return new WaitForSeconds(this.TimeForAIToThinkBase + randomWait);

        if (this.GameConductor.CurrentGameState.CurrentGameState == GameState.GameStateEnum.End)
        {
            yield break;
        }

        Vector2Int move = this.BasicAICore.DetermineMove(this.TurnOrderHolder.CurrentPlayerIndex, this.GameConductor.CurrentGameState);
        this.GameConductor.ChooseCell(move);

        this.ThinkingCoroutine = null;
    }
}
