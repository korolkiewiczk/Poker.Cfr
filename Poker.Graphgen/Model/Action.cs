using System;

namespace Poker.Graphgen.Model
{
    public struct Action
    {
        public OpType OpType { get; }
        public short NumRaise { get; }

        public static Action Initial(int b) => new Action(OpType.Raise, (short)b);

        public Action(OpType opType)
        {
            if (opType == OpType.Raise)
            {
                throw new InvalidOperationException("Call 2 arg ctor for Raise operation.");
            }

            OpType = opType;
            NumRaise = 0;
        }

        public Action(OpType opType, int numRaise)
        {
            OpType = opType;
            NumRaise = (short)numRaise;
        }

        public override string ToString()
        {
            return $"[{OpType} {NumRaise}]";
        }

        public string ToShortString()
        {
            return $"{OpType.ToString()[0]}{(NumRaise != 0 ? NumRaise.ToString() : "")}";
        }
    }
}