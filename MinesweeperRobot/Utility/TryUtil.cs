using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public static class TryUtil
    {
        public static void Invoke<TException>(this Action action, int numberOfTry) where TException : Exception
        {
            for (int i = 0; i < numberOfTry - 1; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (TException) { }

                Console.WriteLine("Retry!!");
            }

            action();
        }
    }
}
