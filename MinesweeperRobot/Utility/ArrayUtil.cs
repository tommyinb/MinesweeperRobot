using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public static class ArrayUtil
    {
        public static Size GetSize<T>(this T[,] array)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);
            return new Size(width, height);
        }
    }
}
