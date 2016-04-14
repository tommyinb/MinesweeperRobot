using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public static class EnumerableUtil
    {
        public static IEnumerable<Point> Rectangle(Size size)
        {
            return Rectangle(size.Width, size.Height);
        }
        public static IEnumerable<Point> Rectangle(int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    yield return new Point(i, j);
                }
            }
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, int, TKey> keySelector, Func<TSource, int, TElement> elementSelector)
        {
            var pairs = source.Select((t, i) => new
            {
                key = keySelector(t, i),
                element = elementSelector(t, i)
            });

            return pairs.ToDictionary(t => t.key, t => t.element);
        }
    }
}
