using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Strategy
{
    public class GuessGrid
    {
        public Point Point;

        public GuessValue Value;

        public double Confidence;
    }

    public enum GuessValue
    {
        Empty, Bomb
    }
}
