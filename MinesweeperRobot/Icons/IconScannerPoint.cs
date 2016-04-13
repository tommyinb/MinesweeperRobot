using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Icons
{
    public struct IconScannerPoint
    {
        public Point Point;

        public IconScannerValue[] Values;
    }

    public struct IconScannerValue
    {
        public uint Pixel;

        public Icon[] Icons;
    }
}
