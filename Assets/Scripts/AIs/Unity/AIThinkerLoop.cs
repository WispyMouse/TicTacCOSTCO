namespace TicTacCOSTCO.Unity.UI
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using TicTacCOSTCO.AIs.Search;
    using TicTacCOSTCO.DataStructures;
    using UnityEngine;
    using Random = UnityEngine.Random;

    public class AIThinkerLoop : MonoBehaviour
    {
        public AICore BasicAICore;
        public GameConductor GameConductor;
        public TurnOrderHolder TurnOrderHolder;
        public float TimeForAIToThinkBase = .1f;
        public AnimationCurve TimeForAIToThinkAdditionalSeconds;
        public AnimationCurve AdditionalTimeDuringCascade;
        private Coroutine thinkingCoroutine;
        private CascadeSearch search;
        private int generation;
        public int LastSearchWorkUnits { get; private set; }
        public int LastSearchSimulations { get; private set; }
        public SearchStopReason LastSearchStopReason { get; private set; }
        public double LastSearchMaxSliceMilliseconds { get; private set; }

        private void OnEnable()
        {
            TurnOrderHolder.OnTurnStarted += OnTurnStarted;
            GameConductor.OnGameReset += CancelThinking;
            if (GameConductor.CurrentGameState != null)
                OnTurnStarted(GameConductor.CurrentGameState.CurrentPlayerIndex);
        }

        private void OnDisable()
        {
            TurnOrderHolder.OnTurnStarted -= OnTurnStarted;
            GameConductor.OnGameReset -= CancelThinking;
            CancelThinking();
        }

        private void CancelThinking()
        {
            generation++;
            search?.Dispose();
            search = null;
            if (thinkingCoroutine != null) StopCoroutine(thinkingCoroutine);
            thinkingCoroutine = null;
        }

        private void OnTurnStarted(int turn)
        {
            CancelThinking();
            var game = GameConductor.CurrentGameState;
            if (BasicAICore == null || game == null || game.CurrentGameState == GameState.GameStateEnum.End ||
                !game.AnyEmptySpots() || TurnOrderHolder.PlayerIsHuman(game.CurrentPlayerIndex) ||
                !game.SideIndexesStillInGame.Contains(game.CurrentPlayerIndex)) return;
            thinkingCoroutine = StartCoroutine(Think(game, game.CurrentPlayerIndex, GameConductor.StateVersion, generation));
        }

        private bool StillCurrent(GameState game, int player, long version, int ticket)
        {
            return ticket == generation && ReferenceEquals(game, GameConductor.CurrentGameState) &&
                version == GameConductor.StateVersion && game.CurrentPlayerIndex == player &&
                game.CurrentGameState != GameState.GameStateEnum.End &&
                game.SideIndexesStillInGame.Contains(player) && !TurnOrderHolder.PlayerIsHuman(player);
        }

        private IEnumerator Think(GameState game, int player, long version, int ticket)
        {
            float delay = TimeForAIToThinkBase + (TimeForAIToThinkAdditionalSeconds?.Evaluate(Random.value) ?? 0);
            if (game.CurrentGameState == GameState.GameStateEnum.Cascade)
                delay += AdditionalTimeDuringCascade?.Evaluate(Random.value) ?? 0;
            yield return new WaitForSeconds(Mathf.Max(0, delay));
            if (!StillCurrent(game, player, version, ticket)) yield break;

            Coordinate? move;
            if (BasicAICore is AISearchCore core)
            {
                search = core.BeginSearch(player, game);
                LastSearchMaxSliceMilliseconds = 0;
                var deadline = Stopwatch.StartNew();
                double frameBudget = float.IsNaN(core.FrameBudgetMilliseconds) ? 2 :
                    Mathf.Clamp(core.FrameBudgetMilliseconds, 0.1f, 8f);
                while (!search.IsComplete)
                {
                    var frame = Stopwatch.StartNew();
                    do
                    {
                        if (deadline.Elapsed.TotalMilliseconds >= Math.Max(1, core.MoveTimeLimitMilliseconds))
                        {
                            search.Stop(SearchStopReason.Deadline);
                            break;
                        }
                        search.Step(1);
                    } while (!search.IsComplete && frame.Elapsed.TotalMilliseconds < frameBudget);
                    LastSearchMaxSliceMilliseconds = Math.Max(LastSearchMaxSliceMilliseconds, frame.Elapsed.TotalMilliseconds);
                    if (search.IsComplete) break;
                    yield return null;
                    if (!StillCurrent(game, player, version, ticket))
                    {
                        search?.Dispose();
                        search = null;
                        thinkingCoroutine = null;
                        yield break;
                    }
                }
                move = search.BestMove;
                LastSearchWorkUnits = search.WorkUnits;
                LastSearchSimulations = search.Simulations;
                LastSearchStopReason = search.StopReason;
                search.Dispose();
                search = null;
            }
            else move = BasicAICore.DetermineMove(player, game);

            // Clear before ChooseCell: its turn event may synchronously start the next CPU.
            thinkingCoroutine = null;
            if (move.HasValue && StillCurrent(game, player, version, ticket) &&
                game.SpotToSideOwnership.TryGetValue(move.Value, out int? owner) && !owner.HasValue)
                GameConductor.ChooseCell(move.Value);
        }
    }
}
