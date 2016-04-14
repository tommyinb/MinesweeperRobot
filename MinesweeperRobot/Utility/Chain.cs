using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public class Chain<T> : IEnumerable<T>
    {
        public Chain(IEnumerable<T> items)
        {
            this.items = new List<T>(items);
        }
        private List<T> items;

        public override bool Equals(object obj)
        {
            if (obj is Chain<T> == false) return false;

            var chain = (Chain<T>)obj;
            return this.SequenceEqual(chain);
        }
        public override int GetHashCode()
        {
            return items.Aggregate(0, (total, curr) =>
                total ^ (curr?.GetHashCode() ?? 0));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }

    public static class ChainUtil
    {
        public static Chain<T> ToChain<T>(this IEnumerable<T> source)
        {
            return new Chain<T>(source);
        }
    }
}
