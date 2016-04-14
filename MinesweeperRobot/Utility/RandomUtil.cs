using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public static class RandomUtil
    {
        private static Lazy<Random> random = new Lazy<Random>();

        public static T[] Randomize<T>(this T[] items)
        {
            return items.OrderBy(t => random.Value.Next(items.Length)).ToArray();
        }
    }
}
