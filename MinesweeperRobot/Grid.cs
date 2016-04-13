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
        Bomb = 11
    }
}
