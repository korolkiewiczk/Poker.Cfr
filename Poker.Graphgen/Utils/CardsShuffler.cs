using System;
using PT.Poker.Model;

namespace Poker.Graphgen.Utils
{
    internal class CardsShuffler
    {
        public static void ShuffleCards(Card[] myCards, Random random)
        {
            for (int i = 51; i > 0; i--)
            {
                int j = random.Next(i);
                Card tmp = myCards[i];
                myCards[i] = myCards[j];
                myCards[j] = tmp;
            }
        }
    }
}