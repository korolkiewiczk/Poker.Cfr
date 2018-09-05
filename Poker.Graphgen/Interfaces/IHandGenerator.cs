using Poker.Graphgen.Model;

namespace Poker.Graphgen.Interfaces
{
    public interface IHandGenerator
    {
        HandInfo GenerateRandomHand();
    }
}