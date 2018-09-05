using Poker.Graphgen.Model;

namespace Poker.Graphgen.Interfaces
{
    public interface ICfr
    {
        float Compute(Node node, float op);
    }
}