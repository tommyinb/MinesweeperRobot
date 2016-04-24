using MinesweeperRobot.Icons;
using MinesweeperRobot.Strategy;
using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                ConsoleUtil.WriteLine(ConsoleColor.Cyan, "Pause the robot by moving your mouse whenever you want to.");

                try
                {
                    new Program().Main();
                }
                catch (GameScanException)
                {
                    Console.WriteLine("Cannot scan game.");
                    Console.WriteLine("Retry after 5 seconds...");
                    logWriter.Flush();
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
                catch (GamePauseException e)
                {
                    ConsoleUtil.WriteLine(ConsoleColor.Magenta, e.Message);
                    Console.WriteLine("Stop game.");
                    ConsoleUtil.WriteLine(ConsoleColor.Cyan, "Press enter to resume game...");
                    logWriter.Flush();
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
                ConsoleUtil.WriteLine(ConsoleColor.Green, "Start a new one.");

                var gameFile = "Winmine__XP.exe";
                if (File.Exists(gameFile) == false)
                {
                    Console.WriteLine("Cannot find " + gameFile + ".");
                    ConsoleUtil.WriteLine(ConsoleColor.Green, "Create a new one.");

                    var resourceName = typeof(Program).Namespace + "." + gameFile;
                    using (var resourceStream = typeof(Program).Assembly.GetManifestResourceStream(resourceName))
                    using (var fileStream = File.OpenWrite(gameFile))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }

                process = Process.Start(gameFile);
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
                        ConsoleUtil.WriteLine(ConsoleColor.Yellow, "Win game!");

                        var winFacePoint = board.FaceRectangle.Center();
                        Cursor.Position = winFacePoint;
                        lastCursor = winFacePoint;
                        ConsoleUtil.WriteLine(ConsoleColor.Cyan, "Click the smart face for another game.");

                        WaitFaceChange(board, waitFace: Face.Win);
                        break;

                    case Face.Lose:
                        Console.WriteLine("Lose game. Retry.");
                        lastCursor = board.ClickFace();
                        Thread.Sleep(1);

                        WaitFaceChange(board, waitFace: Face.Lose);
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }
        private void WaitFaceChange(GameBoard board, Face waitFace)
        {
            while (board.Face == waitFace)
            {
                board.CaptureWindow();
                board.QuickScanFace();

                Thread.Sleep(10);
            }
        }
        
        private void RunGame(GameBoard gameBoard)
        {
            gameBoard.QuickScanGrids();
            gameBoard.QuickScanBombCount();
            //WriteGrids(gameBoard.Grids);

            var guesses = IterateStrategies(gameBoard.Grids, gameBoard.BombCount).ToArray();
            if (guesses.Any() == false) throw new GamePauseException("No strategies can be used.");
            ApplyGuess(gameBoard, guesses);
        }
        private IEnumerable<GuessGrid> IterateStrategies(Grid[,] grids, int bombCount)
        {
            var currBoard = new StrategyBoard(grids, bombCount);
            var currGuesses = RunStrategies(currBoard);
            var currConfirmeds = currGuesses.Where(t => t.Confidence >= 1).ToArray();

            var currBombs = currConfirmeds.Where(t => t.Value == GuessValue.Bomb).Select(t => t.Point).Distinct().ToArray();
            if (currBombs.Any() == false) return currGuesses;

            Console.WriteLine("Iterate strategies");

            var nextGrids = (Grid[,])grids.Clone();
            foreach (var currBomb in currBombs)
            {
                var nextGridValue = nextGrids[currBomb.X, currBomb.Y];
                if (nextGridValue != Grid.Raw) throw new InvalidOperationException();

                nextGrids[currBomb.X, currBomb.Y] = Grid.Flag;
            }
            var nextBombCount = bombCount - currBombs.Count();
            var nextGuesses = IterateStrategies(nextGrids, nextBombCount).ToArray();

            var nextConfirmeds = nextGuesses.Where(t => t.Confidence >= 1);
            if (nextConfirmeds.Any() == false) return currGuesses;

            return currConfirmeds.Concat(nextConfirmeds);
        }
        private IEnumerable<GuessGrid> RunStrategies(StrategyBoard board)
        {
            var progress = 1 - (float)board.RawCount / (board.Size.Width * board.Size.Height);

            if (progress >= .1)
            {
                var countStrategy = new CountStrategy(board);
                Console.WriteLine("Run count strategy");

                var countGuesses = countStrategy.Guess().ToArray();
                if (countGuesses.Any())
                {
                    return countGuesses;
                }
            }

            if (progress >= .4)
            {
                var bruteForceStrategy = new BruteForceStrategy(board, progress < .7 ? 5 : 8);
                Console.WriteLine("Run brute-force strategy");

                var bruteForceGuesses = bruteForceStrategy.Guess().ToArray();
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

                var startStrategy = new RandomStrategy(board);
                Console.WriteLine("Run start strategy");

                var startGuesses = startStrategy.Guess().Take(startCount).ToArray();
                if (startGuesses.Any())
                {
                    return startGuesses;
                }
            }

            var randomStrategy = new RandomStrategy(board);
            Console.WriteLine("Run random strategy");

            var randomGuesses = randomStrategy.Guess();
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
                throw new GamePauseException("Mouse moved.");
            }
        }

        private void WriteGrids(Grid[,] grids)
        {
            var size = grids.GetSize();
            for (int j = 0; j < size.Height; j++)
            {
                for (int i = 0; i < size.Width; i++)
                {
                    var grid = grids[i, j];
                    switch (grid)
                    {
                        case Grid.Raw: Console.Write("x"); break;
                        case Grid.Empty: Console.Write(" "); break;
                        case Grid.Number1: Console.Write(1); break;
                        case Grid.Number2: Console.Write(2); break;
                        case Grid.Number3: Console.Write(3); break;
                        case Grid.Number4: Console.Write(4); break;
                        case Grid.Number5: Console.Write(5); break;
                        case Grid.Number6: Console.Write(6); break;
                        case Grid.Number7: Console.Write(7); break;
                        case Grid.Number8: Console.Write(8); break;
                        case Grid.Flag: Console.Write("F"); break;
                        case Grid.Question: Console.Write("?"); break;
                        case Grid.Bomb: Console.Write("B"); break;
                        default: throw new ArgumentException();
                    }
                }

                Console.WriteLine();
            }
        }
    }
}
