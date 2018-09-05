using System;
using System.Collections.Generic;
using System.Linq;
using Poker.Graphgen.Model;
using Action = Poker.Graphgen.Model.Action;

namespace Poker.Graphgen
{
    public class NodeGen
    {
        private readonly int[][] _possibleRaises;
        private readonly int _reraiseAmount;
        private readonly int _numPlayers;
        private readonly int _sbValue;
        private readonly int _bbValue;
        private readonly int _bankroll;

        private NodeGen(int[][] possibleRaises, int reraiseAmount, int numPlayers, int sbValue, int bbValue, int bankroll)
        {
            if (possibleRaises.Length != 4)
            {
                throw new Exception("Number of raises types must be 4 (PF,F,T,R)");
            }

            _possibleRaises = possibleRaises;
            _reraiseAmount = reraiseAmount;
            _numPlayers = numPlayers;
            _sbValue = sbValue;
            _bbValue = bbValue;
            _bankroll = bankroll;
        }

        public NodeGen(NodeGenConfig config)
            : this(config.PossibleRaises, config.ReraiseAmount, config.NumPlayers, config.SbValue, config.BbValue, config.Bankroll)
        {

        }

        public Node Generate()
        {
            int[] invest = GetInvestValues();

            Node childNode = Generate_rek(_numPlayers - 1, Action.Initial(_sbValue), Section.PF, _reraiseAmount + 1, Action.Initial(_sbValue), _sbValue + _bbValue, invest);

            Node rootNode = new Node((byte)(_numPlayers - 2), Action.Initial(_sbValue), Section.PF, new[] { childNode }, invest.Max());

            return rootNode;
        }

        private int[] GetInvestValues()
        {
            int[] invest = new int[_numPlayers];
            invest[_numPlayers - 1] = _bbValue;
            invest[_numPlayers - 2] = _sbValue;
            return invest;
        }

        private Node Generate_rek(int player, Action action, Section section, int reraiseAmount, Action prevAction, int pot, int[] invest)
        {
            if (player == 1 && action.OpType == OpType.Call && section == Section.PF)
            {
                reraiseAmount = _reraiseAmount;
            }

            Action[] actions = GetPossibleActions(section, reraiseAmount, action);

            List<Node> nodes = new List<Node>(actions.Length);
            for (int i = 0; i < actions.Length; i++)
            {
                var newSection = GetNewSection(player, actions[i], section, action, prevAction, reraiseAmount);

                bool isNewSection = newSection < Section.Show && newSection != section;
                int nextOpp = NextOpp(player, isNewSection);

                if (actions[i].OpType == OpType.All)
                {
                    actions[i] = new Action(OpType.All, actions[i].NumRaise - invest.Max());
                }

                int[] newInvest = new int[invest.Length];
                Array.Copy(invest, newInvest, invest.Length);
                int investValue = GetInvestValue(actions[i], action);
                newInvest[nextOpp] += investValue;
                int diff = action.OpType == OpType.Raise ? 1 : 0;
                nodes.Add(Generate_rek(nextOpp, actions[i], newSection,
                    newSection != section ? _reraiseAmount : reraiseAmount - diff, action,
                    pot + investValue, newInvest));
            }

            return new Node((byte)player, action, section, nodes.ToArray(),
                GetPayOff(player, section, pot, invest));
        }

        private static int GetPayOff(int player, Section section, int pot, int[] invest)
        {
            if (Node.IsTerminal(section))
            {
                if (section == Section.Fold)
                {
                    invest[player] += pot;
                }
                return invest.Select(x => pot - x).Max();
            }

            return 0;
        }

        private int GetInvestValue(Action newAction, Action currentAction)
        {
            switch (newAction.OpType)
            {
                case OpType.Fold:
                    return 0;
                case OpType.Call:
                    return currentAction.NumRaise;
                case OpType.Raise:
                case OpType.All:
                    return currentAction.NumRaise + newAction.NumRaise;
            }

            return 0;
        }

        private Section GetNewSection(int player, Action newAction, Section section, Action currentAction, Action prevAction, int reraiseAmount)
        {
            if (newAction.OpType == OpType.Fold) return Section.Fold;

            if (newAction.OpType == OpType.Call && currentAction.OpType == OpType.All
                || section == Section.R && NextOpp(player) == 0 && newAction.OpType == OpType.Call
                || section == Section.R && NextOpp(player) == 1 && newAction.OpType == OpType.Call && currentAction.OpType == OpType.Raise)
            {
                return Section.Show;
            }

            if (section > Section.PF)
            {
                if (player == 0 && currentAction.OpType == OpType.Call
                    || player == 1 && currentAction.OpType == OpType.Call && prevAction.OpType == OpType.Raise)
                {
                    return section + 1;
                }
            }
            else
            {
                if (player == 1 && currentAction.OpType == OpType.Call
                    || player == 0 && currentAction.OpType == OpType.Call && prevAction.OpType == OpType.Raise && reraiseAmount != _reraiseAmount)
                {
                    return section + 1;
                }
            }

            return section;
        }

        private Action[] GetPossibleActions(Section section, int ramount, Action currentAction)
        {
            if (section == Section.Fold || section == Section.Show)
            {
                return new Action[0];
            }

            if (currentAction.OpType == OpType.All)
            {
                return new[]
                {
                    new Action(OpType.Fold),
                    new Action(OpType.Call)
                };
            }

            if (ramount <= 0)
            {
                if (currentAction.OpType == OpType.Call)
                {
                    return new[]
                    {
                        new Action(OpType.Call)
                    };
                }
                else
                {
                    return new[]
                    {
                        new Action(OpType.Fold),
                        new Action(OpType.Call)
                    };
                }
            }

            var actions = new List<Action>
            {
                new Action(OpType.Call),
                new Action(OpType.All, _bankroll)
            };

            if (currentAction.OpType != OpType.Call)
            {
                actions.Insert(0, new Action(OpType.Fold));
            }

            actions.AddRange(_possibleRaises[(int) section].Select(possibleRaise => new Action(OpType.Raise, possibleRaise)));

            return actions.ToArray();
        }

        private int NextOpp(int player, bool isNewSection = false) => isNewSection ? _numPlayers - 1 : (player == 0 ? _numPlayers - 1 : player - 1);
    }
}
