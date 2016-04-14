using MinesweeperRobot.Icons;
using MinesweeperRobot.Strategy;
using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinesweeperRobot
{
    public class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    new Program().Main();
                }
                catch (GameScanException)
                {
                    Console.WriteLine("Cannot scan game.");
                    Console.WriteLine("Retry after 5 seconds...");
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
        }
        private void Main()
        {
            var board = StartGame();
            board.FullScanWindow();

            while (true)
            {
                using (TimeLog.Stopwatch("Run Face"))
                {
                    RunFace(board);
                }

                using (TimeLog.Stopwatch("Run Game"))
                {
                    RunGame(board);
                }
            }
        }
        
        private GameBoard StartGame()
        {
            var processes = Process.GetProcesses();
            var process = processes.FirstOrDefault(t => t.MainWindowTitle == "Minesweeper");
            if (process == null)
            {
                Console.WriteLine("Cannot find minesweeper.");
                Console.WriteLine("Start a new one.");

                process = Process.Start("Winmine__XP.exe");
                process.WaitForInputIdle();
                Thread.Sleep(1000);
            }

            Console.WriteLine("Start Game...");
            return new GameBoard(process.MainWindowHandle);
        }
        private void WriteGame(GameBoard board)
        {
            Console.WriteLine(board.Face);

            var gridWidth = board.Grids.GetLength(0);
            var gridHeight = board.Grids.GetLength(1);
            for (int j = 0; j < gridHeight; j++)
            {
                for (int i = 0; i < gridWidth; i++)
                {
                    var gridValue = board.Grids[i, j];
                    switch (gridValue)
                    {
                        case Grid.None: Console.Write("N"); break;
                        case Grid.Raw: Console.Write("+"); break;
                        case Grid.Empty: Console.Write("+"); break;
                        case Grid.Number1: Console.Write(1); break;
                        case Grid.Number2: Console.Write(2); break;
                        case Grid.Number3: Console.Write(3); break;
                        case Grid.Number4: Console.Write(4); break;
                        case Grid.Number5: Console.Write(5); break;
                        case Grid.Number6: Console.Write(6); break;
                        case Grid.Number7: Console.Write(7); break;
                        case Grid.Number8: Console.Write(8); break;
                        case Grid.Flag: Console.Write("F"); break;
                        case Grid.Bomb: Console.Write("B"); break;
                        default: Console.Write("#"); break;
                    }
                }

                Console.WriteLine();
            }
        }

        private void RunFace(GameBoard board)
        {
            using (TimeLog.Stopwatch("Scan Face"))
            {
                board.QuickScanFace();
            }

            while (true)
            {
                switch (board.Face)
                {
                    case Face.Normal:
                        Console.WriteLine("Game normal.");
                        return;

                    case Face.Win:
                        Console.WriteLine("Win game!");

                        Cursor.Position = board.FaceRectangle.Center();
                        Console.WriteLine("Click the smart face for another game.");

                        while (board.Face == Face.Win)
                        {
                            using (TimeLog.Stopwatch("Scan Face"))
                            {
                                board.QuickScanFace();
                            }
                        }
                        break;

                    case Face.Lose:
                        Console.WriteLine("Lose game. Retry.");
                        board.ClickFace();
                        Thread.Sleep(1);

                        while (board.Face == Face.Lose)
                        {
                            using (TimeLog.Stopwatch("Scan Face"))
                            {
                                board.QuickScanFace();
                            }
                        }
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private void RunGame(GameBoard gameBoard)
        {
            using (TimeLog.Stopwatch("Scan Grids"))
            {
                gameBoard.QuickScanGrids();
            }

            var strategyBoard = new StrategyBoard(gameBoard.Grids, 99);

            var strategies = new IStrategy[]
            {
                new LocalRatioStrategy(),
                new JoinRatioStrategy(),
                new GlobalRatioStrategy()
            };

            var possibleGuesses = new List<GuessGrid>();
            foreach (var strategy in strategies)
            {
                Console.WriteLine("Run " + strategy.GetType().Name);

                var strategyGuesses = strategy.Guess(strategyBoard).ToArray();

                var confirmedGuesses = strategyGuesses.Where(t => t.Confidence >= 1).ToArray();
                if (confirmedGuesses.Any())
                {
                    ApplyGuess(gameBoard, confirmedGuesses);
                    return;
                }

                possibleGuesses.AddRange(strategyGuesses);
            }

            var possibleEmpties = possibleGuesses.Where(t => t.Value == GuessValue.Empty);
            var bestPossibleEmpty = possibleEmpties.OrderByDescending(t => t.Confidence).First();
            ApplyGuess(gameBoard, new[] { bestPossibleEmpty });
        }
        private void ApplyGuess(GameBoard gameBoard, IEnumerable<GuessGrid> guesses)
        {
            var guessGroups = guesses.GroupBy(t => t.Point);
            foreach (var guessGroup in guessGroups)
            {
                var guess = guessGroup.OrderByDescending(t => t.Confidence).First();
                switch (guess.Value)
                {
                    case GuessValue.Empty:
                        Console.WriteLine("Check (" + guess.Point.X + ", " + guess.Point.Y + ") "
                            + "with confidence " + guess.Confidence.ToString("0.0"));
                        gameBoard.CheckGrid(guess.Point);
                        break;

                    case GuessValue.Bomb:
                        Console.WriteLine("Flag (" + guess.Point.X + ", " + guess.Point.Y + ") "
                            + "with confidence " + guess.Confidence.ToString("0.0"));
                        gameBoard.FlagGrid(guess.Point);
                        break;

                    default:
                        throw new ArgumentException();
                }
            }
        }
    }
}
