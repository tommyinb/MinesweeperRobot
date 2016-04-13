using MinesweeperRobot.Icons;
using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinesweeperRobot
{
    class Program
    {
        static void Main(string[] args)
        {
            var processes = Process.GetProcesses();
            var process = processes.FirstOrDefault(t => t.MainWindowTitle == "Minesweeper");
            if (process == null)
            {
                process = Process.Start("Winmine__XP.exe");
                process.WaitForInputIdle();
            }

            Thread.Sleep(1000);

            var board = new GameBoard(process.MainWindowHandle);
            board.FullScanWindow();

            WriteBoard(board);

            board.ClickFace();
            Console.ReadLine();
        }

        private static void WriteBoard(GameBoard board)
        {
            Console.WriteLine(board.Face);

            var gridWidth = board.Grids.GetLength(0);
            var gridHeight = board.Grids.GetLength(1);
            for (int j = 0; j < gridHeight; j++)
            {
                for (int i = 0; i < gridWidth; i++)
                {
                    var gridValue = (int)board.Grids[i, j];
                    var gridText = gridValue.ToString();

                    Console.Write(gridText.PadLeft(totalWidth: 3));
                }

                Console.WriteLine();
            }
        }
    }
}
