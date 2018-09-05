using Poker.Graphgen.Interfaces;

namespace Poker.Graphgen.Cfr
{
    public class CfrPlusFactory : ICfrFactory
    {
        public ICfr Create(int player, int hand, int winningPlayer)
        {
            return new CfrPlus(player, hand, winningPlayer);
        }
    }
}
