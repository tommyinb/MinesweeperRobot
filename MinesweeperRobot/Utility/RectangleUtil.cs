using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public static class RectangleUtil
    {
        public static Rectangle Add(this Rectangle rectangle, Point point)
        {
            return new Rectangle(rectangle.Location.Add(point), rectangle.Size);
        }

        public static Point Center(this Rectangle rectangle)
        {
            return new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
        }
    }
}
