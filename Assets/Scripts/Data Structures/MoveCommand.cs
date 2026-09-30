namespace TicTacCOSTCO.DataStructures
{
    using System.Collections.Generic;

    public class MoveCommand
    {
        public int SideIndex;
        public Coordinate Position;

        public IReadOnlyList<CellsConnection> ConnectionsMade;

        public int PreviousCascadeLevel;

        public IReadOnlyList<int> PlayersRemoved;

        public MoveCommand(int sideIndex, Coordinate position, IReadOnlyList<CellsConnection> connectionsMade, int previousCascade, IReadOnlyList<int> playersRemoved)
        {
            SideIndex = sideIndex;
            Position = position;
            ConnectionsMade = connectionsMade;
            PreviousCascadeLevel = previousCascade;
            this.PlayersRemoved = playersRemoved;
        }
    }
}
