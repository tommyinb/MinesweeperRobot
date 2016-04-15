using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public static class PointUtil
    {
        public static IEnumerable<Point> Surrounding(this Point center)
        {
            yield return new Point(center.X - 1, center.Y);
            yield return new Point(center.X - 1, center.Y - 1);
            yield return new Point(center.X, center.Y - 1);
            yield return new Point(center.X + 1, center.Y - 1);
            yield return new Point(center.X + 1, center.Y);
            yield return new Point(center.X + 1, center.Y + 1);
            yield return new Point(center.X, center.Y + 1);
            yield return new Point(center.X - 1, center.Y + 1);
        }

        public static Point Add(this Point x, Point y)
        {
            return new Point(x.X + y.X, x.Y + y.Y);
        }
        public static Point Minus(this Point x, Point y)
        {
            return new Point(x.X - y.X, x.Y - y.Y);
        }
        public static Point Multiply(this Point point, Size size)
        {
            return new Point(point.X * size.Width, point.Y * size.Height);
        }

        public static Point MinusOne(this Point point)
        {
            return new Point(point.X - 1, point.Y - 1);
        }
    }
}
