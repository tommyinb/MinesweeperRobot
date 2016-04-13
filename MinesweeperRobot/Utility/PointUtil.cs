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
        public static Point Add(this Point x, Point y)
        {
            return new Point(x.X + y.X, x.Y + y.Y);
        }

        public static Point MinusOne(this Point point)
        {
            return new Point(point.X - 1, point.Y - 1);
        }

        public static Point Multiply(this Point point, Size size)
        {
            return new Point(point.X * size.Width, point.Y * size.Height);
        }
    }
}
