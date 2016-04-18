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
                catch (GamePauseException)
                {
                    Console.WriteLine("Stop game.");
                    Console.WriteLine("Press enter to resume game...");
                    Console.ReadLine();
                    Thread.Sleep(1000);
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
            lastCursor = Cursor.Position;

            while (true)
            {
                TryUtil.Invoke<GameScanException>(() =>
                {
                    board.CaptureWindow();

                    RunFace(board);
                    RunGame(board);
                }, numberOfTry: 3);

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

            Console.WriteLine("Preparing...");
            return new GameBoard(process.MainWindowHandle);
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
                        lastCursor = board.ClickFace();
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
            gameBoard.QuickScanBombCount();

            var strategyBoard = new StrategyBoard(gameBoard.Grids, gameBoard.BombCount);
            var guesses = RunStrategies(strategyBoard);

            ApplyGuess(gameBoard, guesses);
        }
        private IEnumerable<GuessGrid> RunStrategies(StrategyBoard board)
        {
            var progress = 1 - (float)board.RawCount / (board.Size.Width * board.Size.Height);

            if (progress >= .1)
            {
                var countStrategy = new CountStrategy();
                Console.WriteLine("Run count strategy");

                var countGuesses = countStrategy.Guess(board).ToArray();
                if (countGuesses.Any())
                {
                    return countGuesses;
                }
            }

            if (progress >= .4)
            {
                var bruteForceStrategy = new BruteForceStrategy();
                Console.WriteLine("Run brute-force strategy");

                var bruteForceGuesses = bruteForceStrategy.Guess(board).ToArray();
                if (bruteForceGuesses.Any())
                {
                    var confirmedBruteForceGuesses = bruteForceGuesses.Where(t => t.Confidence >= 1).ToArray();
                    if (confirmedBruteForceGuesses.Any())
                    {
                        return confirmedBruteForceGuesses;
                    }

                    var probableBruteForceGuesses = bruteForceGuesses.OrderByDescending(t => t.Confidence).First();
                    return new[] { probableBruteForceGuesses };
                }
            }

            if (progress <= .1)
            {
                var startCount = (int)((float)board.RawCount / board.BombCount * 3);

                var startStrategy = new RandomStrategy();
                Console.WriteLine("Run start strategy");

                var startGuesses = startStrategy.Guess(board).Take(startCount).ToArray();
                if (startGuesses.Any())
                {
                    return startGuesses;
                }
            }

            var randomStrategy = new RandomStrategy();
            Console.WriteLine("Run random strategy");

            var randomGuesses = randomStrategy.Guess(board);
            if (randomGuesses.Any())
            {
                var probableRandomGuesses = randomGuesses.OrderByDescending(t => t.Confidence).First();
                return new[] { probableRandomGuesses };
            }

            return new GuessGrid[0];
        }
        private void ApplyGuess(GameBoard gameBoard, IEnumerable<GuessGrid> guesses)
        {
            var guessGroups = guesses.GroupBy(t => t.Point);
            foreach (var guessGroup in guessGroups)
            {
                CheckCursor(gameBoard);

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

                throw new GamePauseException();
            }
        }
    }
}
