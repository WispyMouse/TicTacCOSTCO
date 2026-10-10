namespace TicTacCOSTCO.DataStructures
{
    public struct GameConfiguration
    {
        public int Width;
        public int Height;
        public int PlayerCount;
        public int InARowToSolve;
        public int StallTurn;

        public GameConfiguration(int width, int height, int playerCount, int inARowToSolve, int stallTurn)
        {
            this.Width = width;
            this.Height = height;
            this.PlayerCount = playerCount;
            this.InARowToSolve = inARowToSolve;
            this.StallTurn = stallTurn;
        }
    }
}
