namespace Poker.Graphgen
{
    public class NodeGenConfig
    {
        public int[][] PossibleRaises { get; set; }

        public int ReraiseAmount { get; set; }

        public int NumPlayers { get; set; }

        public int SbValue { get; set; }

        public int BbValue { get; set; }

        public int Bankroll { get; set; }
    }
}
