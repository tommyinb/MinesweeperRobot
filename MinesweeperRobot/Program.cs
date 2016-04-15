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
            var logWriter = new LogWriter(Console.Out, "console.log");
            Console.SetOut(logWriter);

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
            Console.WriteLine("Start game...");
            var board = StartGame();

            Console.WriteLine("Full scan...");
            board.CaptureWindow();
            board.FullScanWindow();

            while (true)
            {
                board.CaptureWindow();
                RunFace(board);
                RunGame(board);

                CheckCursor(board);
                Thread.Sleep(1);
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

            var foregroundWindow = Window.GetForegroundWindow();
            if (foregroundWindow != process.MainWindowHandle)
            {
                Console.WriteLine("Bring minesweeper to foreground.");
                Window.SetForegroundWindow(process.MainWindowHandle);
                Thread.Sleep(1000);
            }

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
            board.QuickScanFace();

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
                            board.CaptureWindow();
                            board.QuickScanFace();
                        }
                        break;

                    case Face.Lose:
                        Console.WriteLine("Lose game. Retry.");
                        board.ClickFace();
                        Thread.Sleep(1);

                        while (board.Face == Face.Lose)
                        {
                            board.CaptureWindow();
                            board.QuickScanFace();
                        }
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private void RunGame(GameBoard gameBoard)
        {
            gameBoard.QuickScanGrids();

            var strategyBoard = new StrategyBoard(gameBoard.Grids, 99);
            var strategies = GetStrategies(strategyBoard);
            var guesses = RunStrategies(strategyBoard, strategies);

            ApplyGuess(gameBoard, guesses);
        }
        private IEnumerable<IStrategy> GetStrategies(StrategyBoard board)
        {
            var rawAmount = (float)board.RawCount / (board.Size.Width * board.Size.Height);

            if (rawAmount < .9)
            {
                yield return new CountStrategy();
            }
            if (rawAmount < .6)
            {
                yield return new BruteForceStrategy();
            }

            yield return new RandomStrategy();
        }
        private IEnumerable<GuessGrid> RunStrategies(StrategyBoard board, IEnumerable<IStrategy> strategies)
        {
            var possibleGuesses = new List<GuessGrid>();
            foreach (var strategy in strategies)
            {
                Console.WriteLine("Run " + strategy.GetType().Name);

                var strategyGuesses = strategy.Guess(board).ToArray();

                var confirmedGuesses = strategyGuesses.Where(t => t.Confidence >= 1).ToArray();
                if (confirmedGuesses.Any())
                {
                    return confirmedGuesses;
                }

                possibleGuesses.AddRange(strategyGuesses);
            }

            var possibleEmpties = possibleGuesses.Where(t => t.Value == GuessValue.Empty);
            var bestPossibleEmpty = possibleEmpties.OrderByDescending(t => t.Confidence).First();
            return new[] { bestPossibleEmpty };
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
                        lastCursor = gameBoard.CheckGrid(guess.Point);
                        break;

                    case GuessValue.Bomb:
                        Console.WriteLine("Flag (" + guess.Point.X + ", " + guess.Point.Y + ") "
                            + "with confidence " + guess.Confidence.ToString("0.0"));
                        lastCursor = gameBoard.FlagGrid(guess.Point);
                        break;

                    default:
                        throw new ArgumentException();
                }
            }
        }
        private Point lastCursor = Cursor.Position;
        private void CheckCursor(GameBoard board)
        {
            if (Cursor.Position != lastCursor)
            {
                Console.WriteLine("Cursor moved.");
                Console.WriteLine("Stop game.");

                Console.WriteLine("Please enter to resume game...");
                Console.ReadLine();

                Window.SetForegroundWindow(board.WindowHandle);
                Thread.Sleep(1000);
            }
        }
    }
}
