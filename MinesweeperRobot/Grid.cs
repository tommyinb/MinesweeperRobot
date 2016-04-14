using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot
{
    public enum Grid
    {
        None = -10,

        Raw = -1,
        Empty = 0,

        Number1 = 1, Number2, Number3,
        Number4, Number5, Number6,
        Number7, Number8,

        Flag = 10,
        Question = 11,

        Bomb = 20
    }

    public static class GridUtil
    {
        public static bool IsNumber(this Grid grid)
        {
            return Grid.Number1 <= grid && grid <= Grid.Number8;
        }
        public static bool IsWithin(this Grid grid, Grid from, Grid to)
        {
            return from <= grid && grid <= to;
        }
    }
}
