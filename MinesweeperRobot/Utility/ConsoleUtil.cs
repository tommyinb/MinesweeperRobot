using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public static class ConsoleUtil
    {
        public static void WriteLine(ConsoleColor color, string value)
        {
            var restoreColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;

                Console.WriteLine(value);
            }
            finally
            {
                Console.ForegroundColor = restoreColor;
            }
        }
    }
}
