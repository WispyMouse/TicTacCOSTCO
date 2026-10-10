namespace TicTacCOSTCO.DataStructures
{
    using System;
    using System.Collections.Generic;

    public class BoardStateHolder
    {
        public const int MINIMUMPLAYERS = 2;

        public readonly BoardState CurrentBoardState;

        public delegate void OnPlayerTurnDelegate(int turn);
        public OnPlayerTurnDelegate OnPlayerStartTurn;
        public OnPlayerTurnDelegate OnPlayerMadeMove;

        public delegate void OnMoveUndoneDelegate(MoveCommand undone);
        public OnMoveUndoneDelegate OnMoveUndone;

        public delegate void OnGameConclusionDelegate(int? winner);
        public OnGameConclusionDelegate OnGameConclusion;

        public IReadOnlyList<MoveCommand> MoveCommandsApplied => this._MoveCommandsApplied;
        private List<MoveCommand> _MoveCommandsApplied { get; set; } = new List<MoveCommand>();

        public int Width => this.CurrentBoardState.Width;
        public int Height => this.CurrentBoardState.Height;
        public int PlayerCount => this.CurrentBoardState.PlayerCount;
        public int InARowToSolve => this.CurrentBoardState.InARowToSolve;

        public BoardStateHolder(GameConfiguration gameConfiguration, bool forceZeroIndexStart = false)
        {
            int firstToMove = 0;

            if (forceZeroIndexStart)
            {
                // Force the first player to be zero, perhaps because we're in test mode
                firstToMove = 0;
            }
            else
            {
                // Choose a random player to go first
                firstToMove = new Random().Next(gameConfiguration.PlayerCount);
            }

            this.CurrentBoardState = new BoardState(gameConfiguration, firstToMove);
        }

        public bool TryApplyMoveCommand(MoveCommand toApply, bool advancePlayer = true)
        {
            bool applied = this.CurrentBoardState.TryApplyMoveCommand(toApply);

            if (!applied)
            {
                return false;
            }

            this._MoveCommandsApplied.Add(toApply);

            if (advancePlayer)
            {
                AdvancePlayer();
            }

            if (this.CurrentBoardState.CurrentGameState == BoardState.GameStateEnum.End)
            {
                this.OnGameConclusion?.Invoke(this.CurrentBoardState.Winner);
            }
            return true;
        }

        /// <summary>
        /// Reverse the previous move command, entirely undoing everything it did.
        /// The players in the game should be in the state they were before the move, as though it was never run.
        /// </summary>
        /// <returns>
        /// Returns the <see cref="MoveCommand"/> that was undone from <see cref="MoveCommandsApplied"/>.
        /// Returns null if there was nothing that could be removed.
        /// </returns>
        public MoveCommand ReversePreviousMoveCommand()
        {
            int moveCommandsApplied = this._MoveCommandsApplied.Count;
            if (moveCommandsApplied == 0)
            {
                return null;
            }

            MoveCommand toRemove = this.MoveCommandsApplied[moveCommandsApplied - 1];
            this._MoveCommandsApplied.RemoveAt(moveCommandsApplied - 1);

            // HACK: At this point in the game's development, the only thing that *could* be placed during a reverse is a null
            // Will need to track what it used to be, if we can make it any other value while reversing
            this.CurrentBoardState.ForceMarkOwnership(toRemove.Position, null);
            this.CurrentBoardState.CurrentCascadeLevel = toRemove.PreviousCascadeLevel;

            foreach (int playerRemoved in toRemove.PlayersRemoved)
            {
                this.CurrentBoardState.SideIndexesStillInGame.Add(playerRemoved);
            }

            // Remove any added connection
            foreach (CellsConnection connectionsAdded in toRemove.ConnectionsMade)
            {
                foreach (Coordinate coordinate in connectionsAdded.Cells)
                {
                    this.CurrentBoardState.RemoveConnection(coordinate, connectionsAdded);
                }
            }

            if (this.MoveCommandsApplied.Count == 0)
            {
                this.CurrentBoardState.CurrentGameState = BoardState.GameStateEnum.NotStarted;
            }
            else if (this.CurrentBoardState.CurrentCascadeLevel > 0)
            {
                this.CurrentBoardState.CurrentGameState = BoardState.GameStateEnum.Cascade;
            }
            else
            {
                this.CurrentBoardState.CurrentGameState = BoardState.GameStateEnum.Playing;
            }

            this.AdvancePlayer(reversePlayer: true);
            // We certainly no longer have a winner
            this.CurrentBoardState.Winner = null;

            this.OnMoveUndone?.Invoke(toRemove);

            return toRemove;
        }

        public void AdvancePlayer(bool reversePlayer = false)
        {
            this.OnPlayerMadeMove?.Invoke(this.CurrentBoardState.CurrentPlayerIndex);

            // If we're advancing, add one. Reversing, minus one
            // This will modulo around the player count and let us find the next valid player
            int advancer = reversePlayer ? -1 : 1;

            for (int ii = 1; ii < this.CurrentBoardState.PlayerCount; ii++)
            {
                int previousPlayer = (this.CurrentBoardState.CurrentPlayerIndex + (advancer * (ii - 1)) + this.CurrentBoardState.PlayerCount) % this.CurrentBoardState.PlayerCount;
                int nextProspectivePlayer = (this.CurrentBoardState.CurrentPlayerIndex + (advancer * ii) + this.CurrentBoardState.PlayerCount) % this.CurrentBoardState.PlayerCount;

                // If the previous player is the starting player, and we're reversing, go back a round
                if (reversePlayer && previousPlayer == this.CurrentBoardState.FirstPlayerToMove)
                {
                    this.CurrentBoardState.CurrentRound--;
                }

                // If we're going forward, and it would become the starting player's turn, go forward a round
                if (!reversePlayer && nextProspectivePlayer == this.CurrentBoardState.FirstPlayerToMove)
                {
                    this.CurrentBoardState.CurrentRound++;
                }

                if (!this.CurrentBoardState.SideIndexesStillInGame.Contains(nextProspectivePlayer))
                {
                    continue;
                }

                this.CurrentBoardState.CurrentPlayerIndex = nextProspectivePlayer;
                this.OnPlayerStartTurn?.Invoke(this.CurrentBoardState.CurrentPlayerIndex);
                break;
            }
        }

        public bool SpotIsInBounds(Coordinate position) => this.CurrentBoardState.SpotIsInBounds(position);
    }
}
