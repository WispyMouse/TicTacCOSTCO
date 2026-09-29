namespace TicTacCOSTCO.DataStructures
{
    using System.Collections.Generic;

    public class MoveCommand
    {
        public int SideIndex;
        public Coordinate Position;

        public List<CellsConnection> ConnectionsMade;

        public int PreviousCascadeLevel;

        public List<int> PlayersRemoved;

        public MoveCommand(int sideIndex, Coordinate position, List<CellsConnection> connectionsMade, int previousCascade, List<int> playersRemoved)
        {
            SideIndex = sideIndex;
            Position = position;
            ConnectionsMade = connectionsMade;
            PreviousCascadeLevel = previousCascade;
            this.PlayersRemoved = playersRemoved;
        }
    }
}
