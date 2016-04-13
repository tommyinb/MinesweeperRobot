using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public static class SizeUtil
    {
        public static bool Contains(this Size size, Point point)
        {
            return point.X >= 0 && point.Y >= 0
                && point.X < size.Width && point.Y < size.Height;
        }

        public static bool Contains(this Size bigSize, Point point, Size smallSize)
        {
            return point.X >= 0 && point.Y >= 0
                && point.X + smallSize.Width - 1 < bigSize.Width
                && point.Y + smallSize.Height - 1 < bigSize.Height;
        }
    }
}
